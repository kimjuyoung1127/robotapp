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
            runs   = 0
            min_ms = $null
            max_ms = $null
            mean_ms = $null
            median_ms = $null
            stdev_ms = $null
        }
    }

    return [ordered]@{
        runs      = $Values.Count
        min_ms    = [Math]::Round(($Values | Measure-Object -Minimum).Minimum, 2)
        max_ms    = [Math]::Round(($Values | Measure-Object -Maximum).Maximum, 2)
        mean_ms   = [Math]::Round(($Values | Measure-Object -Average).Average, 2)
        median_ms = [Math]::Round((Get-Median -Values $Values), 2)
        stdev_ms  = [Math]::Round((Get-StdDev -Values $Values), 2)
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
                type   = $CommandType
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
            text      = $responseText
            json      = $responseText | ConvertFrom-Json
        }
    } finally {
        if ($client.Connected) {
            $client.Close()
        }
        $client.Dispose()
    }
}

function Invoke-UnityCtl {
    param(
        [string[]]$Arguments
    )

    Push-Location $UnityCtlRoot
    try {
        $output = & dotnet run --project src/Unityctl.Cli -- @Arguments 2>&1
        $exitCode = $LASTEXITCODE
        $text = ($output | Out-String).Trim()
        if ($exitCode -ne 0) {
            throw "unityctl command failed ($exitCode): $text"
        }

        return [ordered]@{
            text = $text
            json = $text | ConvertFrom-Json
        }
    } finally {
        Pop-Location
    }
}

function Invoke-BenchmarkOperation {
    param(
        [string]$Label,
        [scriptblock]$Action
    )

    $warmups = @()
    for ($i = 1; $i -le $WarmupRuns; $i++) {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $result = & $Action
        $sw.Stop()
        $warmups += [ordered]@{
            run = $i
            ms  = [Math]::Round($sw.Elapsed.TotalMilliseconds, 2)
        }
    }

    $runs = @()
    $lastResult = $result
    for ($i = 1; $i -le $MeasuredRuns; $i++) {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $lastResult = & $Action
        $sw.Stop()
        $runs += [ordered]@{
            run = $i
            ms  = [Math]::Round($sw.Elapsed.TotalMilliseconds, 2)
        }
    }

    $values = @($runs | ForEach-Object { [double]$_.ms })
    return [ordered]@{
        label         = $Label
        warmup_runs   = $warmups
        measured_runs = $runs
        stats         = Get-Stats -Values $values
        sample        = $lastResult
    }
}

function Get-SampleSummary {
    param(
        [string]$OperationName,
        [string]$ToolKind,
        $SampleJson
    )

    switch ("$OperationName|$ToolKind") {
        "ping|unityctl" {
            return "message=$($SampleJson.message); unityVersion=$($SampleJson.data.unityVersion)"
        }
        "ping|mcp" {
            return "message=$($SampleJson.result.message)"
        }
        "editor_state|unityctl" {
            return "projectPath=$($SampleJson.data.projectPath); isPlaying=$($SampleJson.data.isPlaying); isCompiling=$($SampleJson.data.isCompiling)"
        }
        "editor_state|mcp" {
            $activeScene = $SampleJson.result.data.editor.active_scene.path
            return "activeScene=$activeScene; isPlaying=$($SampleJson.result.data.editor.play_mode.is_playing); isCompiling=$($SampleJson.result.data.compilation.is_compiling)"
        }
        "active_scene|unityctl" {
            return "activeScene=$($SampleJson.data.result.path)"
        }
        "active_scene|mcp" {
            return "activeScene=$($SampleJson.result.data.path)"
        }
        "diagnostic|unityctl" {
            return "assemblies=$($SampleJson.data.assemblies); scriptCompilationFailed=$($SampleJson.data.scriptCompilationFailed)"
        }
        "diagnostic|mcp" {
            $firstLine = ""
            if ($SampleJson.result.data.Count -gt 0) {
                $firstLine = [string]$SampleJson.result.data[0]
            }
            return "count=$($SampleJson.result.data.Count); firstLine=$firstLine"
        }
        default {
            return ""
        }
    }
}

function Get-MarkdownTableRow {
    param(
        [string]$Operation,
        [hashtable]$UnityCtlResult,
        [hashtable]$McpResult
    )

    $uMedian = [double]$UnityCtlResult.stats.median_ms
    $mMedian = [double]$McpResult.stats.median_ms
    $ratio = if ($uMedian -gt 0) { [Math]::Round($mMedian / $uMedian, 4) } else { $null }

    return "| $Operation | $uMedian | $mMedian | $ratio |"
}

$benchmarkDate = Get-Date -Format "yyyy-MM-dd"
$outputDir = Join-Path $RobotAppRoot "docs\benchmark"
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

