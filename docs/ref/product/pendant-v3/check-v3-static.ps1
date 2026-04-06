$ErrorActionPreference = 'Stop'

$rg = Get-Command rg -ErrorAction SilentlyContinue
if (-not $rg) {
    Write-Error "rg not found in PATH. Install ripgrep or add it to PATH first."
    exit 2
}

$checks = @(
    @{
        Id = 'legacy-input'
        Severity = 'Blocker'
        Path = 'Assets/Scripts/UI/RobotControlV3'
        Pattern = 'UnityEngine\.Input'
        Description = 'V3 UI code must not use UnityEngine.Input directly.'
    },
    @{
        Id = 'lifecycle-awake-start'
        Severity = 'Blocker'
        Path = 'Assets/Scripts/UI/RobotControlV3'
        Pattern = '\bAwake\b|\bStart\b'
        Description = 'V3 UI Toolkit controllers should initialize in OnEnable/OnDisable, not Awake/Start.'
    },
    @{
        Id = 'layout-animation'
        Severity = 'Blocker'
        Path = 'Assets/Scripts/UI/RobotControlV3'
        Pattern = 'style\.(width|height|top|left)'
        Description = 'Layout property animation is forbidden; use translate/scale/opacity.'
    },
    @{
        Id = 'concrete-client-in-ui'
        Severity = 'Blocker'
        Path = 'Assets/Scripts/UI/RobotControlV3'
        Pattern = '(MockFairinoClient|LiveFairinoClient)'
        Description = 'UI should depend on interfaces/facades, not concrete robot clients.'
    },
    @{
        Id = 'concrete-renderer-in-ui'
        Severity = 'Warning'
        Path = 'Assets/Scripts/UI/RobotControlV3'
        Pattern = '(RobotRenderer|SceneCameraDirector)'
        Description = 'Direct UI dependency on renderer/camera may violate DIP.'
    },
    @{
        Id = 'scrollview-large-list'
        Severity = 'Warning'
        Path = 'Assets/Scripts/UI/RobotControlV3'
        Pattern = 'new ScrollView|ScrollView\('
        Description = 'Review whether ListView + virtualization is more appropriate.'
    },
    @{
        Id = 'listview-usage'
        Severity = 'Info'
        Path = 'Assets/Scripts/UI/RobotControlV3'
        Pattern = '\bListView\b'
        Description = 'ListView usage found. Verify fixedItemHeight and FixedHeight virtualization.'
    },
    @{
        Id = 'listview-fixed-height'
        Severity = 'Info'
        Path = 'Assets/Scripts/UI/RobotControlV3'
        Pattern = 'fixedItemHeight|CollectionVirtualizationMethod\.FixedHeight'
        Description = 'ListView fixed-height virtualization markers.'
    },
    @{
        Id = 'binding-mixed-ownership'
        Severity = 'Warning'
        Path = 'Assets/Scripts/UI/RobotControlV3'
        Pattern = 'SetValueWithoutNotify|RegisterValueChangedCallback|dataSourcePath|binding-path'
        Description = 'Inspect for mixed binding ownership in the same file.'
    },
    @{
        Id = 'inline-uxml-style'
        Severity = 'Warning'
        Path = 'Assets/UI/PendantV3'
        Pattern = 'style='
        Glob = '*.uxml'
        Description = 'Inline UXML styles should be temporary only; prefer USS.'
    },
    @{
        Id = 'state-persistence-markers'
        Severity = 'Info'
        Path = 'Assets/UI/PendantV3 Assets/Scripts/UI/RobotControlV3'
        Pattern = 'binding-path|data-source-path|view-data-key|viewDataKey'
        Description = 'State persistence and UI binding markers.'
    }
)

function Invoke-RgCheck {
    param(
        [hashtable]$Check
    )

    $args = @('-n', $Check.Pattern)
    if ($Check.ContainsKey('Glob')) {
        $args += @('-g', $Check.Glob)
    }

    $paths = $Check.Path -split ' '
    $args += $paths

    $output = & rg @args 2>$null
    $exitCode = $LASTEXITCODE

    [pscustomobject]@{
        Id = $Check.Id
        Severity = $Check.Severity
        Description = $Check.Description
        ExitCode = $exitCode
        Output = if ($exitCode -eq 0) { $output } else { @() }
    }
}

$results = $checks | ForEach-Object { Invoke-RgCheck $_ }

$blockers = @()
$warnings = @()

foreach ($result in $results) {
    $hasMatch = $result.ExitCode -eq 0 -and $result.Output.Count -gt 0
    switch ($result.Severity) {
        'Blocker' {
            if ($hasMatch) { $blockers += $result }
        }
        'Warning' {
            if ($hasMatch) { $warnings += $result }
        }
        'Info' {
            if ($hasMatch) {
                Write-Host "[INFO] $($result.Id): $($result.Description)" -ForegroundColor Cyan
                $result.Output | ForEach-Object { Write-Host "  $_" }
            }
        }
    }
}

foreach ($result in $warnings) {
    Write-Host "[WARN] $($result.Id): $($result.Description)" -ForegroundColor Yellow
    $result.Output | ForEach-Object { Write-Host "  $_" }
}

foreach ($result in $blockers) {
    Write-Host "[BLOCKER] $($result.Id): $($result.Description)" -ForegroundColor Red
    $result.Output | ForEach-Object { Write-Host "  $_" }
}

if ($blockers.Count -gt 0) {
    Write-Host "`nV3 static checks failed: $($blockers.Count) blocker group(s)." -ForegroundColor Red
    exit 1
}

Write-Host "`nV3 static checks passed with $($warnings.Count) warning group(s)." -ForegroundColor Green
exit 0
