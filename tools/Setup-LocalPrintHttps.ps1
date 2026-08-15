param([int]$Port = 38452)

$ErrorActionPreference = "Stop"
$data = Join-Path $env:LOCALAPPDATA "CBL.PrintAssistant"
New-Item -ItemType Directory -Force -Path $data | Out-Null

function New-CblRandomBytes([int]$Count) {
    $bytes = New-Object byte[] $Count
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $rng.GetBytes($bytes)
    }
    finally {
        $rng.Dispose()
    }
    return $bytes
}

$machine = $env:COMPUTERNAME
$ips = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
    Where-Object { $_.IPAddress -ne "127.0.0.1" -and $_.IPAddress -notlike "169.254.*" } |
    Select-Object -ExpandProperty IPAddress -Unique

$sanParts = @("DNS=localhost", "DNS=$machine")
foreach ($ip in $ips) { $sanParts += "IPAddress=$ip" }
$san = "2.5.29.17={text}" + ($sanParts -join "&")

$cert = New-SelfSignedCertificate `
    -Subject "CN=$machine" `
    -FriendlyName "CBL Print Assistant Local HTTPS" `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 `
    -NotAfter (Get-Date).AddYears(2) -KeyExportPolicy Exportable `
    -TextExtension @($san)

$passwordPlain = [Convert]::ToBase64String((New-CblRandomBytes 24))
$password = ConvertTo-SecureString $passwordPlain -AsPlainText -Force
$pfx = Join-Path $data "localprint.pfx"
$cer = Join-Path $data "localprint.cer"
$pwd = Join-Path $data "localprint.pfx.password"
$configPath = Join-Path $data "localprint.json"

Export-PfxCertificate -Cert $cert -FilePath $pfx -Password $password | Out-Null
Export-Certificate -Cert $cert -FilePath $cer | Out-Null
Set-Content $pwd $passwordPlain -Encoding ASCII -NoNewline

$root = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
$root.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
try { $root.Add($cert) } finally { $root.Close() }

$config = if (Test-Path $configPath) { Get-Content $configPath -Raw | ConvertFrom-Json } else { [pscustomobject]@{} }
if (-not $config.PairingId) { $config | Add-Member PairingId "$($machine.ToLowerInvariant())-$([Guid]::NewGuid().ToString('N').Substring(0,12))" -Force }
if (-not $config.PairingSecret) {
    $secret = [Convert]::ToBase64String((New-CblRandomBytes 32)).TrimEnd('=').Replace('+','-').Replace('/','_')
    $config | Add-Member PairingSecret $secret -Force
}
$config | Add-Member Enabled $true -Force
$config | Add-Member HttpsPort $Port -Force
$config | Add-Member MaxRequestBytes (25 * 1024 * 1024) -Force
$config | Add-Member AllowedOrigins @(
    "https://kbine.cblconnect.app",
    "https://event-snap-capture.lovable.app",
    "https://id-preview--b6b8f8eb-9e4f-4dee-9889-c887b9f5e482.lovable.app"
) -Force
$config | ConvertTo-Json -Depth 5 | Set-Content $configPath -Encoding UTF8

$firewallName = "CBL Print Assistant Local HTTPS"
$existingRule = Get-NetFirewallRule -DisplayName $firewallName -ErrorAction SilentlyContinue
if ($existingRule) {
    Set-NetFirewallRule -DisplayName $firewallName -Enabled True -Direction Inbound -Action Allow -Profile Private | Out-Null
} else {
    New-NetFirewallRule `
        -DisplayName $firewallName `
        -Direction Inbound `
        -Action Allow `
        -Protocol TCP `
        -LocalPort $Port `
        -Profile Private | Out-Null
}

Write-Host "Configuracao HTTPS local criada em: $data"
Write-Host "Pairing ID: $($config.PairingId)"
Write-Host "Certificado para instalar no iPad: $cer"
Write-Host "Porta HTTPS liberada no Firewall (rede Privada): $Port"
Write-Host "Enderecos incluidos: localhost, $machine, $($ips -join ', ')"
Write-Host "Reinicie o CBL Print Assistant depois deste passo."
