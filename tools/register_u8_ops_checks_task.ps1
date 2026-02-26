param(
    [string]$TaskName = "MascotDesktop_U8_DailyOpsChecks",
    [string]$StartTime = "09:00",
    [string]$WorkingDirectory,
    [switch]$Force,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-SchtasksPath {
    $command = Get-Command "schtasks.exe" -ErrorAction SilentlyContinue
    if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace($command.Source)) {
        return $command.Source
    }

    $systemPath = "C:\Windows\System32\schtasks.exe"
    if (Test-Path -LiteralPath $systemPath) {
        return $systemPath
    }

    throw "schtasks executable not found."
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if (-not $WorkingDirectory) {
    $WorkingDirectory = $repoRoot
} elseif (-not [System.IO.Path]::IsPathRooted($WorkingDirectory)) {
    $WorkingDirectory = (Resolve-Path (Join-Path $repoRoot $WorkingDirectory)).Path
}

if ($StartTime -notmatch '^(?:[01]\d|2[0-3]):[0-5]\d$') {
    throw "StartTime must be HH:mm (24-hour format)."
}

$runScript = Join-Path $repoRoot "tools\run_u8_ops_checks.ps1"
if (-not (Test-Path -LiteralPath $runScript)) {
    throw "run_u8_ops_checks.ps1 not found: $runScript"
}

$psCommand = "Set-Location -LiteralPath '$WorkingDirectory'; & '$runScript' -Profile Daily"
$taskAction = "powershell.exe -NoProfile -ExecutionPolicy Bypass -Command `"$psCommand`""

$arguments = @("/Create", "/SC", "DAILY", "/TN", $TaskName, "/TR", $taskAction, "/ST", $StartTime)
if ($Force) { $arguments += "/F" }

Write-Host "[register_u8_ops_checks_task] TaskName=$TaskName"
Write-Host "[register_u8_ops_checks_task] StartTime=$StartTime"
Write-Host "[register_u8_ops_checks_task] Command=schtasks $($arguments -join ' ')"

if ($DryRun) {
    Write-Host "[register_u8_ops_checks_task] DryRun mode: task registration skipped."
    exit 0
}

$schtasksExe = Resolve-SchtasksPath
try {
    & $schtasksExe @arguments
}
catch {
    Write-Error "[register_u8_ops_checks_task] Failed to run schtasks ($schtasksExe): $($_.Exception.Message)"
    exit 1
}

$exitCode = $LASTEXITCODE
if ($exitCode -ne 0) {
    Write-Error "[register_u8_ops_checks_task] Failed to register task. exit=$exitCode"
    exit $exitCode
}

Write-Host "[register_u8_ops_checks_task] Registered."
exit 0
