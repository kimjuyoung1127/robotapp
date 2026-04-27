param(
    [string]$Project = "C:\Users\ezen601\Desktop\Jason\robotapp2",
    [string]$Unityctl = "C:\Users\ezen601\Desktop\Jason\unityctl\src\Unityctl.Cli\bin\Debug\net10.0\unityctl.exe",
    [string]$Artifact = "C:\Users\ezen601\Desktop\Jason\robotapp2\Artifacts\robotcontrolv3-actual-click-matrix.json"
)

$ErrorActionPreference = "Stop"

function Invoke-UnityctlJson {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Args)

    $raw = & $Unityctl @Args --json
    $text = ($raw | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($text)) {
        throw "unityctl returned empty output for: $($Args -join ' ')"
    }

    return $text | ConvertFrom-Json
}

function Invoke-Debug {
    param(
        [string]$Method,
        [string]$ArgsJson = $null
    )

    if ([string]::IsNullOrWhiteSpace($ArgsJson)) {
        return Invoke-UnityctlJson exec invoke --project $Project --type "KineTutor3D.App.RobotControlV3DebugBridge" --method $Method
    }

    return Invoke-UnityctlJson exec invoke --project $Project --type "KineTutor3D.App.RobotControlV3DebugBridge" --method $Method --args $ArgsJson
}

function Get-DebugResult {
    param([string]$Method)

    try {
        return (Invoke-Debug $Method).data.result
    } catch {
        return "debug-error=$($_.Exception.Message)"
    }
}

function Select-Element {
    param(
        [string]$Name,
        [string]$Prefer = "desktop"
    )

    $found = Invoke-UnityctlJson uitk find --project $Project --name $Name
    $results = @($found.data.results)
    if ($results.Count -eq 0) {
        return $null
    }

    $visible = @($results | Where-Object { $_.visible -eq $true })
    if ($visible.Count -eq 0) {
        $visible = $results
    }

    if ($Prefer -eq "tablet") {
        $tablet = @($visible | Where-Object { $_.elementPath -like "*BottomSheet*" -or $_.elementPath -like "*BottomBar*" })
        if ($tablet.Count -gt 0) {
            return $tablet[0]
        }
    }

    $desktop = @($visible | Where-Object { $_.elementPath -like "*MainSplit*" -or $_.elementPath -like "*TopStatusBar*" -or $_.elementPath -like "*BottomBar*" -or $_.elementPath -like "*ContextPanel*" })
    if ($desktop.Count -gt 0) {
        return $desktop[0]
    }

    return $visible[0]
}

function Click-Actual {
    param(
        [string]$Name,
        [string]$Prefer = "desktop"
    )

    $element = Select-Element -Name $Name -Prefer $Prefer
    if ($null -eq $element) {
        return [pscustomobject]@{
            success = $false
            message = "not-found"
            locator = ""
            enabledSelf = $false
            visible = $false
            elementPath = ""
        }
    }

    $clicked = Invoke-UnityctlJson uitk click --project $Project --locator $element.locator
    Start-Sleep -Milliseconds 250
    return [pscustomobject]@{
        success = [bool]$clicked.success
        message = [string]$clicked.message
        locator = [string]$element.locator
        enabledSelf = [bool]$element.enabledSelf
        visible = [bool]$element.visible
        elementPath = [string]$element.elementPath
    }
}

function Set-Ready {
    Invoke-Debug DisconnectForDebug | Out-Null
    Invoke-Debug ConnectDefaultForDebug | Out-Null
    Invoke-Debug SetShellSelection '["NavHome","TabEasyMotion","BottomTabEasyMotion"]' | Out-Null
    Click-Actual -Name "BtnServoEnable" | Out-Null
    Ensure-DryRunOn
}

function Ensure-DryRunOn {
    $summary = Get-DebugResult "GetV3RuntimeSummary"
    if ($summary -like "*dryRun=False*") {
        Click-Actual -Name "BtnDryRun" | Out-Null
        Start-Sleep -Milliseconds 150
    }
}

function Set-Shell {
    param(
        [string]$Nav,
        [string]$Work,
        [string]$Tablet
    )

    Invoke-Debug SetShellSelection "[`"$Nav`",`"$Work`",`"$Tablet`"]" | Out-Null
}

