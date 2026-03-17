[CmdletBinding()]
param(
    [string]$OutputRoot = "TestResults/UnityCli",
    [int]$PollIntervalSeconds = 3,
    [int]$RunTestsTimeoutSeconds = 900,
    [string]$SnapshotName = "robotcontrol_baseline"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$PSNativeCommandArgumentPassing = "Standard"

function Get-AbsolutePath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return Join-Path (Get-Location).Path $Path
}

function Get-SafeName {
    param([Parameter(Mandatory = $true)][string]$Name)

    $safe = $Name -replace "[^A-Za-z0-9._-]", "-"
    return $safe.Trim("-")
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]$Value
    )

    $json = $Value | ConvertTo-Json -Depth 32
    Set-Content -Path $Path -Value $json -Encoding utf8
}

function Try-ParseJsonFromText {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $null
    }

    $trimmed = $Text.Trim()

    try {
        $parsed = $trimmed | ConvertFrom-Json -Depth 32
        return @{
            Parsed = $parsed
            JsonText = $trimmed
        }
    }
    catch {
    }

    $candidates = @(
        @{ Open = "{"; Close = "}" },
        @{ Open = "["; Close = "]" }
    )

    foreach ($candidate in $candidates) {
        $start = $trimmed.IndexOf($candidate.Open)
        $end = $trimmed.LastIndexOf($candidate.Close)
        if ($start -lt 0 -or $end -le $start) {
            continue
        }

        $jsonText = $trimmed.Substring($start, $end - $start + 1)
        try {
            $parsed = $jsonText | ConvertFrom-Json -Depth 32
            return @{
                Parsed = $parsed
                JsonText = $jsonText
            }
        }
        catch {
        }
    }

    return $null
}

