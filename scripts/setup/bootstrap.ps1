Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Warn([string]$msg) { Write-Warning $msg }
function Fail([string]$msg) { Write-Error $msg; exit 1 }

# Resolve repo root (workspace/)
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\\..")).Path
Set-Location -LiteralPath $root

# ASCII path warning
if ($root -match '[^\u0000-\u007F]') {
  Warn "Non-ASCII path detected ($root). Recommend copying repo to ASCII path (e.g., C:\\dev\\MascotDesktop\\workspace)."
}

# Pick python
$venvPython   = Join-Path $root ".venv\\Scripts\\python.exe"
$pythonCmd    = $null
$pythonArgs   = @()
if (Test-Path -LiteralPath $venvPython) {
  $pythonCmd = $venvPython
} else {
  $cmd = Get-Command python -ErrorAction SilentlyContinue
  if ($cmd) { $pythonCmd = $cmd.Source }
  if (-not $pythonCmd) {
    $cmd = Get-Command py -ErrorAction SilentlyContinue
    if ($cmd) {
      $pythonCmd  = $cmd.Source
      $pythonArgs = @("-3")
    }
  }
}
if (-not $pythonCmd) {
  Fail "Python not found. Install Python 3.x and re-run."
}

Write-Host "Using python: $pythonCmd"

# Create venv if missing
if (-not (Test-Path -LiteralPath $venvPython)) {
  Write-Host "Creating venv at .venv ..."
  & $pythonCmd @pythonArgs -m venv ".venv"
}

# Ensure venv python exists now
if (-not (Test-Path -LiteralPath $venvPython)) {
  Fail "venv creation failed; .venv\\Scripts\\python.exe not found."
}

# Upgrade pip and install requirements if present
& $venvPython @pythonArgs -m pip install --upgrade pip
$req = Join-Path $root "requirements.txt"
if (Test-Path -LiteralPath $req) {
  Write-Host "Installing requirements.txt ..."
  & $venvPython @pythonArgs -m pip install -r $req
} else {
  Warn "requirements.txt not found; skipping pip install."
}

Write-Host "bootstrap.ps1 completed. Activate venv via `.venv\\Scripts\\Activate.ps1` or run scripts/run.ps1 to start services."