function Set-PointDefaults {
    Invoke-Debug SetPointMoveNameForDebug '["AUDIT_UI"]' | Out-Null
    Invoke-Debug SetPointMoveValueForDebug '["X",540]' | Out-Null
    Invoke-Debug SetPointMoveValueForDebug '["Y",130]' | Out-Null
    Invoke-Debug SetPointMoveValueForDebug '["Z",440]' | Out-Null
    Invoke-Debug SetPointMoveValueForDebug '["RX",180]' | Out-Null
    Invoke-Debug SetPointMoveValueForDebug '["RY",0]' | Out-Null
    Invoke-Debug SetPointMoveValueForDebug '["RZ",95]' | Out-Null
}

function New-Case {
    param(
        [string]$Name,
        [scriptblock]$Setup,
        [string]$Summary = "GetMovementStateSummaryForDebug",
        [string]$Needle = "",
        [string]$Prefer = "desktop",
        [string]$Class = "runtime"
    )

    return [pscustomobject]@{
        name = $Name
        setup = $Setup
        summary = $Summary
        needle = $Needle
        prefer = $Prefer
        class = $Class
    }
}

$cases = [System.Collections.Generic.List[object]]::new()

$cases.Add((New-Case "BtnConnect" { Invoke-Debug DisconnectForDebug | Out-Null; Set-Shell "NavHome" "TabEasyMotion" "BottomTabEasyMotion" } "GetV3RuntimeSummary" "connected=True"))
$cases.Add((New-Case "BtnDisconnect" { Invoke-Debug ConnectDefaultForDebug | Out-Null; Set-Shell "NavHome" "TabEasyMotion" "BottomTabEasyMotion" } "GetV3RuntimeSummary" "connected=False"))
$cases.Add((New-Case "BtnQuickAction" { Invoke-Debug ConnectDefaultForDebug | Out-Null; Set-Shell "NavHome" "TabEasyMotion" "BottomTabEasyMotion" } "GetV3RuntimeSummary" "enabled=True"))
$cases.Add((New-Case "BtnPrimaryAction" { Invoke-Debug ConnectDefaultForDebug | Out-Null; Set-Shell "NavHome" "TabEasyMotion" "BottomTabEasyMotion" } "GetV3RuntimeSummary" "enabled=True"))
$cases.Add((New-Case "BtnServoEnable" { Invoke-Debug DisconnectForDebug | Out-Null; Invoke-Debug ConnectDefaultForDebug | Out-Null } "GetV3RuntimeSummary" "enabled=True"))
$cases.Add((New-Case "BtnSync" { Set-Ready } "GetMovementStateSummaryForDebug" "[Sync]"))
$cases.Add((New-Case "BtnStop" { Set-Ready; Invoke-Debug PreviewEasyMotionForDebug '["Ready"]' | Out-Null } "GetMovementStateSummaryForDebug" "[Stop]"))
$cases.Add((New-Case "BtnPause" { Set-Ready } "GetMovementStateSummaryForDebug" "Pause"))
$cases.Add((New-Case "BtnRun" { Set-Ready; Invoke-Debug PreviewEasyMotionForDebug '["Ready"]' | Out-Null } "GetMovementStateSummaryForDebug" "[DryRun Apply]"))
$cases.Add((New-Case "BtnRunBottom" { Set-Ready; Invoke-Debug PreviewEasyMotionForDebug '["Ready"]' | Out-Null } "GetMovementStateSummaryForDebug" "[DryRun Apply]"))
$cases.Add((New-Case "BtnStopBottom" { Set-Ready; Invoke-Debug PreviewEasyMotionForDebug '["Ready"]' | Out-Null } "GetMovementStateSummaryForDebug" "[Stop]"))
$cases.Add((New-Case "BtnResetError" { Set-Ready } "GetMovementStateSummaryForDebug" "[Reset]"))
$cases.Add((New-Case "BtnDryRun" { Set-Ready } "GetV3RuntimeSummary" "dryRun=False"))