function Get-PropertyValue {
    param(
        $Object,
        [Parameter(Mandatory = $true)][string]$Name
    )

    if ($null -eq $Object) {
        return $null
    }

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function Get-ResponseData {
    param($Parsed)

    if ($null -eq $Parsed) {
        return $null
    }

    $data = Get-PropertyValue -Object $Parsed -Name "data"
    if ($null -ne $data) {
        return $data
    }

    return $Parsed
}

function New-AssertionResult {
    param(
        [Parameter(Mandatory = $true)][bool]$Pass,
        [Parameter(Mandatory = $true)][string]$Expected,
        [Parameter(Mandatory = $true)][string]$Actual,
        [string]$Severity = "error"
    )

    return @{
        Pass = $Pass
        Expected = $Expected
        Actual = $Actual
        Severity = $Severity
    }
}

function Test-NumericArrayFinite {
    param($Value)

    if ($null -eq $Value) {
        return $false
    }

    foreach ($item in $Value) {
        $number = 0.0
        if (-not [double]::TryParse($item.ToString(), [ref]$number)) {
            return $false
        }
        if ([double]::IsNaN($number) -or [double]::IsInfinity($number)) {
            return $false
        }
    }

    return $true
}

function Compare-CameraState {
    param(
        $CurrentData,
        $ExpectedData,
        [double]$Tolerance = 0.01
    )

    if ($null -eq $CurrentData -or $null -eq $ExpectedData) {
        return $false
    }

    $currentPos = Get-PropertyValue -Object $CurrentData -Name "position"
    $expectedPos = Get-PropertyValue -Object $ExpectedData -Name "position"
    $currentEuler = Get-PropertyValue -Object $CurrentData -Name "eulerAngles"
    $expectedEuler = Get-PropertyValue -Object $ExpectedData -Name "eulerAngles"

    $pairs = @(
        @{ Current = $currentPos; Expected = $expectedPos; Keys = @("x", "y", "z") },
        @{ Current = $currentEuler; Expected = $expectedEuler; Keys = @("x", "y", "z") }
    )

    foreach ($pair in $pairs) {
        foreach ($key in $pair.Keys) {
            $currentValue = [double](Get-PropertyValue -Object $pair.Current -Name $key)
            $expectedValue = [double](Get-PropertyValue -Object $pair.Expected -Name $key)
            if ([math]::Abs($currentValue - $expectedValue) -gt $Tolerance) {
                return $false
            }
        }
    }

    $currentFov = [double](Get-PropertyValue -Object $CurrentData -Name "fov")
    $expectedFov = [double](Get-PropertyValue -Object $ExpectedData -Name "fov")
    if ([math]::Abs($currentFov - $expectedFov) -gt $Tolerance) {
        return $false
    }

    return $true
}

function Get-EntryByKey {
    param(
        $Entries,
        [Parameter(Mandatory = $true)][string]$Key
    )

    if ($null -eq $Entries) {
        return $null
    }

    foreach ($entry in $Entries) {
        if ((Get-PropertyValue -Object $entry -Name "key") -eq $Key) {
            return $entry
        }
    }

    return $null
}

function Add-StepResult {
    param(
        [Parameter(Mandatory = $true)][pscustomobject]$Result,
        $Parsed
    )

    $script:Results.Add($Result) | Out-Null
    $script:LastParsed = $Parsed
}

function Invoke-UnityCliStep {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Phase,
        [Parameter(Mandatory = $true)][string[]]$Args,
        [string]$Scene = "",
        [string]$Mode = "",
        [scriptblock]$AssertScript
    )

    $script:StepCounter++
    $stepName = "{0:D3}-{1}" -f $script:StepCounter, (Get-SafeName -Name $Name)
    $stepDir = Join-Path $script:RunDir $stepName
    New-Item -ItemType Directory -Force -Path $stepDir | Out-Null

    $commandText = "unity-cli " + ($Args -join " ")
    $rawPath = Join-Path $stepDir "raw.txt"
    $jsonPath = Join-Path $stepDir "parsed.json"

    $rawOutput = ""
    $exitCode = 0

    try {
        $rawOutput = (& unity-cli @Args 2>&1 | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine
        $exitCode = $LASTEXITCODE
    }
    catch {
        $rawOutput = $_ | Out-String
        $exitCode = if ($LASTEXITCODE) { $LASTEXITCODE } else { 1 }
    }

    Set-Content -Path $rawPath -Value $rawOutput -Encoding utf8

    $parseResult = Try-ParseJsonFromText -Text $rawOutput
    $parsed = $null
    if ($null -ne $parseResult) {
        $parsed = $parseResult.Parsed
        Set-Content -Path $jsonPath -Value $parseResult.JsonText -Encoding utf8
    }

    $assertion = $null
    if ($exitCode -ne 0) {
        $assertion = New-AssertionResult -Pass $false -Expected "exit code 0" -Actual "exit code $exitCode"
    }
    elseif ($null -ne $parsed) {
        $successEnvelope = Get-PropertyValue -Object $parsed -Name "success"
        if ($null -ne $successEnvelope -and -not [bool]$successEnvelope) {
            $message = Get-PropertyValue -Object $parsed -Name "message"
            if ([string]::IsNullOrWhiteSpace($message)) {
                $message = "success=false"
            }
            $assertion = New-AssertionResult -Pass $false -Expected "success=true" -Actual $message
        }
    }

    if ($null -eq $assertion) {
        if ($null -ne $AssertScript) {
            $assertion = & $AssertScript $parsed $rawOutput
        }
        else {
            $assertion = New-AssertionResult -Pass $true -Expected "command succeeds" -Actual "command succeeds"
        }
    }

    $severity = if ($assertion.ContainsKey("Severity")) { $assertion.Severity } else { "error" }
    $status = if ($assertion.Pass) { "PASS" } elseif ($severity -eq "warning") { "WARN" } else { "FAIL" }

    $result = [pscustomobject]@{
        step = $script:StepCounter
        name = $Name
        phase = $Phase
        scene = $Scene
        mode = $Mode
        command = $commandText
        status = $status
        severity = $severity
        pass = [bool]$assertion.Pass
        expected = $assertion.Expected
        actual = $assertion.Actual
        exit_code = $exitCode
        raw_output_file = $rawPath
        parsed_json_file = if ($null -ne $parseResult) { $jsonPath } else { $null }
    }

    Add-StepResult -Result $result -Parsed $parsed
    Write-Host ("[{0}] {1}" -f $status, $Name)

    return [pscustomobject]@{
        Result = $result
        Parsed = $parsed
        Data = Get-ResponseData -Parsed $parsed
        Raw = $rawOutput
    }
}

function Invoke-SceneLoad {
    param([Parameter(Mandatory = $true)][string]$SceneName)

    $expression = 'UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/' + $SceneName + '.unity", UnityEditor.SceneManagement.OpenSceneMode.Single); return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;'

    return Invoke-UnityCliStep -Name ("Load scene " + $SceneName) `
        -Phase "Scene Setup" `
        -Scene $SceneName `
        -Mode "EditMode" `
        -Args @("exec", $expression) `
        -AssertScript {
            param($parsed, $raw)
            if ($raw -match [regex]::Escape($SceneName)) {
                return New-AssertionResult -Pass $true -Expected ("active scene = " + $SceneName) -Actual $raw.Trim()
            }

            return New-AssertionResult -Pass $false -Expected ("active scene = " + $SceneName) -Actual $raw.Trim()
        }
}

function Invoke-HasCameraOverride {
    param(
        [Parameter(Mandatory = $true)][string]$SceneName,
        [Parameter(Mandatory = $true)][bool]$Expected
    )

    $expression = 'return KineTutor3D.App.SceneCameraDirector.HasOverride("' + $SceneName + '");'
    $expectedText = if ($Expected) { "True" } else { "False" }

    return Invoke-UnityCliStep -Name ("SceneCameraDirector.HasOverride(" + $SceneName + ")") `
        -Phase "Play Mode" `
        -Scene $SceneName `
        -Mode "EditMode" `
        -Args @("exec", $expression) `
        -AssertScript {
            param($parsed, $raw)
            if ($raw -match $expectedText) {
                return New-AssertionResult -Pass $true -Expected ("override = " + $expectedText) -Actual $raw.Trim()
            }

            $severity = if (-not $Expected) { "warning" } else { "error" }
            return New-AssertionResult -Pass $false -Expected ("override = " + $expectedText) -Actual $raw.Trim() -Severity $severity
        }
}

function Invoke-RunTestsAndWait {
    param(
        [Parameter(Mandatory = $true)][ValidateSet("edit", "play")][string]$Mode,
        [Parameter(Mandatory = $true)][string[]]$ExpectedNames
    )

    Invoke-UnityCliStep -Name ("run-tests " + $Mode) `
        -Phase "RunTests" `
        -Mode $Mode `
        -Args @("run_tests_tool", "--params", ('{"mode":"' + $Mode + '"}')) `
        -AssertScript {
            param($parsed, $raw)
            $data = Get-ResponseData -Parsed $parsed
            $status = Get-PropertyValue -Object $data -Name "status"
            if ($status -eq "launched") {
                return New-AssertionResult -Pass $true -Expected "status=launched" -Actual "status=launched"
            }

            return New-AssertionResult -Pass $false -Expected "status=launched" -Actual ($raw.Trim())
        } | Out-Null

    $deadline = (Get-Date).AddSeconds($RunTestsTimeoutSeconds)
    do {
        Start-Sleep -Seconds $PollIntervalSeconds
        $poll = Invoke-UnityCliStep -Name ("run-tests results " + $Mode) `
            -Phase "RunTests" `
            -Mode $Mode `
            -Args @("run_tests_tool", "--params", '{"results":true,"verbose":true}') `
            -AssertScript {
                param($parsed, $raw)
                $data = Get-ResponseData -Parsed $parsed
                if ($null -eq $data) {
                    return New-AssertionResult -Pass $false -Expected "JSON results payload" -Actual "results payload missing"
                }

                $finished = [bool](Get-PropertyValue -Object $data -Name "finished")
                return New-AssertionResult -Pass $true -Expected "poll run-tests results" -Actual ("finished=" + $finished)
            }

        $data = $poll.Data
        if ($null -eq $data) {
            continue
        }

        $finished = [bool](Get-PropertyValue -Object $data -Name "finished")
        if ($finished) {
            $failed = [int](Get-PropertyValue -Object $data -Name "failed")
            $allNames = Get-PropertyValue -Object $data -Name "all_test_names"
            $missingNames = @()

            foreach ($expectedName in $ExpectedNames) {
                $found = $false
                if ($null -ne $allNames) {
                    foreach ($testName in $allNames) {
                        if ($testName -like ("*" + $expectedName + "*")) {
                            $found = $true
                            break
                        }
                    }
                }

                if (-not $found) {
                    $missingNames += $expectedName
                }
            }

            $finalName = "run-tests verify " + $Mode
            $verification = $null
            if ($failed -eq 0 -and $missingNames.Count -eq 0) {
                $verification = New-AssertionResult -Pass $true -Expected "failed=0 and expected test names present" -Actual "failed=0"
            }
            else {
                $actual = "failed=$failed"
                if ($missingNames.Count -gt 0) {
                    $actual += "; missing=" + ($missingNames -join ", ")
                }
                $verification = New-AssertionResult -Pass $false -Expected "failed=0 and expected test names present" -Actual $actual
            }

            $verificationResult = [pscustomobject]@{
                step = $script:StepCounter + 1
                name = $finalName
                phase = "RunTests"
                scene = ""
                mode = $Mode
                command = "run-tests verification"
                status = if ($verification.Pass) { "PASS" } else { "FAIL" }
                severity = if ($verification.ContainsKey("Severity")) { $verification.Severity } else { "error" }
                pass = [bool]$verification.Pass
                expected = $verification.Expected
                actual = $verification.Actual
                exit_code = 0
                raw_output_file = $poll.Result.raw_output_file
                parsed_json_file = $poll.Result.parsed_json_file
            }

            $script:StepCounter++
            $script:Results.Add($verificationResult) | Out-Null
            Write-Host ("[{0}] {1}" -f $verificationResult.status, $finalName)
            return
        }
    } while ((Get-Date) -lt $deadline)

    $timeoutResult = [pscustomobject]@{
        step = $script:StepCounter + 1
        name = "run-tests timeout $Mode"
        phase = "RunTests"
        scene = ""
        mode = $Mode
        command = "run-tests timeout"
        status = "FAIL"
        severity = "error"
        pass = $false
        expected = "finished=true before timeout"
        actual = ("timed out after " + $RunTestsTimeoutSeconds + " seconds")
        exit_code = 0
        raw_output_file = $null
        parsed_json_file = $null
    }

    $script:StepCounter++
    $script:Results.Add($timeoutResult) | Out-Null
    Write-Host ("[FAIL] run-tests timeout " + $Mode)
}

$script:Results = New-Object System.Collections.Generic.List[object]
$script:StepCounter = 0
$script:LastParsed = $null
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputBase = Get-AbsolutePath -Path $OutputRoot
$script:RunDir = Join-Path $outputBase $timestamp
New-Item -ItemType Directory -Force -Path $script:RunDir | Out-Null

$manifest = [ordered]@{
    generated_at = (Get-Date).ToString("o")
    working_directory = (Get-Location).Path
    output_directory = $script:RunDir
    snapshot_name = $SnapshotName
    poll_interval_seconds = $PollIntervalSeconds
    run_tests_timeout_seconds = $RunTestsTimeoutSeconds
}
Write-JsonFile -Path (Join-Path $script:RunDir "manifest.json") -Value $manifest

if (-not (Get-Command unity-cli -ErrorAction SilentlyContinue)) {
    throw "unity-cli command not found. Install it and ensure it is on PATH before running this script."
}

$expectedCliTools = @(
    "compile_check_tool",
    "console_check_tool",
    "run_tests_tool",
    "scene_validate_tool",
    "prefab_validate_tool",
    "scene_hierarchy_tool",
    "component_inspect_tool",
    "robot_catalog_tool",
    "qa_prep_tool",
    "dh_table_tool",
    "joint_limit_tool",
    "build_settings_tool",
    "canvas_validate_tool",
    "asmdef_validate_tool",
    "player_prefs_inspect_tool",
    "resource_validate_tool",
    "session_context_tool",
    "tutor_step_validate_tool",
    "glossary_validate_tool",
    "fr5_diagnostic_tool",
    "asset_size_tool",
    "scene_diff_tool",
    "pose_compare_tool",
    "learning_tabs_tool",
    "camera_capture_tool",
    "fk_compute_tool"
)

Invoke-UnityCliStep -Name "compile-check" -Phase "Environment" -Mode "EditMode" -Args @("compile_check_tool") -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    if ($null -eq $data) {
        return New-AssertionResult -Pass $false -Expected "success=true, is_compiling=false, errors=0" -Actual "parsed data missing"
    }

    $success = [bool](Get-PropertyValue -Object $data -Name "success")
    $isCompiling = [bool](Get-PropertyValue -Object $data -Name "is_compiling")
    $errors = [int](Get-PropertyValue -Object $data -Name "errors")
    $pass = $success -and (-not $isCompiling) -and $errors -eq 0
    return New-AssertionResult -Pass $pass -Expected "success=true, is_compiling=false, errors=0" -Actual ("success=$success, is_compiling=$isCompiling, errors=$errors")
} | Out-Null

