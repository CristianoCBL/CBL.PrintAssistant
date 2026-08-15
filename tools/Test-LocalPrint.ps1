param(
    [Parameter(Mandatory = $true)][string]$ImagePath,
    [string]$BaseUrl = "https://localhost:38452",
    [int]$Copies = 1
)

$ErrorActionPreference = "Stop"
$data = Join-Path $env:LOCALAPPDATA "CBL.PrintAssistant"
$configPath = Join-Path $data "localprint.json"
if (-not (Test-Path $configPath)) { throw "Execute Setup-LocalPrintHttps.ps1 primeiro." }
$config = Get-Content $configPath -Raw | ConvertFrom-Json

try {
    $image = (Resolve-Path -LiteralPath $ImagePath -ErrorAction Stop).Path
}
catch {
    throw "Imagem nao encontrada: $ImagePath"
}
$bytes = [IO.File]::ReadAllBytes($image)

$sha256 = [Security.Cryptography.SHA256]::Create()
try {
    $hashBytes = $sha256.ComputeHash($bytes)
}
finally {
    $sha256.Dispose()
}
$sha = -join ($hashBytes | ForEach-Object { $_.ToString("x2") })

$jobId = [Guid]::NewGuid().ToString()
$nonce = [Guid]::NewGuid().ToString("N")
$ts = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$copiesSafe = [Math]::Min([Math]::Max($Copies, 1), 10)
$variant = "composed"
$text = "$jobId`n$copiesSafe`n$variant`n$sha`n$nonce`n$ts"

$hmac = New-Object System.Security.Cryptography.HMACSHA256
try {
    $hmac.Key = [Text.Encoding]::UTF8.GetBytes([string]$config.PairingSecret)
    $signatureBytes = $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($text))
}
finally {
    $hmac.Dispose()
}
$sig = -join ($signatureBytes | ForEach-Object { $_.ToString("x2") })

$contentType = switch ([IO.Path]::GetExtension($image).ToLowerInvariant()) { ".png" {"image/png"}; ".bmp" {"image/bmp"}; default {"image/jpeg"} }
$payload = @{
    protocolVersion = 1; jobId = $jobId; profile = "normal"; copies = $copiesSafe; imageVariant = $variant;
    image = @{ encoding="base64"; contentType=$contentType; sha256=$sha; data=[Convert]::ToBase64String($bytes) };
    context = @{ stationCode="desktop-test"; creationKey="test-$jobId"; stationId=$null; eventId=$null; captureId=$null };
    createdAt = [DateTimeOffset]::UtcNow.ToString("o");
    auth = @{ pairingId=[string]$config.PairingId; nonce=$nonce; ts=$ts; signature=$sig }
}

$result = Invoke-RestMethod -Method Post -Uri "$BaseUrl/print-job" -ContentType "application/json" -Body ($payload | ConvertTo-Json -Depth 8 -Compress)
$result | ConvertTo-Json -Depth 5
for ($i=0; $i -lt 30; $i++) {
    Start-Sleep 1
    $status = Invoke-RestMethod -Uri "$BaseUrl/job-status/$jobId"
    Write-Host "Status: $($status.status) | copias: $($status.copiesPrinted)"
    if ($status.status -in @("printed","failed")) { $status | ConvertTo-Json -Depth 5; break }
}
