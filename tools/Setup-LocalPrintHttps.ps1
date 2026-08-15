param(
    [Parameter(Mandatory = $false)]
    [string]$OutputDirectory = ".",

    [Parameter(Mandatory = $false)]
    [int]$Port = 38452
)

$ErrorActionPreference = "Stop"

$output = [System.IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $output | Out-Null

$machine = $env:COMPUTERNAME
$ips = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
    Where-Object {
        $_.IPAddress -ne "127.0.0.1" -and
        $_.IPAddress -notlike "169.254.*" -and
        $_.PrefixOrigin -ne "WellKnown"
    } |
    Select-Object -ExpandProperty IPAddress -Unique

$dnsNames = @("localhost", $machine)
$sanParts = @()
foreach ($dns in $dnsNames) { $sanParts += "DNS=$dns" }
foreach ($ip in $ips) { $sanParts += "IPAddress=$ip" }
$san = "2.5.29.17={text}" + ($sanParts -join "&")

$cert = New-SelfSignedCertificate `
    -Subject "CN=$machine" `
    -FriendlyName "CBL Print Assistant Local HTTPS" `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -HashAlgorithm SHA256 `
    -NotAfter (Get-Date).AddYears(2) `
    -KeyExportPolicy Exportable `
    -TextExtension @($san)

$passwordPlain = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(24))
$password = ConvertTo-SecureString -String $passwordPlain -Force -AsPlainText

$pfxPath = Join-Path $output "localprint.pfx"
$cerPath = Join-Path $output "localprint.cer"
$passwordPath = Join-Path $output "localprint.pfx.password"
$configPath = Join-Path $output "localprint.json"

Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $password | Out-Null
Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
Set-Content -Path $passwordPath -Value $passwordPlain -Encoding ASCII -NoNewline

$rootStore = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
$rootStore.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
try {
    $rootStore.Add($cert)
}
finally {
    $rootStore.Close()
}

if (Test-Path $configPath) {
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
}
else {
    $config = [pscustomobject]@{}
}

if (-not $config.PairingId) {
    $suffix = [Guid]::NewGuid().ToString("N").Substring(0, 12)
    $config | Add-Member -NotePropertyName PairingId -NotePropertyValue "$($machine.ToLowerInvariant())-$suffix" -Force
}
if (-not $config.PairingSecret) {
    $secret = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32)).TrimEnd('=').Replace('+','-').Replace('/','_')
    $config | Add-Member -NotePropertyName PairingSecret -NotePropertyValue $secret -Force
}
$config | Add-Member -NotePropertyName Enabled -NotePropertyValue $true -Force
$config | Add-Member -NotePropertyName HttpsPort -NotePropertyValue $Port -Force
$config | Add-Member -NotePropertyName MaxRequestBytes -NotePropertyValue (25 * 1024 * 1024) -Force
$config | Add-Member -NotePropertyName AllowedOrigins -NotePropertyValue @(
    "https://kbine.cblconnect.app",
    "https://event-snap-capture.lovable.app",
    "https://id-preview--b6b8f8eb-9e4f-4dee-9889-c887b9f5e482.lovable.app"
) -Force
$config | ConvertTo-Json -Depth 5 | Set-Content -Path $configPath -Encoding UTF8

Write-Host "CBL Print Assistant local HTTPS configured."
Write-Host "PFX: $pfxPath"
Write-Host "iPad certificate: $cerPath"
Write-Host "Config: $configPath"
Write-Host "HTTPS port: $Port"
Write-Host "Pairing ID: $($config.PairingId)"
Write-Host "Names/IPs in certificate: $(($dnsNames + $ips) -join ', ')"
Write-Host ""
Write-Host "Keep localprint.json and localprint.pfx.password private."
Write-Host "Install localprint.cer on the iPad and enable full trust before testing from Safari."