foreach ($button in @("BtnEasyHome", "BtnEasyReady", "BtnEasyFolded", "BtnEasyZero", "BtnEasyPreview")) {
    $cases.Add((New-Case $button { Set-Ready; Set-Shell "NavMotion" "TabEasyMotion" "BottomTabEasyMotion" } "GetV3RuntimeSummary" "MoveJ"))
}
$cases.Add((New-Case "BtnEasyApply" { Set-Ready; Set-Shell "NavMotion" "TabEasyMotion" "BottomTabEasyMotion"; Click-Actual "BtnEasyReady" | Out-Null } "GetMovementStateSummaryForDebug" "[DryRun Apply]"))
$cases.Add((New-Case "BtnGripperOpen" { Set-Ready; Set-Shell "NavMotion" "TabEasyMotion" "BottomTabEasyMotion" } "GetMovementStateSummaryForDebug" "Cmd 100%"))
$cases.Add((New-Case "BtnGripperClose" { Set-Ready; Set-Shell "NavMotion" "TabEasyMotion" "BottomTabEasyMotion"; Invoke-Debug SetGripperOpenForDebug '[true]' | Out-Null } "GetMovementStateSummaryForDebug" "Cmd 0%"))

foreach ($axis in 1..6) {
    $cases.Add((New-Case "BtnJoint${axis}Plus" { Set-Ready; Set-Shell "NavMotion" "TabJointJog" "BottomTabJointJog" } "GetMovementStateSummaryForDebug" "MoveJ"))
    $cases.Add((New-Case "BtnJoint${axis}Minus" { Set-Ready; Set-Shell "NavMotion" "TabJointJog" "BottomTabJointJog" } "GetMovementStateSummaryForDebug" "MoveJ"))
}
$cases.Add((New-Case "BtnJointPreview" { Set-Ready; Set-Shell "NavMotion" "TabJointJog" "BottomTabJointJog" } "GetMovementStateSummaryForDebug" "MoveJ"))
$cases.Add((New-Case "BtnJointApply" { Set-Ready; Set-Shell "NavMotion" "TabJointJog" "BottomTabJointJog"; Invoke-Debug NudgeJointForDebug '[1,1]' | Out-Null } "GetMovementStateSummaryForDebug" "[DryRun Apply]"))
$cases.Add((New-Case "BtnJointRestore" { Set-Ready; Set-Shell "NavMotion" "TabJointJog" "BottomTabJointJog"; Invoke-Debug NudgeJointForDebug '[1,1]' | Out-Null } "GetMovementStateSummaryForDebug" "[Restore]"))

foreach ($axis in 1..6) {
    $cases.Add((New-Case "BtnTcp${axis}Plus" { Set-Ready; Set-Shell "NavMotion" "TabTcpJog" "BottomTabTcpJog" } "GetMovementStateSummaryForDebug" "MoveL"))
    $cases.Add((New-Case "BtnTcp${axis}Minus" { Set-Ready; Set-Shell "NavMotion" "TabTcpJog" "BottomTabTcpJog" } "GetMovementStateSummaryForDebug" "MoveL"))
    $cases.Add((New-Case "BtnArrow${axis}Plus" { Set-Ready; Set-Shell "NavMotion" "TabTcpJog" "BottomTabTcpJog" } "GetMovementStateSummaryForDebug" "MoveL"))
    $cases.Add((New-Case "BtnArrow${axis}Minus" { Set-Ready; Set-Shell "NavMotion" "TabTcpJog" "BottomTabTcpJog" } "GetMovementStateSummaryForDebug" "MoveL"))
}
$cases.Add((New-Case "BtnTcpCoordBase" { Set-Ready; Set-Shell "NavMotion" "TabTcpJog" "BottomTabTcpJog" } "GetTcpJogControllerSummary" "coord=Base"))
$cases.Add((New-Case "BtnTcpCoordTool" { Set-Ready; Set-Shell "NavMotion" "TabTcpJog" "BottomTabTcpJog" } "GetTcpJogControllerSummary" "coord=Tool"))
$cases.Add((New-Case "BtnTcpCoordUser" { Set-Ready; Set-Shell "NavMotion" "TabTcpJog" "BottomTabTcpJog" } "GetTcpJogControllerSummary" "coord=User"))
$cases.Add((New-Case "BtnTcpPreview" { Set-Ready; Set-Shell "NavMotion" "TabTcpJog" "BottomTabTcpJog"; Invoke-Debug NudgeTcpAxisForDebug '["X",1]' | Out-Null } "GetMovementStateSummaryForDebug" "MoveL"))
$cases.Add((New-Case "BtnTcpApply" { Set-Ready; Set-Shell "NavMotion" "TabTcpJog" "BottomTabTcpJog"; Invoke-Debug NudgeTcpAxisForDebug '["X",1]' | Out-Null } "GetMovementStateSummaryForDebug" "[DryRun Apply]"))

