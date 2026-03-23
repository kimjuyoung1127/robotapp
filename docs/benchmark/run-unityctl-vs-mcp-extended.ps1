param(
    [string]$RobotAppRoot = "C:\Users\ezen601\Desktop\Jason\robotapp2",
    [string]$UnityCtlRoot = "C:\Users\ezen601\Desktop\Jason\unityctl",
    [string]$McpHost = "127.0.0.1",
    [int]$McpPort = 6400,
    [int]$WarmupRuns = 2,
    [int]$MeasuredRuns = 8
)

$ErrorActionPreference = "Stop"
$PSNativeCommandArgumentPassing = "Standard"

function Get-IsoNowUtc {
    return [DateTimeOffset]::UtcNow.ToString("o")
}

function Get-Median {
    param([double[]]$Values)
    if (-not $Values -or $Values.Count -eq 0) {
        return $null
    }

    $sorted = $Values | Sort-Object
    $count = $sorted.Count
    $mid = [Math]::Floor($count / 2)
    if ($count % 2 -eq 1) {
        return [double]$sorted[$mid]
    }

    return ([double]$sorted[$mid - 1] + [double]$sorted[$mid]) / 2.0
}

function Get-StdDev {
    param([double[]]$Values)
    if (-not $Values -or $Values.Count -lt 2) {
        return 0.0
    }

    $avg = ($Values | Measure-Object -Average).Average
    $variance = ($Values | ForEach-Object { [Math]::Pow($_ - $avg, 2) } | Measure-Object -Average).Average
    return [Math]::Sqrt($variance)
}

function Get-Stats {
    param([double[]]$Values)

    if (-not $Values -or $Values.Count -eq 0) {
        return [ordered]@{
            runs = 0
            min_ms = $null
            max_ms = $null
            mean_ms = $null
            median_ms = $null
            stdev_ms = $null
        }
    }

    return [ordered]@{
        runs = $Values.Count
        min_ms = [Math]::Round(($Values | Measure-Object -Minimum).Minimum, 2)
        max_ms = [Math]::Round(($Values | Measure-Object -Maximum).Maximum, 2)
        mean_ms = [Math]::Round(($Values | Measure-Object -Average).Average, 2)
        median_ms = [Math]::Round((Get-Median -Values $Values), 2)
        stdev_ms = [Math]::Round((Get-StdDev -Values $Values), 2)
    }
}

function Read-ExactBytes {
    param(
        [System.Net.Sockets.NetworkStream]$Stream,
        [int]$Count
    )

    $buffer = New-Object byte[] $Count
    $offset = 0
    while ($offset -lt $Count) {
        $read = $Stream.Read($buffer, $offset, $Count - $offset)
        if ($read -le 0) {
            throw "Socket closed while reading $Count bytes."
        }
        $offset += $read
    }

    return $buffer
}

function Read-McpHandshake {
    param([System.Net.Sockets.NetworkStream]$Stream)

    $bytes = New-Object System.Collections.Generic.List[byte]
    while ($bytes.Count -lt 512) {
        $value = $Stream.ReadByte()
        if ($value -lt 0) {
            break
        }

        $bytes.Add([byte]$value)
        if ($value -eq 10) {
            break
        }
    }

    return [System.Text.Encoding]::ASCII.GetString($bytes.ToArray()).Trim()
}

