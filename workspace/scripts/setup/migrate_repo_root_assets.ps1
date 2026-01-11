<#
.SYNOPSIS
  Repo-root の誤コミットされたモーションファイルを退避する

.DESCRIPTION
  repo-root にある .vmd/.fbx ファイルおよびモーションフォルダを
  ../refs/assets_inbox/ へ移動する。git管理からは別途 git rm が必要。

.PARAMETER Move
  指定すると移動（デフォルトはコピー）

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File scripts/setup/migrate_repo_root_assets.ps1
  powershell -ExecutionPolicy Bypass -File scripts/setup/migrate_repo_root_assets.ps1 -Move
#>

param(
  [switch]$Move
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Paths
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..\..")).Path
$targetDir = Join-Path $repoRoot "refs\assets_inbox"

Write-Host "=== Repo-root Asset Migration ===" -ForegroundColor Cyan
Write-Host "Repo root: $repoRoot"
Write-Host "Target: $targetDir"
Write-Host "Mode: $(if ($Move) { 'MOVE' } else { 'COPY' })"
Write-Host ""

# Targets to migrate
$targets = @(
  # Files
  "きゅーぴっど。モーションデータ.vmd",
  "きゅーぴっど。モーションデータ.fbx",
  # Folders
  "Eyedart_Breath_motion_v1.1",
  "MMO用待機モーションセット",
  "天音かなた公式mmd_ver1.0"
)

# Create target directory
if (-not (Test-Path -LiteralPath $targetDir)) {
  Write-Host "Creating target directory: $targetDir"
  New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
}

$found = 0
$migrated = 0

foreach ($item in $targets) {
  $srcPath = Join-Path $repoRoot $item
  if (Test-Path -LiteralPath $srcPath) {
    $found++
    $dstPath = Join-Path $targetDir $item
    
    if (Test-Path -LiteralPath $dstPath) {
      Write-Host "[SKIP] Already exists: $item" -ForegroundColor Yellow
      continue
    }
    
    if ($Move) {
      Write-Host "[MOVE] $item"
      Move-Item -LiteralPath $srcPath -Destination $dstPath
    } else {
      Write-Host "[COPY] $item"
      if (Test-Path -LiteralPath $srcPath -PathType Container) {
        Copy-Item -LiteralPath $srcPath -Destination $dstPath -Recurse
      } else {
        Copy-Item -LiteralPath $srcPath -Destination $dstPath
      }
    }
    $migrated++
  }
}

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Green
Write-Host "Found: $found"
Write-Host "Migrated: $migrated"
Write-Host ""

if ($found -gt 0 -and -not $Move) {
  Write-Host "Next steps:"
  Write-Host "  1. Verify files in: $targetDir"
  Write-Host "  2. Run with -Move to move (instead of copy)"
  Write-Host "  3. Or manually delete originals and run: git rm <files>"
  Write-Host ""
}

if ($found -eq 0) {
  Write-Host "No migration targets found. Repo-root is clean."
}