Invoke-UnityCliStep -Name "list custom tools" -Phase "Environment" -Mode "EditMode" -Args @("list") -AssertScript {
    param($parsed, $raw)
    $missing = @()
    foreach ($tool in $expectedCliTools) {
        if ($raw -notmatch [regex]::Escape($tool)) {
            $missing += $tool
        }
    }

    if ($missing.Count -eq 0) {
        return New-AssertionResult -Pass $true -Expected "all expected custom tools listed" -Actual "all expected custom tools listed"
    }

    return New-AssertionResult -Pass $false -Expected "all expected custom tools listed" -Actual ("missing: " + ($missing -join ", "))
} | Out-Null

Invoke-UnityCliStep -Name "console-check error" -Phase "Static Gate" -Mode "EditMode" -Args @("console_check_tool", "--params", '{"type":"error"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $errorCount = [int](Get-PropertyValue -Object $data -Name "error_count")
    return New-AssertionResult -Pass ($errorCount -eq 0) -Expected "error_count=0" -Actual ("error_count=$errorCount")
} | Out-Null

Invoke-UnityCliStep -Name "console-check all" -Phase "Static Gate" -Mode "EditMode" -Args @("console_check_tool", "--params", '{"type":"all"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $errorCount = [int](Get-PropertyValue -Object $data -Name "error_count")
    return New-AssertionResult -Pass ($errorCount -eq 0) -Expected "error_count=0 in all logs" -Actual ("error_count=$errorCount")
} | Out-Null

