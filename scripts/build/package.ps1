<#
.SYNOPSIS
  PyInstaller で PoC Avatar サーバをビルドする

.DESCRIPTION
  venv 内で pyinstaller をインストールし、mascot.spec を使って one-folder ビルドを実行。
  dist/ に出力される。

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts/build/package.ps1
#>

param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
Set-Location -LiteralPath $root

Write-Host "=== MascotDesktop Packaging ===" -ForegroundColor Cyan
Write-Host "Root: $root"

# Check ASCII path
if ($root -match '[^\u0000-\u007F]') {
  Write-Warning "Non-ASCII path detected. PyInstaller may fail. Consider moving to ASCII path."
}

# Check venv
$venvPython = Join-Path $root ".venv\Scripts\python.exe"
if (-not (Test-Path -LiteralPath $venvPython)) {
  Write-Error "venv not found. Run scripts/setup/bootstrap.ps1 first."
  exit 1
}
Write-Host "Using Python: $venvPython"

# Install pyinstaller if not present
$pipPath = Join-Path $root ".venv\Scripts\pip.exe"
Write-Host "Checking pyinstaller..."
$ErrorActionPreference = 'SilentlyContinue'
$pyiCheck = & $venvPython -m pyinstaller --version 2>&1
$pyiInstalled = $LASTEXITCODE -eq 0
$ErrorActionPreference = 'Stop'

if (-not $pyiInstalled) {
  Write-Host "Installing pyinstaller..."
  & $pipPath install pyinstaller
  if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to install pyinstaller"
    exit 1
  }
}
Write-Host "pyinstaller ready."

# Spec file
$specFile = Join-Path $root "mascot.spec"
if (-not (Test-Path -LiteralPath $specFile)) {
  Write-Error "Spec file not found: $specFile"
  exit 1
}
Write-Host "Using spec: $specFile"

# Clean previous dist
$distDir = Join-Path $root "dist"
if (Test-Path -LiteralPath $distDir) {
  Write-Host "Cleaning previous dist..."
  Remove-Item -LiteralPath $distDir -Recurse -Force
}

# Build
Write-Host "Building..."
& $venvPython -m PyInstaller --clean --noconfirm $specFile
if ($LASTEXITCODE -ne 0) {
  Write-Error "PyInstaller build failed"
  exit 1
}

Write-Host ""
Write-Host "=== Build Complete ===" -ForegroundColor Green
Write-Host "Output: $distDir\mascot_avatar"
Write-Host ""
Write-Host "To run:"
Write-Host "  cd dist\mascot_avatar"
Write-Host "  .\mascot_avatar.exe"
