param(
    [string]$ProjectPath,
    [string]$UnityPath,
    [string]$UnityComPath,
    [string]$LogFile,
    [string]$ExecuteMethod,
    [string[]]$ExtraArgs,
    [switch]$NoGraphics,
    [switch]$BatchMode,
    [switch]$Quit
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
    $Quit = $true
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

if (-not $LogFile) {
    $logDir = Join-Path $PSScriptRoot "..\Unity_PJ\artifacts\runtime-check"
    New-Item -ItemType Directory -Force -Path $logDir | Out-Null
    $ts = Get-Date -Format "yyyyMMdd_HHmmss"
    $LogFile = (Join-Path $logDir "unity-$ts.log")
} else {
    $logDir = Split-Path $LogFile -Parent
    if ($logDir) {
        New-Item -ItemType Directory -Force -Path $logDir | Out-Null
    }
}

$args = @()
if ($BatchMode) { $args += "-batchmode" }
if ($NoGraphics) { $args += "-nographics" }
$args += @("-projectPath", $ProjectPath)
if ($ExecuteMethod) { $args += @("-executeMethod", $ExecuteMethod) }
if ($LogFile) { $args += @("-logFile", $LogFile) }
if ($ExtraArgs) { $args += $ExtraArgs }
if ($Quit) { $args += "-quit" }

Write-Host "[run_unity] UnityPath=$UnityPath"
Write-Host "[run_unity] Args=$($args -join ' ')"
& $UnityPath @args
exit $LASTEXITCODE