Invoke-UnityCliStep -Name "scene-validate all" -Phase "Static Gate" -Mode "EditMode" -Args @("scene_validate_tool", "--params", '{"name":"all"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $allValid = [bool](Get-PropertyValue -Object $data -Name "all_valid")
    $totalMissing = [int](Get-PropertyValue -Object $data -Name "total_missing")
    return New-AssertionResult -Pass ($allValid -and $totalMissing -eq 0) -Expected "all_valid=true, total_missing=0" -Actual ("all_valid=$allValid, total_missing=$totalMissing")
} | Out-Null

Invoke-UnityCliStep -Name "prefab-validate FR5 control" -Phase "Static Gate" -Mode "EditMode" -Args @("prefab_validate_tool", "--params", '{"path":"Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $valid = [bool](Get-PropertyValue -Object $data -Name "valid")
    $missingScripts = [int](Get-PropertyValue -Object $data -Name "missing_scripts")
    $zeroVertex = [int](Get-PropertyValue -Object $data -Name "zero_vertex_meshes")
    $pass = $valid -and $missingScripts -eq 0 -and $zeroVertex -eq 0
    return New-AssertionResult -Pass $pass -Expected "valid=true, missing_scripts=0, zero_vertex_meshes=0" -Actual ("valid=$valid, missing_scripts=$missingScripts, zero_vertex_meshes=$zeroVertex")
} | Out-Null

Invoke-UnityCliStep -Name "build-settings" -Phase "Static Gate" -Mode "EditMode" -Args @("build_settings_tool") -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $valid = [bool](Get-PropertyValue -Object $data -Name "valid")
    $sceneCount = [int](Get-PropertyValue -Object $data -Name "scene_count")
    return New-AssertionResult -Pass ($valid -and $sceneCount -ge 8) -Expected "valid=true and scene_count>=8" -Actual ("valid=$valid, scene_count=$sceneCount")
} | Out-Null

Invoke-UnityCliStep -Name "asmdef-validate" -Phase "Static Gate" -Mode "EditMode" -Args @("asmdef_validate_tool") -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $valid = [bool](Get-PropertyValue -Object $data -Name "valid")
    return New-AssertionResult -Pass $valid -Expected "valid=true" -Actual ("valid=$valid")
} | Out-Null

Invoke-UnityCliStep -Name "resource-validate" -Phase "Static Gate" -Mode "EditMode" -Args @("resource_validate_tool") -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $valid = [bool](Get-PropertyValue -Object $data -Name "valid")
    $issueCount = [int](Get-PropertyValue -Object $data -Name "issue_count")
    return New-AssertionResult -Pass ($valid -and $issueCount -eq 0) -Expected "valid=true, issue_count=0" -Actual ("valid=$valid, issue_count=$issueCount")
} | Out-Null

Invoke-UnityCliStep -Name "tutorstep-validate" -Phase "Static Gate" -Mode "EditMode" -Args @("tutor_step_validate_tool") -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $valid = [bool](Get-PropertyValue -Object $data -Name "valid")
    $totalExpected = [int](Get-PropertyValue -Object $data -Name "total_expected")
    return New-AssertionResult -Pass ($valid -and $totalExpected -eq 8) -Expected "valid=true and total_expected=8" -Actual ("valid=$valid, total_expected=$totalExpected")
} | Out-Null

Invoke-UnityCliStep -Name "glossary-validate" -Phase "Static Gate" -Mode "EditMode" -Args @("glossary_validate_tool") -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $valid = [bool](Get-PropertyValue -Object $data -Name "valid")
    $databaseExists = [bool](Get-PropertyValue -Object $data -Name "database_exists")
    return New-AssertionResult -Pass ($valid -and $databaseExists) -Expected "valid=true and database_exists=true" -Actual ("valid=$valid, database_exists=$databaseExists")
} | Out-Null

Invoke-UnityCliStep -Name "learning-tabs all" -Phase "Static Gate" -Mode "EditMode" -Args @("learning_tabs_tool", "--params", '{"robot_id":"all"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $valid = [bool](Get-PropertyValue -Object $data -Name "valid")
    $results = Get-PropertyValue -Object $data -Name "results"
    $foundCount = 0
    if ($null -ne $results) {
        foreach ($result in $results) {
            if ([bool](Get-PropertyValue -Object $result -Name "found")) {
                $foundCount++
            }
        }
    }
    $pass = $valid -and $foundCount -ge 6
    return New-AssertionResult -Pass $pass -Expected "valid=true and found_count>=6" -Actual ("valid=$valid, found_count=$foundCount")
} | Out-Null

Invoke-UnityCliStep -Name "asset-size top 10" -Phase "Static Gate" -Mode "EditMode" -Args @("asset_size_tool", "--params", '{"top":10}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $totalFiles = [int](Get-PropertyValue -Object $data -Name "total_files")
    return New-AssertionResult -Pass ($totalFiles -gt 0) -Expected "total_files>0" -Actual ("total_files=$totalFiles")
} | Out-Null

