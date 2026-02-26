# check_u8_ops_freshness.ps1
# U8 run summary の鮮度チェック。
#
# 選定ルール（Profile=Any 時）:
#   - ArtifactDir 内の u8_ops_checks_run_*.json を全列挙し、
#     各ファイルの run_at_utc フィールド（DateTimeOffset）が最も新しいものを対象とする。
#   - ファイル名タイムスタンプ（命名規則）ではなく run_at_utc を正とする。
#   - run_at_utc が欠損または不正なファイルはスキップし、警告を出す。
#   - profile が欠損していても継続可能（latest_profile は filename 推定 or Unknown）。
#   - 有効なファイルが 0 件 → Fail(InvalidArtifact)
#   - 対象ファイルが 0 件 → Fail(NoArtifact)
#
# 終了コード:
#   0 = Pass または Warn（経過時間が WarnHours 以上 ThresholdHours 未満の場合は Warn）
#   1 = Fail（Stale / NoArtifact / InvalidArtifact）
#
param(
    [string]$ArtifactDir,
    [ValidateSet("Custom", "Daily", "Gate", "Any")]
    [string]$Profile = "Any",
    [double]$ThresholdHours = 25.0,
    [double]$WarnHours = 22.0,
    [string]$ArtifactPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if (-not $ArtifactDir) {
    $ArtifactDir = Join-Path $repoRoot "Unity_PJ/artifacts/manual-check"
} elseif (-not [System.IO.Path]::IsPathRooted($ArtifactDir)) {
    $ArtifactDir = Join-Path $repoRoot $ArtifactDir
}

function Write-Result {
    param([hashtable]$Result, [string]$Path)
    if ($Path) {
        if (-not [System.IO.Path]::IsPathRooted($Path)) {
            $Path = Join-Path $repoRoot $Path
        }
        $Result | ConvertTo-Json -Depth 4 | Set-Content -Path $Path -Encoding UTF8
        Write-Host "[check_u8_ops_freshness] artifact=$Path"
    }
}

function Resolve-ProfileFromArtifact {
    param(
        $JsonObject,
        [string]$FileName
    )

    if ($null -ne $JsonObject.PSObject.Properties["profile"]) {
        $value = [string]$JsonObject.profile
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value
        }
    }

    if ($FileName -match "^u8_ops_checks_run_(custom|daily|gate)_") {
        $token = $Matches[1].ToLowerInvariant()
        switch ($token) {
            "custom" { return "Custom" }
            "daily"  { return "Daily" }
            "gate"   { return "Gate" }
        }
    }

    return "Unknown"
}

$nowUtc = [DateTimeOffset]::UtcNow

# --- 1. Collect candidate files ---
if (-not (Test-Path -LiteralPath $ArtifactDir)) {
    $result = @{
        checked_at       = $nowUtc.ToString("o")
        status           = "Fail"
        reason           = "NoArtifact"
        profile_filter   = $Profile
        artifact_dir     = $ArtifactDir
        latest_artifact  = $null
        elapsed_hours    = $null
        threshold_hours  = $ThresholdHours
        warn_hours       = $WarnHours
    }
    Write-Result -Result $result -Path $ArtifactPath
    [Console]::Error.WriteLine("[check_u8_ops_freshness] Fail: NoArtifact (directory not found: $ArtifactDir)")
    exit 1
}

$allFiles = Get-ChildItem -Path $ArtifactDir -Filter "u8_ops_checks_run_*.json" -File -ErrorAction SilentlyContinue

if ($Profile -ne "Any") {
    $profileToken = $Profile.ToLowerInvariant()
    $allFiles = $allFiles | Where-Object { $_.Name -like "u8_ops_checks_run_${profileToken}_*.json" }
}

if ($null -eq $allFiles -or @($allFiles).Count -eq 0) {
    $result = @{
        checked_at       = $nowUtc.ToString("o")
        status           = "Fail"
        reason           = "NoArtifact"
        profile_filter   = $Profile
        artifact_dir     = $ArtifactDir
        latest_artifact  = $null
        elapsed_hours    = $null
        threshold_hours  = $ThresholdHours
        warn_hours       = $WarnHours
    }
    Write-Result -Result $result -Path $ArtifactPath
    [Console]::Error.WriteLine("[check_u8_ops_freshness] Fail: NoArtifact (no run summary in $ArtifactDir for profile=$Profile)")
    exit 1
}

