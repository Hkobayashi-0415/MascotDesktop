param(
    [string]$QuickstartPath = "docs/05-dev/QUICKSTART.md",
    [string]$ManualCheckPath = "docs/05-dev/unity-runtime-manual-check.md",
    [string]$PackagingPath = "docs/PACKAGING.md",
    [string]$ResidentModePath = "docs/RESIDENT_MODE.md",
    [int]$MaxLagDays = 0,
    [switch]$RequireSameDay,
    [switch]$RequireUnityRefsInLegacy = $true,
    [string]$ArtifactPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-RepoRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PathValue
    )

    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        if (-not (Test-Path -LiteralPath $PathValue)) {
            throw "Path not found: $PathValue"
        }
        return (Resolve-Path -LiteralPath $PathValue).Path
    }

    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
    $resolved = Join-Path $repoRoot $PathValue
    if (-not (Test-Path -LiteralPath $resolved)) {
        throw "Path not found: $resolved"
    }

    return (Resolve-Path -LiteralPath $resolved).Path
}

function Parse-LastUpdatedDate {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )

    $line = Select-String -Path $FilePath -Pattern '^\s*-\s*Last Updated:\s*(\d{4}-\d{2}-\d{2})\s*$' |
        Select-Object -First 1

    if (-not $line) {
        throw "Last Updated not found in $FilePath"
    }

    $matched = [regex]::Match($line.Line, '(\d{4}-\d{2}-\d{2})')
    if (-not $matched.Success) {
        throw "Failed to parse Last Updated in $FilePath"
    }

    return [DateTime]::ParseExact($matched.Groups[1].Value, "yyyy-MM-dd", $null)
}

function Contains-UnityReferences {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )

    $raw = Get-Content -Path $FilePath -Raw -Encoding UTF8
    $hasQuickstart = $raw.Contains("docs/05-dev/QUICKSTART.md")
    $hasManual = $raw.Contains("docs/05-dev/unity-runtime-manual-check.md")
    return ($hasQuickstart -and $hasManual)
}

if ($MaxLagDays -lt 0) {
    throw "MaxLagDays must be >= 0."
}

$quickstart = Resolve-RepoRelativePath -PathValue $QuickstartPath
$manualCheck = Resolve-RepoRelativePath -PathValue $ManualCheckPath
$packaging = Resolve-RepoRelativePath -PathValue $PackagingPath
$resident = Resolve-RepoRelativePath -PathValue $ResidentModePath

$quickstartDate = Parse-LastUpdatedDate -FilePath $quickstart
$manualDate = Parse-LastUpdatedDate -FilePath $manualCheck
$packagingDate = Parse-LastUpdatedDate -FilePath $packaging
$residentDate = Parse-LastUpdatedDate -FilePath $resident

$unityLatest = if ($quickstartDate -ge $manualDate) { $quickstartDate } else { $manualDate }

$violations = New-Object System.Collections.Generic.List[string]

if ($quickstartDate -ne $manualDate) {
    $violations.Add("Unity docs last-updated mismatch: QUICKSTART=$($quickstartDate.ToString('yyyy-MM-dd')) manual-check=$($manualDate.ToString('yyyy-MM-dd')).") | Out-Null
}

$legacyDocs = @(
    [pscustomobject]@{ name = "PACKAGING"; path = $packaging; date = $packagingDate },
    [pscustomobject]@{ name = "RESIDENT_MODE"; path = $resident; date = $residentDate }
)

foreach ($doc in $legacyDocs) {
    $lagDays = ($unityLatest.Date - $doc.date.Date).Days
    if ($lagDays -gt $MaxLagDays) {
        $violations.Add("$($doc.name) is stale by $lagDays day(s): legacy=$($doc.date.ToString('yyyy-MM-dd')) unity_latest=$($unityLatest.ToString('yyyy-MM-dd')).") | Out-Null
    }

    if ($RequireSameDay -and $doc.date.Date -ne $unityLatest.Date) {
        $violations.Add("$($doc.name) last-updated is not same day as unity latest: legacy=$($doc.date.ToString('yyyy-MM-dd')) unity_latest=$($unityLatest.ToString('yyyy-MM-dd')).") | Out-Null
    }

    if ($RequireUnityRefsInLegacy -and -not (Contains-UnityReferences -FilePath $doc.path)) {
        $violations.Add("$($doc.name) is missing Unity reference links to QUICKSTART/manual-check.") | Out-Null
    }
}

$summary = [pscustomobject]@{
    status = if ($violations.Count -gt 0) { "FAIL" } else { "PASS" }
    max_lag_days = $MaxLagDays
    require_same_day = [bool]$RequireSameDay
    require_unity_refs_in_legacy = [bool]$RequireUnityRefsInLegacy
    unity_docs = [pscustomobject]@{
        quickstart = [pscustomobject]@{
            path = $quickstart
            last_updated = $quickstartDate.ToString("yyyy-MM-dd")
        }
        manual_check = [pscustomobject]@{
            path = $manualCheck
            last_updated = $manualDate.ToString("yyyy-MM-dd")
        }
        unity_latest = $unityLatest.ToString("yyyy-MM-dd")
    }
    legacy_docs = @(
        [pscustomobject]@{
            path = $packaging
            last_updated = $packagingDate.ToString("yyyy-MM-dd")
            lag_days_from_unity_latest = ($unityLatest.Date - $packagingDate.Date).Days
        },
        [pscustomobject]@{
            path = $resident
            last_updated = $residentDate.ToString("yyyy-MM-dd")
            lag_days_from_unity_latest = ($unityLatest.Date - $residentDate.Date).Days
        }
    )
    violations = @($violations)
    artifact_generated_at_utc = [DateTimeOffset]::UtcNow.ToString("o")
}

if ($ArtifactPath) {
    $artifactDir = Split-Path -Parent $ArtifactPath
    if ($artifactDir) {
        New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
    }
    $summary | ConvertTo-Json -Depth 8 | Set-Content -Path $ArtifactPath -Encoding UTF8
}

Write-Host "[check_unity_legacy_docs_sync] status=$($summary.status) unity_latest=$($summary.unity_docs.unity_latest) max_lag_days=$MaxLagDays"
Write-Host "[check_unity_legacy_docs_sync] QUICKSTART=$($quickstartDate.ToString('yyyy-MM-dd')) manual-check=$($manualDate.ToString('yyyy-MM-dd')) PACKAGING=$($packagingDate.ToString('yyyy-MM-dd')) RESIDENT_MODE=$($residentDate.ToString('yyyy-MM-dd'))"

if ($violations.Count -gt 0) {
    foreach ($violation in $violations) {
        Write-Error "[check_unity_legacy_docs_sync] $violation"
    }
    exit 1
}

exit 0