Invoke-UnityCliStep -Name "robot-catalog" -Phase "Domain Tools" -Mode "EditMode" -Args @("robot_catalog_tool") -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $robots = Get-PropertyValue -Object $data -Name "robots"
    $ids = @()
    if ($null -ne $robots) {
        foreach ($robot in $robots) {
            $ids += (Get-PropertyValue -Object $robot -Name "id")
        }
    }
    $required = @("2DOF_RR", "SCARA_RV", "FAIRINO_FR5")
    $missing = @($required | Where-Object { $ids -notcontains $_ })
    $pass = $ids.Count -gt 0 -and $missing.Count -eq 0
    return New-AssertionResult -Pass $pass -Expected "catalog contains 2DOF_RR, SCARA_RV, FAIRINO_FR5" -Actual ("ids=" + ($ids -join ", "))
} | Out-Null

Invoke-UnityCliStep -Name "fk-compute 2DOF_RR" -Phase "Domain Tools" -Mode "EditMode" -Args @("fk_compute_tool", "--params", '{"template":"2DOF_RR","joints":"45,30"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $eePosition = Get-PropertyValue -Object $data -Name "ee_position"
    return New-AssertionResult -Pass (Test-NumericArrayFinite -Value $eePosition) -Expected "finite ee_position values" -Actual ("ee_position=" + (($eePosition | ForEach-Object { $_.ToString() }) -join ", "))
} | Out-Null

Invoke-UnityCliStep -Name "fk-compute FR5" -Phase "Domain Tools" -Mode "EditMode" -Args @("fk_compute_tool", "--params", '{"template":"FR5","joints":"0,-45,0,-59,-92,-42"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $eePosition = Get-PropertyValue -Object $data -Name "ee_position"
    return New-AssertionResult -Pass (Test-NumericArrayFinite -Value $eePosition) -Expected "finite ee_position values" -Actual ("ee_position=" + (($eePosition | ForEach-Object { $_.ToString() }) -join ", "))
} | Out-Null

Invoke-UnityCliStep -Name "dh-table FR5" -Phase "Domain Tools" -Mode "EditMode" -Args @("dh_table_tool", "--params", '{"template":"FR5"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $dof = [int](Get-PropertyValue -Object $data -Name "dof")
    $links = Get-PropertyValue -Object $data -Name "links"
    $count = if ($null -eq $links) { 0 } else { @($links).Count }
    $finite = $true
    foreach ($link in $links) {
        $values = @(
            Get-PropertyValue -Object $link -Name "theta_rad",
            Get-PropertyValue -Object $link -Name "d",
            Get-PropertyValue -Object $link -Name "a",
            Get-PropertyValue -Object $link -Name "alpha_rad"
        )
        if (-not (Test-NumericArrayFinite -Value $values)) {
            $finite = $false
            break
        }
    }
    return New-AssertionResult -Pass ($count -eq $dof -and $finite) -Expected "links.Count == dof and finite numeric fields" -Actual ("dof=$dof, link_count=$count, finite=$finite")
} | Out-Null

Invoke-UnityCliStep -Name "joint-limit FR5" -Phase "Domain Tools" -Mode "EditMode" -Args @("joint_limit_tool", "--params", '{"template":"FR5"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $joints = Get-PropertyValue -Object $data -Name "joints"
    $valid = $true
    foreach ($joint in $joints) {
        $min = [double](Get-PropertyValue -Object $joint -Name "min_deg")
        $max = [double](Get-PropertyValue -Object $joint -Name "max_deg")
        $range = [double](Get-PropertyValue -Object $joint -Name "range_deg")
        if ($min -gt $max -or $range -le 0) {
            $valid = $false
            break
        }
    }
    return New-AssertionResult -Pass $valid -Expected "min<=max and range_deg>0 for each joint" -Actual ("joint_count=" + @($joints).Count + ", valid=$valid")
} | Out-Null

Invoke-UnityCliStep -Name "pose-compare 2DOF_RR" -Phase "Domain Tools" -Mode "EditMode" -Args @("pose_compare_tool", "--params", '{"template":"2DOF_RR","joints_a":"0,0","joints_b":"45,30"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $distance = [double](Get-PropertyValue -Object $data -Name "distance")
    return New-AssertionResult -Pass ($distance -gt 0) -Expected "distance>0" -Actual ("distance=$distance")
} | Out-Null

Invoke-UnityCliStep -Name "scene-diff Boot/Home" -Phase "Domain Tools" -Mode "EditMode" -Args @("scene_diff_tool", "--params", '{"scene_a":"Boot","scene_b":"Home"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $rootA = [int](Get-PropertyValue -Object $data -Name "root_count_a")
    $rootB = [int](Get-PropertyValue -Object $data -Name "root_count_b")
    $commonCount = [int](Get-PropertyValue -Object $data -Name "common_count")
    $onlyInA = Get-PropertyValue -Object $data -Name "only_in_a"
    $onlyInB = Get-PropertyValue -Object $data -Name "only_in_b"
    $hasDiffData = $commonCount -gt 0 -or ($null -ne $onlyInA) -or ($null -ne $onlyInB)
    return New-AssertionResult -Pass ($rootA -gt 0 -and $rootB -gt 0 -and $hasDiffData) -Expected "non-empty scene comparison" -Actual ("root_count_a=$rootA, root_count_b=$rootB, common_count=$commonCount")
} | Out-Null

