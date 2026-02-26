param(
    [ValidateSet("Custom", "Daily", "Gate")]
    [string]$Profile = "Custom",
    [string]$LogDir,
    [string]$LogPattern = "runtime-*.jsonl",
    [int]$ThresholdConsecutive = 3,
    [double]$MaxGapSeconds = 5.0,
    [int]$MaxLagDays = 0,
    [switch]$RequireSameDay,
    [switch]$AllowNoLogs,
    [switch]$FailOnParseError,
    [string]$ArtifactDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($Profile -eq "Daily") {
    if (-not $PSBoundParameters.ContainsKey("AllowNoLogs")) { $AllowNoLogs = $true }
    if (-not $PSBoundParameters.ContainsKey("RequireSameDay")) { $RequireSameDay = $false }
    if (-not $PSBoundParameters.ContainsKey("MaxLagDays")) { $MaxLagDays = 0 }
}
elseif ($Profile -eq "Gate") {
    if (-not $PSBoundParameters.ContainsKey("AllowNoLogs")) { $AllowNoLogs = $false }
    if (-not $PSBoundParameters.ContainsKey("RequireSameDay")) { $RequireSameDay = $true }
    if (-not $PSBoundParameters.ContainsKey("MaxLagDays")) { $MaxLagDays = 0 }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if (-not $ArtifactDir) {
    $ArtifactDir = Join-Path $repoRoot "Unity_PJ/artifacts/manual-check"
} elseif (-not [System.IO.Path]::IsPathRooted($ArtifactDir)) {
    $ArtifactDir = Join-Path $repoRoot $ArtifactDir
}

New-Item -ItemType Directory -Force -Path $ArtifactDir | Out-Null

function Get-SafeLastExitCode {
    if (Test-Path variable:LASTEXITCODE) {
        return [int]$LASTEXITCODE
    }
    return $null
}

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$profileToken = $Profile.ToLowerInvariant()
$runtimeArtifact = Join-Path $ArtifactDir "u8_runtime_monitor_summary_${profileToken}_$timestamp.json"
$docsArtifact = Join-Path $ArtifactDir "u8_docs_sync_summary_${profileToken}_$timestamp.json"

$runtimeCheckPath = Join-Path $repoRoot "tools/check_runtime_bootstrap_missing.ps1"
$docsCheckPath = Join-Path $repoRoot "tools/check_unity_legacy_docs_sync.ps1"

$runtimeParams = @{
    LogPattern = $LogPattern
    ThresholdConsecutive = $ThresholdConsecutive
    MaxGapSeconds = $MaxGapSeconds
    ArtifactPath = $runtimeArtifact
}
if ($LogDir) { $runtimeParams.LogDir = $LogDir }
if ($AllowNoLogs) { $runtimeParams.AllowNoLogs = $true }
if ($FailOnParseError) { $runtimeParams.FailOnParseError = $true }

$docsParams = @{
    MaxLagDays = $MaxLagDays
    ArtifactPath = $docsArtifact
}
if ($RequireSameDay) { $docsParams.RequireSameDay = $true }

Write-Host "[run_u8_ops_checks] runtime check start"
$runtimeExit = 1
$runtimeError = $null
try {
    & $runtimeCheckPath @runtimeParams
    $runtimeLastExit = Get-SafeLastExitCode
    $runtimeExit = if ($null -eq $runtimeLastExit) { 0 } else { $runtimeLastExit }
} catch {
    $runtimeError = $_.Exception.Message
    $runtimeLastExit = Get-SafeLastExitCode
    $runtimeExit = if ($null -ne $runtimeLastExit -and $runtimeLastExit -ne 0) { $runtimeLastExit } else { 1 }
    Write-Warning "[run_u8_ops_checks] runtime check threw terminating error: $runtimeError"
}
Write-Host "[run_u8_ops_checks] runtime check exit=$runtimeExit artifact=$runtimeArtifact"

Write-Host "[run_u8_ops_checks] docs sync check start"
$docsExit = 1
$docsError = $null
try {
    & $docsCheckPath @docsParams
    $docsLastExit = Get-SafeLastExitCode
    $docsExit = if ($null -eq $docsLastExit) { 0 } else { $docsLastExit }
} catch {
    $docsError = $_.Exception.Message
    $docsLastExit = Get-SafeLastExitCode
    $docsExit = if ($null -ne $docsLastExit -and $docsLastExit -ne 0) { $docsLastExit } else { 1 }
    Write-Warning "[run_u8_ops_checks] docs sync check threw terminating error: $docsError"
}
Write-Host "[run_u8_ops_checks] docs sync check exit=$docsExit artifact=$docsArtifact"

$summary = @{
    run_at_utc = [DateTimeOffset]::UtcNow.ToString("o")
    profile = $Profile
    runtime_check_exit = $runtimeExit
    runtime_check_error = $runtimeError
    docs_sync_check_exit = $docsExit
    docs_sync_check_error = $docsError
    runtime_artifact = $runtimeArtifact
    docs_sync_artifact = $docsArtifact
    options = @{
        log_pattern = $LogPattern
        threshold_consecutive = $ThresholdConsecutive
        max_gap_seconds = $MaxGapSeconds
        max_lag_days = $MaxLagDays
        require_same_day = [bool]$RequireSameDay
        allow_no_logs = [bool]$AllowNoLogs
        fail_on_parse_error = [bool]$FailOnParseError
    }
}
$runSummaryPath = Join-Path $ArtifactDir "u8_ops_checks_run_${profileToken}_$timestamp.json"
$summary | ConvertTo-Json -Depth 4 | Set-Content -Path $runSummaryPath -Encoding UTF8
Write-Host "[run_u8_ops_checks] run summary artifact=$runSummaryPath"

if ($runtimeExit -ne 0 -or $docsExit -ne 0) {
    [Console]::Error.WriteLine("[run_u8_ops_checks] FAIL: runtime_exit=$runtimeExit docs_exit=$docsExit")
    exit 1
}

Write-Host "[run_u8_ops_checks] PASS"
exit 0