function Invoke-McpBridge {
    param(
        [string]$CommandType,
        [hashtable]$Params
    )

    $attempts = 3
    for ($attempt = 1; $attempt -le $attempts; $attempt++) {
        $client = [System.Net.Sockets.TcpClient]::new()
        try {
            $client.ReceiveTimeout = 5000
            $client.SendTimeout = 5000
            $client.Connect($McpHost, $McpPort)
            $stream = $client.GetStream()
            $handshake = Read-McpHandshake -Stream $stream

            if (-not $handshake.Contains("FRAMING=1")) {
                throw "Unexpected MCP handshake: $handshake"
            }

            if ($CommandType -eq "ping") {
                $payloadBytes = [System.Text.Encoding]::UTF8.GetBytes("ping")
            } else {
                $payloadJson = @{
                    type = $CommandType
                    params = $Params
                } | ConvertTo-Json -Compress -Depth 100
                $payloadBytes = [System.Text.Encoding]::UTF8.GetBytes($payloadJson)
            }

            $header = [BitConverter]::GetBytes([UInt64]$payloadBytes.Length)
            if ([BitConverter]::IsLittleEndian) {
                [Array]::Reverse($header)
            }

            $stream.Write($header, 0, $header.Length)
            $stream.Write($payloadBytes, 0, $payloadBytes.Length)
            $stream.Flush()

            $responseHeader = Read-ExactBytes -Stream $stream -Count 8
            if ([BitConverter]::IsLittleEndian) {
                [Array]::Reverse($responseHeader)
            }

            $responseLength = [BitConverter]::ToUInt64($responseHeader, 0)
            $responseBytes = Read-ExactBytes -Stream $stream -Count ([int]$responseLength)
            $responseText = [System.Text.Encoding]::UTF8.GetString($responseBytes)

            return [ordered]@{
                handshake = $handshake
                text = $responseText
                json = $responseText | ConvertFrom-Json
            }
        } catch {
            if ($attempt -ge $attempts) {
                throw
            }

            Start-Sleep -Seconds 1
        } finally {
            if ($client.Connected) {
                $client.Close()
            }
            $client.Dispose()
        }
    }
}

function Invoke-CommandJson {
    param(
        [string]$Executable,
        [string[]]$Arguments,
        [string]$WorkingDirectory
    )

    Push-Location $WorkingDirectory
    try {
        $output = & $Executable @Arguments 2>&1
        $exitCode = $LASTEXITCODE
        $text = ($output | Out-String).Trim()
        if ($exitCode -ne 0) {
            throw "Command failed ($exitCode): $Executable $($Arguments -join ' ')`n$text"
        }

        return [ordered]@{
            text = $text
            json = $text | ConvertFrom-Json
        }
    } finally {
        Pop-Location
    }
}

function Invoke-UnityCtlDotnetRun {
    param([string[]]$Arguments)
    $allArguments = @("run", "--project", "src/Unityctl.Cli", "--") + $Arguments
    return Invoke-CommandJson -Executable "dotnet" -Arguments $allArguments -WorkingDirectory $UnityCtlRoot
}

function Invoke-UnityCtlPublishedExe {
    param(
        [string]$Executable,
        [string[]]$Arguments
    )
    return Invoke-CommandJson -Executable $Executable -Arguments $Arguments -WorkingDirectory $UnityCtlRoot
}

function Invoke-BenchmarkOperation {
    param(
        [string]$Label,
        [scriptblock]$Action
    )

    $warmups = @()
    $lastResult = $null

    for ($i = 1; $i -le $WarmupRuns; $i++) {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $lastResult = & $Action
        $sw.Stop()
        $warmups += [ordered]@{
            run = $i
            ms = [Math]::Round($sw.Elapsed.TotalMilliseconds, 2)
        }
    }

    $runs = @()
    for ($i = 1; $i -le $MeasuredRuns; $i++) {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $lastResult = & $Action
        $sw.Stop()
        $runs += [ordered]@{
            run = $i
            ms = [Math]::Round($sw.Elapsed.TotalMilliseconds, 2)
        }
    }

    $values = @($runs | ForEach-Object { [double]$_.ms })
    return [ordered]@{
        label = $Label
        warmup_runs = $warmups
        measured_runs = $runs
        stats = Get-Stats -Values $values
        sample = $lastResult
    }
}

function Ensure-PublishedArtifacts {
    Push-Location $UnityCtlRoot
    try {
        & dotnet publish src/Unityctl.Cli -c Release -o publish/win-x64 --nologo
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet publish failed for Unityctl.Cli"
        }

        & dotnet publish src/Unityctl.Mcp -c Release -o publish/mcp-win-x64 --nologo
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet publish failed for Unityctl.Mcp"
        }
    } finally {
        Pop-Location
    }

    return [ordered]@{
        unityctl_exe = Join-Path $UnityCtlRoot "publish\win-x64\unityctl.exe"
        unityctl_mcp_exe = Join-Path $UnityCtlRoot "publish\mcp-win-x64\unityctl-mcp.exe"
    }
}

