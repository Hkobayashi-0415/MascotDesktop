param(
    [string]$ProjectPath,
    [string]$UnityPath,
    [string]$UnityComPath,
    [ValidateSet("EditMode", "PlayMode")]
    [string]$TestPlatform = "EditMode",
    [string]$TestFilter,
    [string]$ResultsDir,
    [switch]$NoGraphics,
    [switch]$BatchMode,
    [switch]$Quit,
    [switch]$RequireArtifacts
)

if (-not $ProjectPath) {
    $ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..\Unity_PJ\project")).Path
}

if (-not $PSBoundParameters.ContainsKey("BatchMode")) {
    $BatchMode = $true
}
if (-not $PSBoundParameters.ContainsKey("NoGraphics")) {
    $NoGraphics = $true
}
if (-not $PSBoundParameters.ContainsKey("Quit")) {
    $Quit = $false
}

if (-not $UnityPath) {
    $UnityPath = $env:UNITY_EXE
}
if (-not $UnityComPath) {
    $UnityComPath = $env:UNITY_COM
}
if (-not $UnityPath) {
    $UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe"
}
if (-not (Test-Path $UnityPath)) {
    if (-not $UnityComPath) {
        $UnityComPath = "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com"
    }
    if (Test-Path $UnityComPath) {
        $UnityPath = $UnityComPath
    }
}
if (-not (Test-Path $UnityPath)) {
    throw "Unity executable not found. Set UNITY_EXE or pass -UnityPath."
}

if (-not $ResultsDir) {
    $ResultsDir = (Resolve-Path (Join-Path $PSScriptRoot "..\Unity_PJ\artifacts\test-results")).Path
}
New-Item -ItemType Directory -Force -Path $ResultsDir | Out-Null

$ts = Get-Date -Format "yyyyMMdd_HHmmss"
$platformName = $TestPlatform.ToLower()
$results = Join-Path $ResultsDir "$platformName-$ts.xml"
$log = Join-Path $ResultsDir "$platformName-$ts.log"

$args = @()
if ($BatchMode) { $args += "-batchmode" }
if ($NoGraphics) { $args += "-nographics" }
$args += @("-projectPath", $ProjectPath, "-runTests", "-testPlatform", $TestPlatform)
if ($TestFilter) { $args += @("-testFilter", $TestFilter) }
$args += @("-testResults", $results, "-logFile", $log)
if ($Quit) { $args += "-quit" }

Write-Host "[run_unity_tests] UnityPath=$UnityPath"
Write-Host "[run_unity_tests] Args=$($args -join ' ')"

$launched = $false
$exitCode = 1

# Primary: Unity.exe (or explicitly configured path) で起動を試みる
try {
    & $UnityPath @args
    $exitCode = $LASTEXITCODE
    $launched = $true
} catch {
    $errMsg = $_.ToString()
    Write-Warning "[run_unity_tests] Primary launch failed: $errMsg"
    # 起動前失敗（指定されたモジュールが見つかりません など）の場合は Unity.com へフォールバック
}

# Fallback: 起動前失敗時のみ Unity.com を試みる
if (-not $launched) {
    $comFallback = $UnityComPath
    if (-not $comFallback) {
        $comFallback = "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.com"
    }

    if (-not (Test-Path $comFallback)) {
        Write-Error "[run_unity_tests] Fallback Unity.com not found: $comFallback"
        exit 1
    }

    Write-Host "[run_unity_tests] Fallback to Unity.com: $comFallback"
    try {
        & $comFallback @args
        $exitCode = $LASTEXITCODE
        $launched = $true
    } catch {
        Write-Error "[run_unity_tests] Fallback launch also failed: $_"
        exit 1
    }
}

if ($RequireArtifacts) {
    $missingArtifacts = @()
    if (-not (Test-Path $results)) {
        $missingArtifacts += $results
    }
    if (-not (Test-Path $log)) {
        $missingArtifacts += $log
    }

    if ($missingArtifacts.Count -gt 0) {
        Write-Error "[run_unity_tests] Required artifacts were not generated. ExitCode=$exitCode Missing=$($missingArtifacts -join ', ')"
        exit 1
    }

    Write-Host "[run_unity_tests] Artifact check passed. xml=$results log=$log"
}

exit $exitCode