# --- 2. Find latest by run_at_utc (not filename) ---
$bestFile   = $null
$bestTime   = [DateTimeOffset]::MinValue
$bestProfile = $null
$invalidCount = 0

foreach ($file in $allFiles) {
    $json = $null
    try {
        $json = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json
    } catch {
        Write-Warning "[check_u8_ops_freshness] Skip $($file.Name): JSON parse failed — $($_.Exception.Message)"
        $invalidCount++
        continue
    }

    if (-not $json.PSObject.Properties["run_at_utc"]) {
        Write-Warning "[check_u8_ops_freshness] Skip $($file.Name): run_at_utc field missing"
        $invalidCount++
        continue
    }

    $parsed = [DateTimeOffset]::MinValue
    if (-not [DateTimeOffset]::TryParse([string]$json.run_at_utc, [ref]$parsed)) {
        Write-Warning "[check_u8_ops_freshness] Skip $($file.Name): run_at_utc parse failed ('$($json.run_at_utc)')"
        $invalidCount++
        continue
    }

    if ($parsed -gt $bestTime) {
        $bestTime    = $parsed
        $bestFile    = $file
        $bestProfile = Resolve-ProfileFromArtifact -JsonObject $json -FileName $file.Name
        if ($bestProfile -eq "Unknown") {
            Write-Warning "[check_u8_ops_freshness] Latest candidate $($file.Name): profile missing; latest_profile=Unknown"
        }
    }
}

if ($null -eq $bestFile) {
    $result = @{
        checked_at       = $nowUtc.ToString("o")
        status           = "Fail"
        reason           = "InvalidArtifact"
        profile_filter   = $Profile
        artifact_dir     = $ArtifactDir
        invalid_files    = $invalidCount
        latest_artifact  = $null
        elapsed_hours    = $null
        threshold_hours  = $ThresholdHours
        warn_hours       = $WarnHours
    }
    Write-Result -Result $result -Path $ArtifactPath
    [Console]::Error.WriteLine("[check_u8_ops_freshness] Fail: InvalidArtifact (no valid run_at_utc found across $invalidCount file(s))")
    exit 1
}

# --- 3. Compute elapsed and determine status ---
$elapsed        = ($nowUtc - $bestTime).TotalHours
$elapsedRounded = [Math]::Round($elapsed, 2)

$status = if ($elapsed -ge $ThresholdHours) { "Fail" }
          elseif ($elapsed -ge $WarnHours)   { "Warn" }
          else                                { "Pass" }

$result = @{
    checked_at         = $nowUtc.ToString("o")
    status             = $status
    reason             = if ($status -eq "Fail") { "Stale" } else { "OK" }
    profile_filter     = $Profile
    latest_artifact    = $bestFile.Name
    latest_run_at_utc  = $bestTime.ToString("o")
    latest_profile     = $bestProfile
    elapsed_hours      = $elapsedRounded
    threshold_hours    = $ThresholdHours
    warn_hours         = $WarnHours
    scanned_files      = @($allFiles).Count
    invalid_files      = $invalidCount
}

Write-Result -Result $result -Path $ArtifactPath

if ($status -eq "Fail") {
    [Console]::Error.WriteLine("[check_u8_ops_freshness] Fail: Stale (elapsed=${elapsedRounded}h >= threshold=${ThresholdHours}h) latest=$($bestFile.Name)")
    exit 1
} elseif ($status -eq "Warn") {
    Write-Warning "[check_u8_ops_freshness] Warn: elapsed=${elapsedRounded}h (warn=${WarnHours}h <= x < threshold=${ThresholdHours}h) latest=$($bestFile.Name)"
    exit 0
} else {
    Write-Host "[check_u8_ops_freshness] Pass: elapsed=${elapsedRounded}h < warn=${WarnHours}h latest=$($bestFile.Name)"
    exit 0
}
