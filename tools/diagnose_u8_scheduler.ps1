# diagnose_u8_scheduler.ps1
# Task Scheduler への実登録可否を事前診断するスクリプト。
# 結果は JSON で標準出力し、ArtifactPath が指定された場合はファイルにも保存する。
#
# 終了コード:
#   0 = can_register: true（登録可能）
#   1 = can_register: false（blockers あり）または診断エラー
#
# 判定項目:
#   1. schtasks.exe のパス解決（PATH → System32 フォールバック）
#   2. schtasks.exe の実行可否テスト（/? 呼び出し）
#   3. 既存タスクの競合確認（/QUERY /TN）
#   4. PowerShell ExecutionPolicy 確認
#   5. run_u8_ops_checks.ps1 の存在確認
#
param(
    [string]$TaskName    = "MascotDesktop_U8_DailyOpsChecks",
    [string]$StartTime   = "09:00",
    [int]$ExecutableProbeRetries = 2,
    [int]$ExecutableProbeRetryIntervalMs = 250,
    [string]$ArtifactPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot  = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$runScript = Join-Path $repoRoot "tools\run_u8_ops_checks.ps1"
$nowUtc    = [DateTimeOffset]::UtcNow
$blockers  = [System.Collections.Generic.List[string]]::new()

if ($ExecutableProbeRetries -lt 1) {
    throw "ExecutableProbeRetries must be >= 1."
}

if ($ExecutableProbeRetryIntervalMs -lt 0) {
    throw "ExecutableProbeRetryIntervalMs must be >= 0."
}

# ---- 1. schtasks.exe path resolution ----
$schtasksFound = $false
$schtasksPath  = $null

$cmdInfo = Get-Command "schtasks.exe" -ErrorAction SilentlyContinue
if ($null -ne $cmdInfo -and -not [string]::IsNullOrWhiteSpace($cmdInfo.Source)) {
    $schtasksPath  = $cmdInfo.Source
    $schtasksFound = $true
} elseif (Test-Path -LiteralPath "C:\Windows\System32\schtasks.exe") {
    $schtasksPath  = "C:\Windows\System32\schtasks.exe"
    $schtasksFound = $true
}

if (-not $schtasksFound) {
    $blockers.Add("schtasks.exe not found in PATH or C:\Windows\System32")
}

Write-Host "[diagnose_u8_scheduler] (1) schtasks_found=$schtasksFound path=$schtasksPath"

# ---- 2. schtasks.exe executable test ----
$schtasksExecutable = $false
$probeAttempts = 0
$probeErrors = [System.Collections.Generic.List[string]]::new()
if ($schtasksFound) {
    for ($attempt = 1; $attempt -le $ExecutableProbeRetries; $attempt++) {
        $probeAttempts = $attempt
        try {
            $null = & $schtasksPath "/?" 2>&1
            # schtasks /? exits with 0 or 1 depending on OS locale; non-zero other than 1 indicates launch failure
            if ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq 1) {
                $schtasksExecutable = $true
                break
            }
            $probeErrors.Add("attempt=$attempt exit_code=$LASTEXITCODE")
        } catch {
            $probeErrors.Add("attempt=$attempt error=$($_.Exception.Message)")
        }

        if (-not $schtasksExecutable -and $attempt -lt $ExecutableProbeRetries -and $ExecutableProbeRetryIntervalMs -gt 0) {
            Start-Sleep -Milliseconds $ExecutableProbeRetryIntervalMs
        }
    }
}

if ($schtasksFound -and -not $schtasksExecutable) {
    $lastProbeError = if ($probeErrors.Count -gt 0) { $probeErrors[$probeErrors.Count - 1] } else { "none" }
    $blockers.Add("schtasks.exe exists but failed executable probe after $probeAttempts attempt(s). last_error=$lastProbeError")
}

Write-Host "[diagnose_u8_scheduler] (2) schtasks_executable=$schtasksExecutable attempts=$probeAttempts"

# ---- 3. Existing task conflict check ----
$taskExists = $false
if ($schtasksExecutable) {
    try {
        $null = & $schtasksPath "/QUERY" "/TN" $TaskName 2>&1
        $taskExists = ($LASTEXITCODE -eq 0)
    } catch {
        # Query failure is non-blocking for registration (task simply does not exist)
        $taskExists = $false
    }
}

if ($taskExists) {
    $blockers.Add("Task '$TaskName' already registered. Use register_u8_ops_checks_task.ps1 with -Force to overwrite, or run unregister first.")
}

Write-Host "[diagnose_u8_scheduler] (3) task_exists=$taskExists"

# ---- 4. PowerShell ExecutionPolicy ----
$policy   = (Get-ExecutionPolicy -Scope LocalMachine).ToString()
$policyOk = $policy -in @("Unrestricted", "RemoteSigned", "Bypass", "AllSigned")
if (-not $policyOk) {
    $blockers.Add("ExecutionPolicy (LocalMachine=$policy) may block script execution. Set to RemoteSigned or Bypass for scheduled tasks.")
}

Write-Host "[diagnose_u8_scheduler] (4) execution_policy_localmachine=$policy ok=$policyOk"

# ---- 5. run_u8_ops_checks.ps1 exists ----
$runScriptFound = Test-Path -LiteralPath $runScript
if (-not $runScriptFound) {
    $blockers.Add("run_u8_ops_checks.ps1 not found at: $runScript")
}

Write-Host "[diagnose_u8_scheduler] (5) run_script_found=$runScriptFound"

# ---- 6. Summary ----
$canRegister = ($blockers.Count -eq 0)

$recommendedCommand = if ($canRegister) {
    "./tools/register_u8_ops_checks_task.ps1 -StartTime '$StartTime' -Force"
} else {
    $null
}

$result = [ordered]@{
    checked_at                    = $nowUtc.ToString("o")
    task_name                     = $TaskName
    start_time                    = $StartTime
    can_register                  = $canRegister
    blockers                      = @($blockers)
    diagnostics                   = [ordered]@{
        schtasks_found             = $schtasksFound
        schtasks_path              = $schtasksPath
        schtasks_executable        = $schtasksExecutable
        executable_probe_retries   = $ExecutableProbeRetries
        executable_probe_retry_interval_ms = $ExecutableProbeRetryIntervalMs
        executable_probe_attempts  = $probeAttempts
        executable_probe_errors    = @($probeErrors)
        task_exists                = $taskExists
        execution_policy_localmachine = $policy
        execution_policy_ok        = $policyOk
        run_script_found           = $runScriptFound
        run_script_path            = $runScript
    }
    recommended_command           = $recommendedCommand
}

$resultJson = $result | ConvertTo-Json -Depth 6

Write-Host ""
Write-Host "=== diagnose_u8_scheduler result ==="
Write-Host $resultJson

if ($ArtifactPath) {
    if (-not [System.IO.Path]::IsPathRooted($ArtifactPath)) {
        $ArtifactPath = Join-Path $repoRoot $ArtifactPath
    }
    $resultJson | Set-Content -Path $ArtifactPath -Encoding UTF8
    Write-Host "[diagnose_u8_scheduler] artifact=$ArtifactPath"
}

if (-not $canRegister) {
    Write-Host ""
    Write-Host "[diagnose_u8_scheduler] Result: CANNOT REGISTER"
    foreach ($b in $blockers) { Write-Host "  BLOCKER: $b" }
    exit 1
}

Write-Host ""
Write-Host "[diagnose_u8_scheduler] Result: CAN REGISTER"
Write-Host "  Run: $recommendedCommand"
exit 0