$qaScenarios = @(
    @{
        Name = "first-time"
        Checks = {
            param($prefsData, $sessionData)
            $hasVisited = Get-EntryByKey -Entries (Get-PropertyValue -Object $prefsData -Name "entries") -Key "KineTutor3D.HasVisited"
            $exists = [bool](Get-PropertyValue -Object $hasVisited -Name "exists")
            $nextScene = Get-PropertyValue -Object $sessionData -Name "expected_next_scene"
            return ($exists -eq $false -and $nextScene -eq "Onboarding")
        }
        Description = "HasVisited unset and expected_next_scene=Onboarding"
    },
    @{
        Name = "returning"
        Checks = {
            param($prefsData, $sessionData)
            $hasVisited = Get-EntryByKey -Entries (Get-PropertyValue -Object $prefsData -Name "entries") -Key "KineTutor3D.HasVisited"
            $exists = [bool](Get-PropertyValue -Object $hasVisited -Name "exists")
            $value = Get-PropertyValue -Object $hasVisited -Name "value"
            $nextScene = Get-PropertyValue -Object $sessionData -Name "expected_next_scene"
            return ($exists -and $value -eq "1" -and $nextScene -eq "Home")
        }
        Description = "HasVisited=1 and expected_next_scene=Home"
    },
    @{
        Name = "sandbox"
        Checks = {
            param($prefsData, $sessionData)
            $modeEntry = Get-EntryByKey -Entries (Get-PropertyValue -Object $prefsData -Name "entries") -Key "KineTutor3D.SelectedMode"
            $robotEntry = Get-EntryByKey -Entries (Get-PropertyValue -Object $prefsData -Name "entries") -Key "KineTutor3D.SelectedRobotId"
            $nextScene = Get-PropertyValue -Object $sessionData -Name "expected_next_scene"
            return ((Get-PropertyValue -Object $modeEntry -Name "value") -eq "sandbox" -and (Get-PropertyValue -Object $robotEntry -Name "value") -eq "2DOF_RR" -and $nextScene -eq "Sandbox")
        }
        Description = "SelectedMode=sandbox, SelectedRobotId=2DOF_RR, expected_next_scene=Sandbox"
    },
    @{
        Name = "robot-control"
        Checks = {
            param($prefsData, $sessionData)
            $modeEntry = Get-EntryByKey -Entries (Get-PropertyValue -Object $prefsData -Name "entries") -Key "KineTutor3D.SelectedMode"
            $robotEntry = Get-EntryByKey -Entries (Get-PropertyValue -Object $prefsData -Name "entries") -Key "KineTutor3D.SelectedRobotId"
            $nextScene = Get-PropertyValue -Object $sessionData -Name "expected_next_scene"
            return ((Get-PropertyValue -Object $modeEntry -Name "value") -eq "robot_control" -and (Get-PropertyValue -Object $robotEntry -Name "value") -eq "FAIRINO_FR5" -and $nextScene -eq "RobotControl")
        }
        Description = "SelectedMode=robot_control, SelectedRobotId=FAIRINO_FR5, expected_next_scene=RobotControl"
    },
    @{
        Name = "math-readiness"
        Checks = {
            param($prefsData, $sessionData)
            $trackEntry = Get-EntryByKey -Entries (Get-PropertyValue -Object $prefsData -Name "entries") -Key "KineTutor3D.CurrentTrack"
            $nextScene = Get-PropertyValue -Object $sessionData -Name "expected_next_scene"
            return ((Get-PropertyValue -Object $trackEntry -Name "value") -eq "math_readiness" -and $nextScene -eq "MathReadiness")
        }
        Description = "CurrentTrack=math_readiness and expected_next_scene=MathReadiness"
    }
)

foreach ($scenario in $qaScenarios) {
    Invoke-UnityCliStep -Name ("qa-prep " + $scenario.Name) -Phase "Scenario State" -Mode "EditMode" -Args @("qa_prep_tool", "--params", ('{"scenario":"' + $scenario.Name + '"}')) -AssertScript {
        param($parsed, $raw)
        $data = Get-ResponseData -Parsed $parsed
        $scenarioValue = Get-PropertyValue -Object $data -Name "scenario"
        return New-AssertionResult -Pass ($scenarioValue -eq $scenario.Name) -Expected ("scenario=" + $scenario.Name) -Actual ("scenario=" + $scenarioValue)
    } | Out-Null

    $prefs = Invoke-UnityCliStep -Name ("playerprefs-inspect after " + $scenario.Name) -Phase "Scenario State" -Mode "EditMode" -Args @("player_prefs_inspect_tool") -AssertScript {
        param($parsed, $raw)
        $data = Get-ResponseData -Parsed $parsed
        $entries = Get-PropertyValue -Object $data -Name "entries"
        $count = if ($null -eq $entries) { 0 } else { @($entries).Count }
        return New-AssertionResult -Pass ($count -eq 9) -Expected "entries count = 9" -Actual ("entries count = $count")
    }

    $session = Invoke-UnityCliStep -Name ("session-context after " + $scenario.Name) -Phase "Scenario State" -Mode "EditMode" -Args @("session_context_tool") -AssertScript {
        param($parsed, $raw)
        $data = Get-ResponseData -Parsed $parsed
        $nextScene = Get-PropertyValue -Object $data -Name "expected_next_scene"
        return New-AssertionResult -Pass ([string]::IsNullOrWhiteSpace($nextScene) -eq $false) -Expected "expected_next_scene populated" -Actual ("expected_next_scene=" + $nextScene)
    }

    $scenarioPass = & $scenario.Checks $prefs.Data $session.Data
    $scenarioResult = [pscustomobject]@{
        step = $script:StepCounter + 1
        name = "scenario verify $($scenario.Name)"
        phase = "Scenario State"
        scene = ""
        mode = "EditMode"
        command = "scenario verification"
        status = if ($scenarioPass) { "PASS" } else { "FAIL" }
        severity = "error"
        pass = [bool]$scenarioPass
        expected = $scenario.Description
        actual = if ($scenarioPass) { $scenario.Description } else { "scenario state mismatch" }
        exit_code = 0
        raw_output_file = $session.Result.raw_output_file
        parsed_json_file = $session.Result.parsed_json_file
    }
    $script:StepCounter++
    $script:Results.Add($scenarioResult) | Out-Null
    Write-Host ("[{0}] {1}" -f $scenarioResult.status, $scenarioResult.name)
}

Invoke-SceneLoad -SceneName "Home" | Out-Null

