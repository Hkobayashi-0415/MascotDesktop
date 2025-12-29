Param(
    [string]$CoreUrl = "http://127.0.0.1:8765"
)

Write-Host "== Smoke IPC against $CoreUrl ==" -ForegroundColor Cyan

function Invoke-Json {
    param(
        [string]$Method,
        [string]$Path,
        [string]$Body
    )
    $reqId = "req-" + [guid]::NewGuid().ToString()
    try {
        $resp = Invoke-RestMethod -Method $Method -Uri ($CoreUrl + $Path) -Body $Body -ContentType "application/json" -Headers @{ "X-Request-Id" = $reqId }
        return @{ ok = $true; status = 200; body = $resp; request_id = $resp.request_id }
    }
    catch {
        $err = $_.ErrorDetails.Message
        try { $parsed = $err | ConvertFrom-Json } catch { $parsed = $null }
        return @{ ok = $false; status = $_.Exception.Response.StatusCode.value__; body = $parsed; raw = $err }
    }
}

function Show-Result {
    param(
        [string]$Name,
        $Result
    )
    Write-Host ("-- " + $Name) -ForegroundColor Yellow
    if ($Result.ok -and $Result.body) {
        Write-Host ("status=" + $Result.status + " request_id=" + $Result.body.request_id + " error_code=" + $Result.body.error_code) -ForegroundColor Green
    } else {
        $ec = $Result.body.error_code
        $rid = $Result.body.request_id
        Write-Host ("status=" + $Result.status + " request_id=" + $rid + " error_code=" + $ec) -ForegroundColor Red
    }
}

# A) health
$health = Invoke-Json -Method GET -Path "/health" -Body ""
Show-Result "health" $health

# B) broken JSON
$badJson = Invoke-Json -Method POST -Path "/v1/config/set" -Body "not json"
Show-Result "config.set bad json" $badJson

# C) missing fields
$missing = Invoke-Json -Method POST -Path "/v1/config/set" -Body '{"dto_version":"0.1.0","request_id":"req-missing"}'
Show-Result "config.set missing entries" $missing

Write-Host "Expect all responses to be JSON with request_id and error_code for failures." -ForegroundColor Cyan
