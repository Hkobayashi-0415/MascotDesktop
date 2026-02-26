param(
    [string]$TaskName = "MascotDesktop_U8_DailyOpsChecks",
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

$arguments = @("/Delete", "/TN", $TaskName, "/F")
Write-Host "[unregister_u8_ops_checks_task] Command=schtasks $($arguments -join ' ')"

if ($DryRun) {
    Write-Host "[unregister_u8_ops_checks_task] DryRun mode: task deletion skipped."
    exit 0
}

try {
    $schtasksExe = Resolve-SchtasksPath
    & $schtasksExe @arguments
}
catch {
    Write-Error "[unregister_u8_ops_checks_task] Failed to run schtasks: $($_.Exception.Message)"
    exit 1
}

$exitCode = $LASTEXITCODE
if ($exitCode -ne 0) {
    Write-Error "[unregister_u8_ops_checks_task] Failed to delete task. exit=$exitCode"
    exit $exitCode
}

Write-Host "[unregister_u8_ops_checks_task] Deleted."
exit 0