$projectPath = $RobotAppRoot.Replace("\", "/")
$timestampUtc = Get-IsoNowUtc

$operations = @(
    [ordered]@{
        name          = "ping"
        comparability = "high"
        description   = "Basic reachability check."
        unityctl      = {
            Invoke-UnityCtl -Arguments @("ping", "--project", $projectPath, "--json")
        }
        mcp           = {
            Invoke-McpBridge -CommandType "ping" -Params @{}
        }
    },
    [ordered]@{
        name          = "editor_state"
        comparability = "high"
        description   = "Editor state lookup."
        unityctl      = {
            Invoke-UnityCtl -Arguments @("status", "--project", $projectPath, "--json")
        }
        mcp           = {
            Invoke-McpBridge -CommandType "get_editor_state" -Params @{}
        }
    },
    [ordered]@{
        name          = "active_scene"
        comparability = "medium"
        description   = "Active scene lookup. unityctl uses exec; MCP uses manage_scene/get_active."
        unityctl      = {
            Invoke-UnityCtl -Arguments @("exec", "--project", $projectPath, "--code", "UnityEngine.SceneManagement.SceneManager.GetActiveScene().path", "--json")
        }
        mcp           = {
            Invoke-McpBridge -CommandType "manage_scene" -Params @{ action = "get_active" }
        }
    },
    [ordered]@{
        name          = "diagnostic"
        comparability = "low"
        description   = "Diagnostic read. Not apples-to-apples: unityctl=check, MCP=read_console."
        unityctl      = {
            Invoke-UnityCtl -Arguments @("check", "--project", $projectPath, "--json")
        }
        mcp           = {
            Invoke-McpBridge -CommandType "read_console" -Params @{ action = "get"; count = 5 }
        }
    }
)

$results = @()
foreach ($operation in $operations) {
    $unityCtlResult = Invoke-BenchmarkOperation -Label "unityctl/$($operation.name)" -Action $operation.unityctl
    $mcpResult = Invoke-BenchmarkOperation -Label "mcp/$($operation.name)" -Action $operation.mcp

    $results += [ordered]@{
        name               = $operation.name
        comparability      = $operation.comparability
        description        = $operation.description
        unityctl           = $unityCtlResult
        mcp_bridge         = $mcpResult
        sample_summaries   = [ordered]@{
            unityctl = Get-SampleSummary -OperationName $operation.name -ToolKind "unityctl" -SampleJson $unityCtlResult.sample.json
            mcp      = Get-SampleSummary -OperationName $operation.name -ToolKind "mcp" -SampleJson $mcpResult.sample.json
        }
    }
}

$raw = [ordered]@{
    benchmark_name = "unityctl-vs-coplaydev-mcp-bridge"
    collected_at_utc = $timestampUtc
    environment = [ordered]@{
        cwd = $RobotAppRoot
        unityctl_root = $UnityCtlRoot
        unity_project = $projectPath
        mcp_host = $McpHost
        mcp_port = $McpPort
        warmup_runs = $WarmupRuns
        measured_runs = $MeasuredRuns
    }
    assumptions = @(
        "docs/tasks/benchmark-vs-mcp.md was not present in the repository, so the benchmark spec was inferred from the live environment.",
        "CoplayDev was measured by direct framed TCP calls to the exposed Unity bridge on port 6400, not by spawning an extra uvx stdio/http wrapper.",
        "Two Unity Editor processes were running. Some scene-oriented commands routed to 'C:/Users/ezen601/Desktop/Jason/My project' even though the bridge status files claimed robotapp2."
    )
    findings = @(
        "unityctl status/check stayed on robotapp2, but unityctl exec active-scene lookup returned My project.",
        "CoplayDev get_editor_state/manage_scene/read_console also returned My project content through port 6400.",
        "Because of the routing mismatch, only ping and editor-state latency should be treated as high-confidence transport comparisons. Active-scene is medium confidence, diagnostic is low confidence."
    )
    operations = $results
}

$jsonPath = Join-Path $outputDir "$benchmarkDate-unityctl-vs-mcp.json"
$raw | ConvertTo-Json -Depth 100 | Set-Content -Path $jsonPath -Encoding UTF8

$lines = @()
$lines += "# unityctl vs CoplayDev Unity MCP benchmark"
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
$lines += '- CoplayDev was measured by direct framed TCP calls to the exposed Unity bridge on port `6400`.'
$lines += '- Two Unity Editor processes were open. Scene-oriented reads on both stacks sometimes resolved to `C:/Users/ezen601/Desktop/Jason/My project` instead of `robotapp2`.'
$lines += ""
$lines += "## Median latency"
$lines += ""
$lines += "| Operation | unityctl median ms | MCP bridge median ms | MCP/unityctl ratio |"
$lines += "| --- | ---: | ---: | ---: |"
foreach ($result in $results) {
    $lines += Get-MarkdownTableRow -Operation $result.name -UnityCtlResult $result.unityctl -McpResult $result.mcp_bridge
}
$lines += ""
$lines += "## Notes"
$lines += ""
foreach ($result in $results) {
    $lines += "### $($result.name)"
    $lines += ""
    $lines += "- Comparability: $($result.comparability)"
    $lines += "- Description: $($result.description)"
    $lines += "- unityctl sample: $($result.sample_summaries.unityctl)"
    $lines += "- MCP sample: $($result.sample_summaries.mcp)"
    $lines += ""
}
$lines += "## Files"
$lines += ""
$lines += "- Raw JSON: ``$(Split-Path -Leaf $jsonPath)``"
$lines += '- Runner: `run-unityctl-vs-mcp.ps1`'

$mdPath = Join-Path $outputDir "$benchmarkDate-unityctl-vs-mcp.md"
$lines -join "`r`n" | Set-Content -Path $mdPath -Encoding UTF8

Write-Output "Saved:"
Write-Output $jsonPath
Write-Output $mdPath