Invoke-UnityCliStep -Name "scene-hierarchy Home depth 2" -Phase "Scene Read Tools" -Scene "Home" -Mode "EditMode" -Args @("scene_hierarchy_tool", "--params", '{"depth":2}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $rootCount = [int](Get-PropertyValue -Object $data -Name "root_count")
    $hierarchy = Get-PropertyValue -Object $data -Name "hierarchy"
    $hasHierarchy = $null -ne $hierarchy -and @($hierarchy).Count -gt 0
    return New-AssertionResult -Pass ($rootCount -gt 0 -and $hasHierarchy) -Expected "root_count>0 and hierarchy not empty" -Actual ("root_count=$rootCount, hierarchy_count=" + @($hierarchy).Count)
} | Out-Null

Invoke-UnityCliStep -Name "component-inspect Canvas" -Phase "Scene Read Tools" -Scene "Home" -Mode "EditMode" -Args @("component_inspect_tool", "--params", '{"path":"Canvas"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $componentCount = [int](Get-PropertyValue -Object $data -Name "component_count")
    return New-AssertionResult -Pass ($componentCount -gt 0) -Expected "component_count>0" -Actual ("component_count=$componentCount")
} | Out-Null

Invoke-SceneLoad -SceneName "RobotControl" | Out-Null

Invoke-UnityCliStep -Name "canvas-validate RobotControl" -Phase "Scene Read Tools" -Scene "RobotControl" -Mode "EditMode" -Args @("canvas_validate_tool") -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $valid = [bool](Get-PropertyValue -Object $data -Name "valid")
    $hasEventSystem = [bool](Get-PropertyValue -Object $data -Name "has_event_system")
    return New-AssertionResult -Pass ($valid -and $hasEventSystem) -Expected "valid=true and has_event_system=true" -Actual ("valid=$valid, has_event_system=$hasEventSystem")
} | Out-Null

Invoke-UnityCliStep -Name "component-inspect Canvas Transform" -Phase "Scene Read Tools" -Scene "RobotControl" -Mode "EditMode" -Args @("component_inspect_tool", "--params", '{"path":"Canvas","component":"Transform"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $properties = Get-PropertyValue -Object $data -Name "properties"
    $localScale = Get-PropertyValue -Object $properties -Name "localScale"
    return New-AssertionResult -Pass ($null -ne $localScale) -Expected "Transform properties serialized" -Actual ("has_localScale=" + ($null -ne $localScale))
} | Out-Null

Invoke-UnityCliStep -Name "fr5-diagnostic edit mode" -Phase "Play Mode" -Scene "RobotControl" -Mode "EditMode" -Args @("fr5_diagnostic_tool") -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $playMode = [bool](Get-PropertyValue -Object $data -Name "play_mode")
    return New-AssertionResult -Pass (-not $playMode) -Expected "play_mode=false in edit mode" -Actual ("play_mode=$playMode")
} | Out-Null

$cameraCurrentEdit = Invoke-UnityCliStep -Name "camera-capture current edit" -Phase "Play Mode" -Scene "RobotControl" -Mode "EditMode" -Args @("camera_capture_tool", "--params", '{"action":"current"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $scene = Get-PropertyValue -Object $data -Name "scene"
    $playMode = [bool](Get-PropertyValue -Object $data -Name "playMode")
    return New-AssertionResult -Pass ($scene -eq "RobotControl" -and -not $playMode) -Expected "scene=RobotControl and playMode=false" -Actual ("scene=$scene, playMode=$playMode")
}

Invoke-UnityCliStep -Name "editor play --wait" -Phase "Play Mode" -Scene "RobotControl" -Mode "Transition" -Args @("editor", "play", "--wait") -AssertScript {
    param($parsed, $raw)
    if ($raw -match "play" -or $raw -match "Play") {
        return New-AssertionResult -Pass $true -Expected "entered Play Mode" -Actual $raw.Trim()
    }
    return New-AssertionResult -Pass $true -Expected "entered Play Mode" -Actual "command completed"
} | Out-Null

Invoke-UnityCliStep -Name "fr5-diagnostic play mode" -Phase "Play Mode" -Scene "RobotControl" -Mode "PlayMode" -Args @("fr5_diagnostic_tool") -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $playMode = [bool](Get-PropertyValue -Object $data -Name "play_mode")
    $hasCoordinator = [bool](Get-PropertyValue -Object $data -Name "has_coordinator")
    $connection = Get-PropertyValue -Object $data -Name "connection"
    $kinematics = Get-PropertyValue -Object $data -Name "kinematics"
    $pass = $playMode -and $hasCoordinator -and $null -ne $connection -and $null -ne $kinematics
    return New-AssertionResult -Pass $pass -Expected "play_mode=true, has_coordinator=true, connection/kinematics present" -Actual ("play_mode=$playMode, has_coordinator=$hasCoordinator, has_connection=" + ($null -ne $connection) + ", has_kinematics=" + ($null -ne $kinematics))
} | Out-Null

$cameraCurrentPlay = Invoke-UnityCliStep -Name "camera-capture current play" -Phase "Play Mode" -Scene "RobotControl" -Mode "PlayMode" -Args @("camera_capture_tool", "--params", '{"action":"current"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $scene = Get-PropertyValue -Object $data -Name "scene"
    $playMode = [bool](Get-PropertyValue -Object $data -Name "playMode")
    return New-AssertionResult -Pass ($scene -eq "RobotControl" -and $playMode) -Expected "scene=RobotControl and playMode=true" -Actual ("scene=$scene, playMode=$playMode")
}

$cameraCapture = Invoke-UnityCliStep -Name "camera-capture baseline" -Phase "Play Mode" -Scene "RobotControl" -Mode "PlayMode" -Args @("camera_capture_tool", "--params", ('{"action":"capture","name":"' + $SnapshotName + '"}')) -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $name = Get-PropertyValue -Object $data -Name "name"
    return New-AssertionResult -Pass ($name -eq $SnapshotName) -Expected ("name=" + $SnapshotName) -Actual ("name=" + $name)
}
$cameraBaseline = $cameraCapture.Data