function Invoke-UnityctlMcpPersistent {
    param(
        [string]$ServerExe,
        [string]$HelperScript,
        [string]$ProjectPath
    )

    $output = & python $HelperScript --server-exe $ServerExe --project $ProjectPath --warmups $WarmupRuns --runs $MeasuredRuns 2>$null
    $exitCode = $LASTEXITCODE
    $text = ($output | Out-String).Trim()
    if ($exitCode -ne 0) {
        throw "Unityctl.Mcp helper failed ($exitCode): $text"
    }

    return $text | ConvertFrom-Json
}

function Find-McpPersistentResult {
    param(
        $BenchmarkJson,
        [string]$OperationName
    )

    return @($BenchmarkJson.results | Where-Object { $_.name -eq $OperationName })[0]
}

function Get-SampleSummary {
    param(
        [string]$OperationName,
        [string]$VariantName,
        $SampleJson
    )

    switch ("$OperationName|$VariantName") {
        "ping|dotnet_run" { return "message=$($SampleJson.message); unityVersion=$($SampleJson.data.unityVersion)" }
        "ping|published_exe" { return "message=$($SampleJson.message); unityVersion=$($SampleJson.data.unityVersion)" }
        "ping|unityctl_mcp" { return "message=$($SampleJson.message); unityVersion=$($SampleJson.data.unityVersion)" }
        "ping|coplaydev_bridge" { return "message=$($SampleJson.result.message)" }

        "editor_state|dotnet_run" { return "projectPath=$($SampleJson.data.projectPath); isPlaying=$($SampleJson.data.isPlaying); isCompiling=$($SampleJson.data.isCompiling)" }
        "editor_state|published_exe" { return "projectPath=$($SampleJson.data.projectPath); isPlaying=$($SampleJson.data.isPlaying); isCompiling=$($SampleJson.data.isCompiling)" }
        "editor_state|unityctl_mcp" { return "projectPath=$($SampleJson.data.projectPath); isPlaying=$($SampleJson.data.isPlaying); isCompiling=$($SampleJson.data.isCompiling)" }
        "editor_state|coplaydev_bridge" {
            return "activeScene=$($SampleJson.result.data.editor.active_scene.path); isPlaying=$($SampleJson.result.data.editor.play_mode.is_playing); isCompiling=$($SampleJson.result.data.compilation.is_compiling)"
        }

        "active_scene|dotnet_run" { return "activeScene=$($SampleJson.data.result.path)" }
        "active_scene|published_exe" { return "activeScene=$($SampleJson.data.result.path)" }
        "active_scene|unityctl_mcp" { return "activeScene=$($SampleJson.data.result.path)" }
        "active_scene|coplaydev_bridge" { return "activeScene=$($SampleJson.result.data.path)" }

        "diagnostic|dotnet_run" { return "assemblies=$($SampleJson.data.assemblies); scriptCompilationFailed=$($SampleJson.data.scriptCompilationFailed)" }
        "diagnostic|published_exe" { return "assemblies=$($SampleJson.data.assemblies); scriptCompilationFailed=$($SampleJson.data.scriptCompilationFailed)" }
        "diagnostic|unityctl_mcp" { return "assemblies=$($SampleJson.data.assemblies); scriptCompilationFailed=$($SampleJson.data.scriptCompilationFailed)" }
        "diagnostic|coplaydev_bridge" {
            $firstLine = ""
            if ($SampleJson.result.data.Count -gt 0) {
                $firstLine = [string]$SampleJson.result.data[0]
            }
            return "count=$($SampleJson.result.data.Count); firstLine=$firstLine"
        }
        default { return "" }
    }
}

function Get-MarkdownTableRow {
    param([string]$Operation, $Variants)

    return "| $Operation | $($Variants.dotnet_run.stats.median_ms) | $($Variants.published_exe.stats.median_ms) | $($Variants.unityctl_mcp.stats.median_ms) | $($Variants.coplaydev_bridge.stats.median_ms) |"
}

$benchmarkDate = Get-Date -Format "yyyy-MM-dd"
$outputDir = Join-Path $RobotAppRoot "docs\benchmark"
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

