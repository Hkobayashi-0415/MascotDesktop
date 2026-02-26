param(
    [string]$LogDir,
    [string]$LogPattern = "runtime-*.jsonl",
    [string]$TargetEventName = "ui.hud.bootstrap_missing",
    [string[]]$ResetEventNames = @("ui.hud.bootstrap_recovered", "avatar.model.displayed"),
    [int]$ThresholdConsecutive = 3,
    [double]$MaxGapSeconds = 5.0,
    [switch]$AllowNoLogs,
    [switch]$FailOnParseError,
    [string]$ArtifactPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-DefaultLogDir {
    $candidates = New-Object System.Collections.Generic.List[string]

    function Add-Candidate {
        param(
            [string]$CandidatePath
        )

        if ([string]::IsNullOrWhiteSpace($CandidatePath)) {
            return
        }

        if (-not $candidates.Contains($CandidatePath)) {
            $candidates.Add($CandidatePath) | Out-Null
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($env:MASCOTDESKTOP_RUNTIMELOG_DIR)) {
        Add-Candidate $env:MASCOTDESKTOP_RUNTIMELOG_DIR
    }

    if (-not [string]::IsNullOrWhiteSpace($env:USERPROFILE)) {
        Add-Candidate (Join-Path $env:USERPROFILE "AppData\LocalLow\DefaultCompany\project\logs")
    }

    if (-not [string]::IsNullOrWhiteSpace($env:HOMEDRIVE) -and -not [string]::IsNullOrWhiteSpace($env:HOMEPATH)) {
        Add-Candidate (Join-Path ($env:HOMEDRIVE + $env:HOMEPATH) "AppData\LocalLow\DefaultCompany\project\logs")
    }

    if (Test-Path -LiteralPath "C:\Users") {
        Get-ChildItem -LiteralPath "C:\Users" -Directory -ErrorAction SilentlyContinue | ForEach-Object {
            $userLogDir = Join-Path $_.FullName "AppData\LocalLow\DefaultCompany\project\logs"
            if (Test-Path -LiteralPath $userLogDir) {
                Add-Candidate $userLogDir
            }
        }
    }

    foreach ($candidate in $candidates) {
        if (-not [string]::IsNullOrWhiteSpace($candidate) -and (Test-Path -LiteralPath $candidate)) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    if ($candidates.Count -gt 0) {
        return $candidates[0]
    }

    throw "Could not resolve default runtime log directory."
}

function Try-ParseTimestamp {
    param(
        [string]$RawTimestamp
    )

    if ([string]::IsNullOrWhiteSpace($RawTimestamp)) {
        return $null
    }

    $parsed = [DateTimeOffset]::MinValue
    if ([DateTimeOffset]::TryParse($RawTimestamp, [ref]$parsed)) {
        return $parsed
    }

    return $null
}

if (-not $LogDir) {
    $LogDir = Resolve-DefaultLogDir
} elseif (Test-Path -LiteralPath $LogDir) {
    $LogDir = (Resolve-Path -LiteralPath $LogDir).Path
}

if ($ThresholdConsecutive -lt 1) {
    throw "ThresholdConsecutive must be >= 1."
}

if ($MaxGapSeconds -lt 0) {
    throw "MaxGapSeconds must be >= 0."
}

$files = @()
if (Test-Path -LiteralPath $LogDir) {
    $files = @(
        Get-ChildItem -LiteralPath $LogDir -File -Filter $LogPattern |
            Sort-Object LastWriteTimeUtc, Name
    )
}

if ($files.Count -eq 0) {
    $summaryNoLogs = [pscustomobject]@{
        status = if ($AllowNoLogs) { "PASS" } else { "FAIL" }
        reason = "no_logs_found"
        log_dir = $LogDir
        log_pattern = $LogPattern
        threshold_consecutive = $ThresholdConsecutive
        max_gap_seconds = $MaxGapSeconds
        checked_files = @()
        total_lines = 0
        matched_events = 0
        max_consecutive = 0
        reset_events = $ResetEventNames
        artifact_generated_at_utc = [DateTimeOffset]::UtcNow.ToString("o")
    }

    if ($ArtifactPath) {
        $artifactDir = Split-Path -Parent $ArtifactPath
        if ($artifactDir) {
            New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
        }
        $summaryNoLogs | ConvertTo-Json -Depth 8 | Set-Content -Path $ArtifactPath -Encoding UTF8
    }

    if ($AllowNoLogs) {
        Write-Host "[check_runtime_bootstrap_missing] PASS (no logs found, allowed). dir=$LogDir pattern=$LogPattern"
        exit 0
    }

    Write-Error "[check_runtime_bootstrap_missing] FAIL: no logs found. dir=$LogDir pattern=$LogPattern"
    exit 1
}

$totalLines = 0
$matchedEvents = 0
$parseErrors = New-Object System.Collections.Generic.List[object]

$maxStreak = 0
$maxSequence = $null

$currentStreak = 0
$currentSequence = $null
$lastTargetTimestamp = $null

$normalizedResetEvents = @($ResetEventNames | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })

foreach ($file in $files) {
    $currentStreak = 0
    $currentSequence = $null
    $lastTargetTimestamp = $null
    $lineNo = 0

    foreach ($line in (Get-Content -LiteralPath $file.FullName)) {
        $lineNo++
        $totalLines++
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        $entry = $null
        try {
            $entry = $line | ConvertFrom-Json -ErrorAction Stop
        } catch {
            $parseErrors.Add([pscustomobject]@{
                file = $file.FullName
                line = $lineNo
                error = $_.Exception.Message
            }) | Out-Null
            continue
        }

        $eventName = [string]($entry.event_name)
        $timestamp = Try-ParseTimestamp -RawTimestamp ([string]($entry.timestamp))

        if ($eventName -eq $TargetEventName) {
            $matchedEvents++
            $startNewStreak = $false

            if ($currentStreak -eq 0) {
                $startNewStreak = $true
            } elseif ($null -ne $timestamp -and $null -ne $lastTargetTimestamp) {
                $gap = ($timestamp - $lastTargetTimestamp).TotalSeconds
                if ($gap -gt $MaxGapSeconds) {
                    $startNewStreak = $true
                }
            }

            if ($startNewStreak) {
                $currentStreak = 1
                $currentSequence = [pscustomobject]@{
                    file = $file.FullName
                    start_line = $lineNo
                    end_line = $lineNo
                    start_timestamp = if ($null -ne $timestamp) { $timestamp.ToString("o") } else { "" }
                    end_timestamp = if ($null -ne $timestamp) { $timestamp.ToString("o") } else { "" }
                }
            } else {
                $currentStreak++
                $currentSequence.end_line = $lineNo
                if ($null -ne $timestamp) {
                    $currentSequence.end_timestamp = $timestamp.ToString("o")
                }
            }

            if ($null -ne $timestamp) {
                $lastTargetTimestamp = $timestamp
            }

            if ($currentStreak -gt $maxStreak) {
                $maxStreak = $currentStreak
                $maxSequence = [pscustomobject]@{
                    file = $currentSequence.file
                    start_line = $currentSequence.start_line
                    end_line = $currentSequence.end_line
                    start_timestamp = $currentSequence.start_timestamp
                    end_timestamp = $currentSequence.end_timestamp
                }
            }

            continue
        }

        if ($normalizedResetEvents -contains $eventName) {
            $currentStreak = 0
            $currentSequence = $null
            $lastTargetTimestamp = $null
            continue
        }

        if ($currentStreak -gt 0 -and $null -ne $timestamp -and $null -ne $lastTargetTimestamp) {
            $gapOnOtherEvent = ($timestamp - $lastTargetTimestamp).TotalSeconds
            if ($gapOnOtherEvent -gt $MaxGapSeconds) {
                $currentStreak = 0
                $currentSequence = $null
                $lastTargetTimestamp = $null
            }
        }
    }
}

$violated = [bool]($maxStreak -ge $ThresholdConsecutive)
$parseErrorExceeded = ([bool]$FailOnParseError) -and ($parseErrors.Count -gt 0)
$status = "PASS"
$reason = "ok"
if ($violated) {
    $status = "FAIL"
    $reason = "threshold_exceeded"
} elseif ($parseErrorExceeded) {
    $status = "FAIL"
    $reason = "parse_error"
}

$summary = @{
    status = $status
    reason = $reason
    log_dir = $LogDir
    log_pattern = $LogPattern
    target_event = $TargetEventName
    reset_events = $normalizedResetEvents
    threshold_consecutive = $ThresholdConsecutive
    max_gap_seconds = $MaxGapSeconds
    checked_files = @($files.FullName)
    total_lines = $totalLines
    matched_events = $matchedEvents
    max_consecutive = $maxStreak
    max_sequence = $maxSequence
    parse_error_count = $parseErrors.Count
    parse_errors = @($parseErrors.ToArray())
    artifact_generated_at_utc = [DateTimeOffset]::UtcNow.ToString("o")
}

if ($ArtifactPath) {
    $artifactDir = Split-Path -Parent $ArtifactPath
    if ($artifactDir) {
        New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
    }
    $summary | ConvertTo-Json -Depth 8 | Set-Content -Path $ArtifactPath -Encoding UTF8
}

Write-Host "[check_runtime_bootstrap_missing] status=$($summary.status) max_consecutive=$maxStreak threshold=$ThresholdConsecutive files=$($files.Count) matched=$matchedEvents parse_errors=$($parseErrors.Count)"
if ($null -ne $maxSequence) {
    Write-Host "[check_runtime_bootstrap_missing] max_sequence file=$($maxSequence.file) lines=$($maxSequence.start_line)-$($maxSequence.end_line) ts=$($maxSequence.start_timestamp) -> $($maxSequence.end_timestamp)"
}

if ($violated) {
    Write-Error "[check_runtime_bootstrap_missing] FAIL: '$TargetEventName' reached $maxStreak consecutive outputs (threshold=$ThresholdConsecutive)."
    exit 1
}

if ($parseErrorExceeded) {
    Write-Error "[check_runtime_bootstrap_missing] FAIL: parse errors detected (count=$($parseErrors.Count))."
    exit 1
}

if ($parseErrors.Count -gt 0) {
    Write-Warning "[check_runtime_bootstrap_missing] Parse errors were detected but ignored (count=$($parseErrors.Count)). Use -FailOnParseError to fail."
}

exit 0