Invoke-UnityCliStep -Name "editor stop" -Phase "Play Mode" -Scene "RobotControl" -Mode "Transition" -Args @("editor", "stop") -AssertScript {
    param($parsed, $raw)
    if ($raw -match "stop" -or $raw -match "Stop") {
        return New-AssertionResult -Pass $true -Expected "exited Play Mode" -Actual $raw.Trim()
    }
    return New-AssertionResult -Pass $true -Expected "exited Play Mode" -Actual "command completed"
} | Out-Null

Start-Sleep -Seconds 1

Invoke-UnityCliStep -Name "camera-capture list" -Phase "Play Mode" -Scene "RobotControl" -Mode "EditMode" -Args @("camera_capture_tool", "--params", '{"action":"list"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $snapshots = Get-PropertyValue -Object $data -Name "snapshots"
    $found = $false
    foreach ($snapshot in $snapshots) {
        if ((Get-PropertyValue -Object $snapshot -Name "name") -eq $SnapshotName) {
            $found = $true
            break
        }
    }
    return New-AssertionResult -Pass $found -Expected ("snapshot list contains " + $SnapshotName) -Actual ("found=" + $found)
} | Out-Null

Invoke-UnityCliStep -Name "camera-capture apply" -Phase "Play Mode" -Scene "RobotControl" -Mode "EditMode" -Args @("camera_capture_tool", "--params", ('{"action":"apply","name":"' + $SnapshotName + '","scene":"RobotControl"}')) -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $appliedTo = Get-PropertyValue -Object $data -Name "appliedTo"
    $cameraUpdated = [bool](Get-PropertyValue -Object $data -Name "cameraUpdated")
    return New-AssertionResult -Pass ($appliedTo -eq "RobotControl" -and $cameraUpdated) -Expected "appliedTo=RobotControl and cameraUpdated=true" -Actual ("appliedTo=$appliedTo, cameraUpdated=$cameraUpdated")
} | Out-Null

Invoke-HasCameraOverride -SceneName "RobotControl" -Expected $true | Out-Null
Invoke-SceneLoad -SceneName "RobotControl" | Out-Null

Invoke-UnityCliStep -Name "camera-capture current after apply" -Phase "Play Mode" -Scene "RobotControl" -Mode "EditMode" -Args @("camera_capture_tool", "--params", '{"action":"current"}') -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $matches = Compare-CameraState -CurrentData $data -ExpectedData $cameraBaseline
    return New-AssertionResult -Pass $matches -Expected "current camera matches captured baseline" -Actual ("matches=" + $matches)
} | Out-Null

Invoke-UnityCliStep -Name "camera-capture delete" -Phase "Play Mode" -Scene "RobotControl" -Mode "EditMode" -Args @("camera_capture_tool", "--params", ('{"action":"delete","name":"' + $SnapshotName + '"}')) -AssertScript {
    param($parsed, $raw)
    $data = Get-ResponseData -Parsed $parsed
    $deleted = Get-PropertyValue -Object $data -Name "deleted"
    return New-AssertionResult -Pass ($deleted -eq $SnapshotName) -Expected ("deleted=" + $SnapshotName) -Actual ("deleted=" + $deleted)
} | Out-Null

Invoke-HasCameraOverride -SceneName "RobotControl" -Expected $false | Out-Null

Invoke-RunTestsAndWait -Mode "edit" -ExpectedNames @("CliToolsCoreLogicTests", "UIInventoryValidatorTests")
Invoke-RunTestsAndWait -Mode "play" -ExpectedNames @("AllButtonsSmokeTests", "FullSceneTransitionTests")

$summary = [ordered]@{
    generated_at = (Get-Date).ToString("o")
    output_directory = $script:RunDir
    total_steps = $script:Results.Count
    passed = @($script:Results | Where-Object { $_.status -eq "PASS" }).Count
    warnings = @($script:Results | Where-Object { $_.status -eq "WARN" }).Count
    failed = @($script:Results | Where-Object { $_.status -eq "FAIL" }).Count
    results = $script:Results
}

Write-JsonFile -Path (Join-Path $script:RunDir "summary.json") -Value $summary

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add("# Unity CLI Integration Test Summary")
$markdown.Add("")
$markdown.Add("- Generated: " + $summary.generated_at)
$markdown.Add('- Output directory: `' + $summary.output_directory + '`')
$markdown.Add("- Total steps: " + $summary.total_steps)
$markdown.Add("- Passed: " + $summary.passed)
$markdown.Add("- Warnings: " + $summary.warnings)
$markdown.Add("- Failed: " + $summary.failed)
$markdown.Add("")
$markdown.Add("| Step | Status | Phase | Name | Expected | Actual |")
$markdown.Add("|------|--------|-------|------|----------|--------|")

foreach ($result in $script:Results) {
    $markdown.Add("| {0} | {1} | {2} | {3} | {4} | {5} |" -f `
        $result.step, `
        $result.status, `
        $result.phase, `
        $result.name.Replace("|", "\|"), `
        $result.expected.Replace("|", "\|"), `
        $result.actual.Replace("|", "\|"))
}

Set-Content -Path (Join-Path $script:RunDir "summary.md") -Value ($markdown -join [Environment]::NewLine) -Encoding utf8

$failedSteps = @($script:Results | Where-Object { $_.status -eq "FAIL" })
if ($failedSteps.Count -gt 0) {
    Write-Host ""
    Write-Host ("FAILURES: " + $failedSteps.Count)
    foreach ($failed in $failedSteps) {
        Write-Host (" - [{0}] {1}: expected {2}; actual {3}" -f $failed.phase, $failed.name, $failed.expected, $failed.actual)
    }
    exit 1
}

Write-Host ""
Write-Host ("All Unity CLI integration checks passed. Evidence saved to: " + $script:RunDir)
