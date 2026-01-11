param(
  [ValidateSet('split','single')]
  [string]$Mode = 'split'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Warn([string]$msg) { Write-Warning $msg }
function Fail([string]$msg) { Write-Error $msg; exit 1 }

$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path
Set-Location -LiteralPath $root

if ($root -match '[^\u0000-\u007F]') {
  Warn "Non-ASCII path detected ($root). Recommend copying repo to ASCII path (e.g., C:\dev\MascotDesktop\workspace). See docs/PATHS.md for migration guide."
}

# Select python (prefer venv)
$venvPython = Join-Path $root ".venv\Scripts\python.exe"
if (Test-Path -LiteralPath $venvPython) {
  $python = $venvPython
  $pythonArgs = @()
} else {
  $python = $null
  $pythonArgs = @()
  $cmd = Get-Command python -ErrorAction SilentlyContinue
  if ($cmd) { $python = $cmd.Source }
  if (-not $python) {
    $cmd = Get-Command py -ErrorAction SilentlyContinue
    if ($cmd) {
      $python     = $cmd.Source
      $pythonArgs = @("-3")
    }
  }
}
if (-not $python) { Fail "Python not found. Run scripts/setup/bootstrap.ps1 first." }

function Ensure-Dir([string]$path) {
  if (-not (Test-Path -LiteralPath $path)) {
    New-Item -ItemType Directory -Force -Path $path | Out-Null
  }
}

function Wait-Health([string]$url, [string]$name, [int]$retries = 30, [int]$delaySec = 1) {
  for ($i = 1; $i -le $retries; $i++) {
    try {
      $resp = Invoke-RestMethod -Uri $url -Method Get -TimeoutSec 2
      if ($resp) { Write-Host "$name OK ($url)"; return }
    } catch { }
    Start-Sleep -Seconds $delaySec
  }
  Fail "$name did not become healthy: $url"
}

function Stop-ChildProcesses {
  param([System.Diagnostics.Process]$coreProc, [System.Diagnostics.Process]$avatarProc)
  if ($coreProc -and -not $coreProc.HasExited) {
    Write-Host "Stopping Core (PID $($coreProc.Id)) ..."
    try { Stop-Process -Id $coreProc.Id -Force -ErrorAction SilentlyContinue } catch { }
  }
  if ($avatarProc -and -not $avatarProc.HasExited) {
    Write-Host "Stopping Avatar (PID $($avatarProc.Id)) ..."
    try { Stop-Process -Id $avatarProc.Id -Force -ErrorAction SilentlyContinue } catch { }
  }
}

$logs = Join-Path $root "logs"
Ensure-Dir $logs

$coreProc = $null
$avatarProc = $null

try {
  Write-Host "Starting Core ..."
  if ($Mode -eq 'single') {
    $coreStdOut = Join-Path $logs "core.out.log"
    $coreStdErr = Join-Path $logs "core.err.log"
    $coreProc = Start-Process -FilePath $python -ArgumentList @($pythonArgs + "-m apps.core.poc.poc_core_http") -WorkingDirectory $root -PassThru -WindowStyle Hidden -RedirectStandardOutput $coreStdOut -RedirectStandardError $coreStdErr
  } else {
    $coreProc = Start-Process -FilePath $python -ArgumentList @($pythonArgs + "-m apps.core.poc.poc_core_http") -WorkingDirectory $root -PassThru
  }

  Write-Host "Starting Avatar Viewer ..."
  if ($Mode -eq 'single') {
    $avatarStdOut = Join-Path $logs "avatar.out.log"
    $avatarStdErr = Join-Path $logs "avatar.err.log"
    $avatarProc = Start-Process -FilePath $python -ArgumentList @($pythonArgs + "-m apps.avatar.poc.poc_avatar_mmd_viewer") -WorkingDirectory $root -PassThru -WindowStyle Hidden -RedirectStandardOutput $avatarStdOut -RedirectStandardError $avatarStdErr
  } else {
    $avatarProc = Start-Process -FilePath $python -ArgumentList @($pythonArgs + "-m apps.avatar.poc.poc_avatar_mmd_viewer") -WorkingDirectory $root -PassThru
  }

  Wait-Health "http://127.0.0.1:8765/health" "Core"
  Wait-Health "http://127.0.0.1:8770/avatar/health" "Avatar"

  Write-Host "Opening viewer at http://127.0.0.1:8770/viewer ..."
  Start-Process "http://127.0.0.1:8770/viewer"

  Write-Host "Core PID   : $($coreProc.Id)"
  Write-Host "Avatar PID : $($avatarProc.Id)"

  if ($Mode -eq 'single') {
    Write-Host ""
    Write-Host "=== Single mode: Press Ctrl+C or Enter to stop servers ==="
    Write-Host "Logs: $logs\core.*.log, $logs\avatar.*.log"
    Write-Host ""
    # Wait for user input or Ctrl+C
    try {
      $null = Read-Host "Press Enter to stop"
    } catch {
      # Ctrl+C triggers exception in Read-Host
    }
  } else {
    Write-Host "To stop, close the windows or run: Stop-Process -Id $($coreProc.Id), $($avatarProc.Id)"
  }
} finally {
  if ($Mode -eq 'single') {
    Stop-ChildProcesses -coreProc $coreProc -avatarProc $avatarProc
    Write-Host "Servers stopped."
  }
}
