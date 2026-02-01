# Quick doctor for repo boundaries (run at workspace root)
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Fail($msg) {
  Write-Host "doctor: NG - $msg" -ForegroundColor Red
  exit 1
}

try {
  # scripts/dev/doctor.ps1 -> workspace を相対で特定（日本語パス直書き禁止）
  $root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path
  Set-Location -LiteralPath $root

  $errors = New-Object System.Collections.Generic.List[string]
  $warnings = New-Object System.Collections.Generic.List[string]

  # 必須パス（テンプレ）
  $requiredPaths = @(
    "data\templates\assets\placeholders"
  )

  foreach ($p in $requiredPaths) {
    $full = Join-Path $root $p
    if (-not (Test-Path -LiteralPath $full)) {
      $errors.Add("missing required path: $p")
    }
  }

  # 参照箱 refs は workspace 外にある想定（同階層）
  $containerRoot = (Resolve-Path -LiteralPath (Join-Path $root "..")).Path
  $siblingRefs = Join-Path $containerRoot "refs"
  if (Test-Path -LiteralPath (Join-Path $root "refs")) {
    $errors.Add("workspace/refs exists. refs should be sibling of workspace, not inside workspace.")
  }
  if (-not (Test-Path -LiteralPath $siblingRefs)) {
    $warnings.Add("sibling refs/ not found (ok if not set up on this machine): $siblingRefs")
  }

  # Git 追跡チェック（gitがあれば）
  $gitCmd = Get-Command git -ErrorAction SilentlyContinue
  if ($null -eq $gitCmd) {
    $warnings.Add("git not found; skipping tracked-files checks (filesystem checks only).")
  } else {
    $tracked = & git -C $root ls-files

    $forbiddenPrefixes = @(
      "refs/",
      "data/assets_user/",
      "logs/",
      "secrets/",
      "attic/"
    )

    foreach ($prefix in $forbiddenPrefixes) {
      $hit = $tracked | Where-Object { $_ -like "$prefix*" } | Select-Object -First 5
      if ($hit) {
        $errors.Add("forbidden tracked paths detected under '$prefix' (showing up to 5): " + ($hit -join ", "))
      }
    }
  }

  if ($errors.Count -gt 0) {
    foreach ($e in $errors) { Write-Host "doctor: NG - $e" -ForegroundColor Red }
    exit 1
  }

  foreach ($w in $warnings) { Write-Host "doctor: WARN - $w" -ForegroundColor Yellow }

  if ($warnings.Count -gt 0) {
    Write-Host "doctor: OK (with warnings)" -ForegroundColor Yellow
  } else {
    Write-Host "doctor: OK" -ForegroundColor Green
  }
  exit 0
}
catch {
  Fail($_.Exception.Message)
}
