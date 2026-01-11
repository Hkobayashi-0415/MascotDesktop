# Usage: run from repo root (CocoroMascotDesktop) to mark refs/ as read-only.
# This is optional and non-destructive; remove +R to make writable again.
$refsPath = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\\..\\..\\refs"
if (-not (Test-Path $refsPath)) {
    Write-Error "refs path not found: $refsPath"
    exit 1
}
Get-ChildItem -Path $refsPath -Recurse | ForEach-Object {
    try {
        attrib +R $_.FullName
    } catch {
        Write-Warning "Failed to set read-only: $($_.FullName) - $($_.Exception.Message)"
    }
}
Write-Output "Applied +R to refs under: $refsPath"
