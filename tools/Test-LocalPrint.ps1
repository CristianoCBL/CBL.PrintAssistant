param(
    [Parameter(Mandatory = $true)]
    [string]$ImagePath,

    [Parameter(Mandatory = $false)]
    [string]$BaseUrl = "https://localhost:38452",

    [Parameter(Mandatory = $false)]
    [int]$Copies = 1
)

$ErrorActionPreference = "Stop"

$imageFullPath = [System.IO.Path]::GetFullPath($ImagePath)
if (-not (Test-Path $imageFullPath)) {
    throw "Imagem não encontrada: $imageFullPath"
}

$configPath = Join-Path $PSScriptRoot "..\localprint.json"
$configPath = [System.IO.Path]::GetFullPath($configPath)
if (-not (Test-Path $configPath)) {
    throw "localprint.json não encontrado ao lado do aplicativo. Execute Setup-LocalPrintHttps.ps1 primeiro."
}

$config = Get-Content $configPath -Raw | ConvertFrom-Json
if (-not $config.PairingId -or -not $config.PairingSecret) {
    throw "PairingId/PairingSecret ausentes no localprint.json."
}

$bytes = [System.IO.File]::ReadAllBytes($imageFullPath)
$shaBytes = [System.Security.Cryptography.SHA256]::HashData($bytes)
$sha256 = [Convert]::ToHexString($shaBytes).ToLowerInvariant()
$data = [Convert]::ToBase64String($bytes)

$jobId = [Guid]::NewGuid().ToString()
$nonce = [Guid]::NewGuid().ToString("N")
$ts = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$variant = "composed"
$signingText = "$jobId`n$Copies`n$variant`n$sha256`n$nonce`n$ts"

$hmac = [System.Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes([string]$config.PairingSecret))
try {
    $signature = [Convert]::ToHexString($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($signingText))).ToLowerInvariant()
}
finally {
    $hmac.Dispose()
}

$contentType = switch ([System.IO.Path]::GetExtension($imageFullPath).ToLowerInvariant()) {
    ".png" { "image/png" }
    ".bmp" { "image/bmp" }
    default { "image/jpeg" }
}

$payload = @{
    protocolVersion = 1
    jobId = $jobId
    profile = "normal"
    copies = [Math]::Clamp($Copies, 1, 10)
    imageVariant = $variant
    image = @{
        encoding = "base64"
        contentType = $contentType
        sha256 = $sha256
        data = $data
    }
    context = @{
        stationCode = "desktop-test"
        stationId = $null
        eventId = $null
        creationKey = "test-$jobId"
        captureId = $null
    }
    createdAt = [DateTimeOffset]::UtcNow.ToString("o")
    auth = @{
        pairingId = [string]$config.PairingId
        nonce = $nonce
        ts = $ts
        signature = $signature
    }
}

$json = $payload | ConvertTo-Json -Depth 8 -Compress
$response = Invoke-RestMethod -Method Post -Uri "$BaseUrl/print-job" -ContentType "application/json" -Body $json
$response | ConvertTo-Json -Depth 5

Write-Host "Job enviado: $jobId"
Write-Host "Consultando status..."
for ($i = 0; $i -lt 30; $i++) {
    Start-Sleep -Seconds 1
    try {
        $status = Invoke-RestMethod -Method Get -Uri "$BaseUrl/job-status/$jobId"
        Write-Host "Status: $($status.status) | cópias: $($status.copiesPrinted)"
        if ($status.status -eq "printed" -or $status.status -eq "failed") {
            $status | ConvertTo-Json -Depth 5
            break
        }
    }
    catch {
        Write-Host "Aguardando status..."
    }
}