foreach ($button in @("BtnPointMoveJ", "BtnPointMoveL", "BtnPointPreview", "BtnPointApply", "BtnPointSave", "BtnPointRecall", "BtnPointDelete", "BtnPointRename", "BtnPointExport", "BtnPointCleanup")) {
    $cases.Add((New-Case $button { Set-Ready; Set-Shell "NavMotion" "TabPointMove" "BottomTabPointMove"; Set-PointDefaults; Invoke-Debug CleanupPointMoveForDebug | Out-Null; Invoke-Debug SavePointMoveForDebug | Out-Null } "GetPointMoveListSummaryForDebug" "points="))
}

foreach ($button in @("BtnIoGripperOpen", "BtnIoGripperClose", "BtnIoGripperApply")) {
    $cases.Add((New-Case $button { Set-Ready; Set-Shell "NavMotion" "TabEasyMotion" "BottomTabEasyMotion" } "GetMovementStateSummaryForDebug" "status=ReadyToJog"))
}

foreach ($button in @("BtnViewportBaseFrame", "BtnViewportToolFrame", "BtnViewportTrail", "BtnViewportGhost", "BtnViewportBoundary", "BtnViewportCollision", "BtnViewportCameraReset")) {
    $cases.Add((New-Case $button { Set-Ready; Set-Shell "NavMotion" "TabTcpJog" "BottomTabTcpJog" } "GetAuxLayoutSummaryForDebug" "viewportHorizontalVisible=False"))
}

$cases.Add((New-Case "BtnCoordModeJoint" { Set-Ready } "GetContextPanelScrollSummary" ""))
$cases.Add((New-Case "BtnCoordModeTcp" { Set-Ready } "GetContextPanelScrollSummary" ""))
$cases.Add((New-Case "BtnCoordModeBoth" { Set-Ready } "GetContextPanelScrollSummary" ""))

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Artifact) | Out-Null

Invoke-UnityctlJson exec invoke --project $Project --type "KineTutor3D.Editor.BootScenePlayModeSetup" --method "SetAlwaysStartFromOnboarding" --args "[false]" | Out-Null
$preStatus = Invoke-UnityctlJson status --project $Project --wait
if ($preStatus.data.isPlaying) {
    Invoke-UnityctlJson play stop --project $Project | Out-Null
    Start-Sleep -Seconds 2
}
Invoke-UnityctlJson scene open --project $Project --path "Assets/Scenes/RobotControlV3.unity" | Out-Null
$status = Invoke-UnityctlJson status --project $Project --wait
if (-not $status.data.isPlaying) {
    Invoke-UnityctlJson play start --project $Project | Out-Null
    Start-Sleep -Seconds 4
}

$results = [System.Collections.Generic.List[object]]::new()
foreach ($case in $cases) {
    $before = ""
    $after = ""
    $click = $null
    $passed = $false
    $failureClass = ""

    try {
        & $case.setup
        Start-Sleep -Milliseconds 150
        $before = Get-DebugResult $case.summary
        $click = Click-Actual -Name $case.name -Prefer $case.prefer
        $after = Get-DebugResult $case.summary

        $needleFound = [string]::IsNullOrWhiteSpace($case.needle) -or ($after.IndexOf($case.needle, [System.StringComparison]::Ordinal) -ge 0)
        $passed = $click.success -and $needleFound
        if (-not $passed) {
            if ($click.message -eq "not-found") {
                $failureClass = "locator"
            } elseif (-not $click.enabledSelf) {
                $failureClass = "disabled"
            } elseif ($case.class -eq "product-pending") {
                $failureClass = "product-pending"
            } else {
                $failureClass = "runtime"
            }
        }
    } catch {
        $after = "exception=$($_.Exception.Message)"
        $failureClass = "exception"
    }

    $results.Add([pscustomobject]@{
        name = $case.name
        passed = $passed
        failureClass = $failureClass
        expected = $case.needle
        before = $before
        after = $after
        click = $click
    })
}

$passCount = @($results | Where-Object { $_.passed }).Count
$failCount = @($results | Where-Object { -not $_.passed }).Count
$payload = [pscustomobject]@{
    generatedAt = (Get-Date).ToString("o")
    project = $Project
    caseCount = $results.Count
    passCount = $passCount
    failCount = $failCount
    results = $results
}

$payload | ConvertTo-Json -Depth 12 | Set-Content -Path $Artifact -Encoding UTF8
$payload | ConvertTo-Json -Depth 5
