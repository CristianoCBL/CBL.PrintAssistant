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

Write-Host "CBL Print Assistant local HTTPS certificate created."
Write-Host "PFX: $pfxPath"
Write-Host "iPad certificate: $cerPath"
Write-Host "HTTPS port: $Port"
Write-Host "Names/IPs in certificate: $($dnsNames + $ips -join ', ')"
Write-Host ""
Write-Host "Next: copy localprint.pfx and localprint.pfx.password beside CBL.PrintAssistant.exe."
Write-Host "Install localprint.cer on the iPad and enable full trust before testing."