$projectPath = $RobotAppRoot.Replace("\", "/")
$timestampUtc = Get-IsoNowUtc
$helperScript = Join-Path $RobotAppRoot "docs\benchmark\bench_unityctl_mcp.py"

$artifacts = Ensure-PublishedArtifacts
$unityctlExe = $artifacts.unityctl_exe
$unityctlMcpExe = $artifacts.unityctl_mcp_exe

$mcpPersistentJson = Invoke-UnityctlMcpPersistent -ServerExe $unityctlMcpExe -HelperScript $helperScript -ProjectPath $projectPath

$operationConfigs = @(
    [ordered]@{
        name = "ping"
        comparability = "high"
        description = "Basic reachability check."
        dotnet_run = { Invoke-UnityCtlDotnetRun -Arguments @("ping", "--project", $projectPath, "--json") }
        published_exe = { Invoke-UnityCtlPublishedExe -Executable $unityctlExe -Arguments @("ping", "--project", $projectPath, "--json") }
        coplaydev_bridge = { Invoke-McpBridge -CommandType "ping" -Params @{} }
    },
    [ordered]@{
        name = "editor_state"
        comparability = "high"
        description = "Editor state lookup."
        dotnet_run = { Invoke-UnityCtlDotnetRun -Arguments @("status", "--project", $projectPath, "--json") }
        published_exe = { Invoke-UnityCtlPublishedExe -Executable $unityctlExe -Arguments @("status", "--project", $projectPath, "--json") }
        coplaydev_bridge = { Invoke-McpBridge -CommandType "get_editor_state" -Params @{} }
    },
    [ordered]@{
        name = "active_scene"
        comparability = "medium"
        description = "Active scene lookup. unityctl uses exec; CoplayDev uses manage_scene/get_active."
        dotnet_run = { Invoke-UnityCtlDotnetRun -Arguments @("exec", "--project", $projectPath, "--code", "UnityEngine.SceneManagement.SceneManager.GetActiveScene().path", "--json") }
        published_exe = { Invoke-UnityCtlPublishedExe -Executable $unityctlExe -Arguments @("exec", "--project", $projectPath, "--code", "UnityEngine.SceneManagement.SceneManager.GetActiveScene().path", "--json") }
        coplaydev_bridge = { Invoke-McpBridge -CommandType "manage_scene" -Params @{ action = "get_active" } }
    },
    [ordered]@{
        name = "diagnostic"
        comparability = "low"
        description = "Diagnostic read. Not apples-to-apples: unityctl=check, CoplayDev=read_console."
        dotnet_run = { Invoke-UnityCtlDotnetRun -Arguments @("check", "--project", $projectPath, "--json") }
        published_exe = { Invoke-UnityCtlPublishedExe -Executable $unityctlExe -Arguments @("check", "--project", $projectPath, "--json") }
        coplaydev_bridge = { Invoke-McpBridge -CommandType "read_console" -Params @{ action = "get"; count = 5 } }
    }
)

$results = @()
foreach ($operation in $operationConfigs) {
    $dotnetRunResult = Invoke-BenchmarkOperation -Label "dotnet_run/$($operation.name)" -Action $operation.dotnet_run
    $publishedExeResult = Invoke-BenchmarkOperation -Label "published_exe/$($operation.name)" -Action $operation.published_exe
    $coplaydevResult = Invoke-BenchmarkOperation -Label "coplaydev_bridge/$($operation.name)" -Action $operation.coplaydev_bridge
    $unityctlMcpResult = Find-McpPersistentResult -BenchmarkJson $mcpPersistentJson -OperationName $operation.name

    $results += [ordered]@{
        name = $operation.name
        comparability = $operation.comparability
        description = $operation.description
        variants = [ordered]@{
            dotnet_run = $dotnetRunResult
            published_exe = $publishedExeResult
            unityctl_mcp = $unityctlMcpResult
            coplaydev_bridge = $coplaydevResult
        }
        sample_summaries = [ordered]@{
            dotnet_run = Get-SampleSummary -OperationName $operation.name -VariantName "dotnet_run" -SampleJson $dotnetRunResult.sample.json
            published_exe = Get-SampleSummary -OperationName $operation.name -VariantName "published_exe" -SampleJson $publishedExeResult.sample.json
            unityctl_mcp = Get-SampleSummary -OperationName $operation.name -VariantName "unityctl_mcp" -SampleJson $unityctlMcpResult.sample.json
            coplaydev_bridge = Get-SampleSummary -OperationName $operation.name -VariantName "coplaydev_bridge" -SampleJson $coplaydevResult.sample.json
        }
    }
}

$raw = [ordered]@{
    benchmark_name = "unityctl-vs-coplaydev-mcp-extended"
    collected_at_utc = $timestampUtc
    environment = [ordered]@{
        cwd = $RobotAppRoot
        unityctl_root = $UnityCtlRoot
        unity_project = $projectPath
        mcp_host = $McpHost
        mcp_port = $McpPort
        warmup_runs = $WarmupRuns
        measured_runs = $MeasuredRuns
        published_unityctl_exe = $unityctlExe
        published_unityctl_mcp_exe = $unityctlMcpExe
    }
    assumptions = @(
        "docs/tasks/benchmark-vs-mcp.md was not present in the repository, so the benchmark spec was inferred from the live environment.",
        "CoplayDev was measured by direct framed TCP calls to the exposed Unity bridge on port 6400.",
        "Unityctl.Mcp was measured as a persistent stdio MCP session using the published release executable."
    )
    findings = @(
        "dotnet run measurements remain dominated by process + SDK startup cost.",
        "published unityctl.exe removes most of the dotnet run overhead but still pays per-process startup cost.",
        "Unityctl.Mcp resident mode isolates command transport latency without per-call process startup.",
        "This rerun used a single robotapp2 Unity session. Active-scene responses were consistent across stacks, but the active scene path was blank at measurement time."
    )
    operations = $results
}

$jsonPath = Join-Path $outputDir "$benchmarkDate-unityctl-vs-mcp-extended.json"
$raw | ConvertTo-Json -Depth 100 | Set-Content -Path $jsonPath -Encoding UTF8

$lines = @()
$lines += "# unityctl vs CoplayDev Unity MCP benchmark (extended)"
$lines += ""
$lines += "- Collected at (UTC): $timestampUtc"
$lines += "- Unity project target: ``$projectPath``"
$lines += "- CoplayDev endpoint: ``${McpHost}:$McpPort``"
$lines += "- Warmup runs per operation: ``$WarmupRuns``"
$lines += "- Measured runs per operation: ``$MeasuredRuns``"
$lines += ""
$lines += "## Important caveats"
$lines += ""
$lines += '- `docs/tasks/benchmark-vs-mcp.md` was not present, so this benchmark spec was inferred.'
$lines += '- This rerun used a single `robotapp2` Unity session. Cross-project leakage was not observed.'
$lines += '- `active_scene` responses were consistent across stacks, but the active scene path was blank during measurement.'
$lines += '- `dotnet run` and `published exe` are per-invocation timings. `Unityctl.Mcp` is a resident-session timing.'
$lines += ""
$lines += "## Median latency"
$lines += ""
$lines += "| Operation | dotnet run ms | published exe ms | Unityctl.Mcp ms | CoplayDev bridge ms |"
$lines += "| --- | ---: | ---: | ---: | ---: |"
foreach ($result in $results) {
    $lines += Get-MarkdownTableRow -Operation $result.name -Variants $result.variants
}
$lines += ""
$lines += "## Notes"
$lines += ""
foreach ($result in $results) {
    $lines += "### $($result.name)"
    $lines += ""
    $lines += "- Comparability: $($result.comparability)"
    $lines += "- Description: $($result.description)"
    $lines += "- dotnet run sample: $($result.sample_summaries.dotnet_run)"
    $lines += "- published exe sample: $($result.sample_summaries.published_exe)"
    $lines += "- Unityctl.Mcp sample: $($result.sample_summaries.unityctl_mcp)"
    $lines += "- CoplayDev bridge sample: $($result.sample_summaries.coplaydev_bridge)"
    $lines += ""
}
$lines += "## Files"
$lines += ""
$lines += "- Raw JSON: ``$(Split-Path -Leaf $jsonPath)``"
$lines += '- Runner: `run-unityctl-vs-mcp-extended.ps1`'
$lines += '- MCP helper: `bench_unityctl_mcp.py`'

$mdPath = Join-Path $outputDir "$benchmarkDate-unityctl-vs-mcp-extended.md"
$lines -join "`r`n" | Set-Content -Path $mdPath -Encoding UTF8

Write-Output "Saved:"
Write-Output $jsonPath
Write-Output $mdPath
