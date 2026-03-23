param(
    [switch]$SkipCoplayDev,
    [switch]$SkipMcp
)

$ErrorActionPreference = "Stop"
$PSNativeCommandArgumentPassing = "Standard"

$RobotAppRoot = "C:\Users\ezen601\Desktop\Jason\robotapp2"
$BenchmarkDir = Join-Path $RobotAppRoot "docs\benchmark"
$ScriptPath = Join-Path $BenchmarkDir "measure_claude_code_tokens.py"

Write-Host "=== Claude Code Token Efficiency Benchmark ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Project: $RobotAppRoot"
Write-Host "Script:  $ScriptPath"
Write-Host ""

# Verify prerequisites
$unityctlExe = "C:\Users\ezen601\Desktop\Jason\unityctl\publish\win-x64\unityctl.exe"
$unityctlMcpExe = "C:\Users\ezen601\Desktop\Jason\unityctl\publish\mcp-win-x64\unityctl-mcp.exe"

if (-not (Test-Path $unityctlExe)) {
    Write-Host "[WARN] unityctl.exe not found at $unityctlExe" -ForegroundColor Yellow
    Write-Host "  Run: dotnet publish src/Unityctl.Cli -c Release -o publish/win-x64" -ForegroundColor Yellow
}

if (-not (Test-Path $unityctlMcpExe)) {
    Write-Host "[WARN] unityctl-mcp.exe not found at $unityctlMcpExe" -ForegroundColor Yellow
    Write-Host "  Run: dotnet publish src/Unityctl.Mcp -c Release -o publish/mcp-win-x64" -ForegroundColor Yellow
}

# Build Python arguments
$pyArgs = @($ScriptPath)
if ($SkipCoplayDev) {
    $pyArgs += "--skip-coplaydev"
    Write-Host "[INFO] Skipping CoplayDev MCP measurement" -ForegroundColor Yellow
}
if ($SkipMcp) {
    $pyArgs += "--skip-mcp"
    Write-Host "[INFO] Skipping all MCP measurements" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Running benchmark..." -ForegroundColor Green
Write-Host ""

& python @pyArgs
$exitCode = $LASTEXITCODE

if ($exitCode -ne 0) {
    Write-Host ""
    Write-Host "[ERROR] Benchmark failed with exit code $exitCode" -ForegroundColor Red
    exit $exitCode
}

Write-Host ""
Write-Host "=== Benchmark complete ===" -ForegroundColor Green

# Show generated files
$jsonFile = Join-Path $BenchmarkDir "2026-03-20-claude-code-token-benchmark.json"
$mdFile = Join-Path $BenchmarkDir "2026-03-20-claude-code-token-benchmark.md"

if (Test-Path $jsonFile) {
    $jsonSize = (Get-Item $jsonFile).Length
    Write-Host "  JSON: $jsonFile ($jsonSize bytes)" -ForegroundColor White
}
if (Test-Path $mdFile) {
    $mdSize = (Get-Item $mdFile).Length
    Write-Host "  Report: $mdFile ($mdSize bytes)" -ForegroundColor White
}
