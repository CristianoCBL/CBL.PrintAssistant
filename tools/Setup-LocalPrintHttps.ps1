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

$rootFriendlyName = "CBL Print Assistant Local Root CA - $machine"
$serverFriendlyName = "CBL Print Assistant Local HTTPS"

# Remove somente certificados de teste anteriores criados por este setup.
Get-ChildItem Cert:\LocalMachine\My -ErrorAction SilentlyContinue |
    Where-Object { $_.FriendlyName -eq $serverFriendlyName -or $_.FriendlyName -like "CBL Print Assistant Local Root CA*" } |
    ForEach-Object { Remove-Item -LiteralPath $_.PSPath -Force -ErrorAction SilentlyContinue }
Get-ChildItem Cert:\LocalMachine\Root -ErrorAction SilentlyContinue |
    Where-Object { $_.FriendlyName -eq $serverFriendlyName -or $_.FriendlyName -like "CBL Print Assistant Local Root CA*" } |
    ForEach-Object { Remove-Item -LiteralPath $_.PSPath -Force -ErrorAction SilentlyContinue }

# CA raiz real: e esta raiz que deve ser instalada e marcada como confiavel no iPad.
$rootCert = New-SelfSignedCertificate `
    -Type Custom `
    -Subject "CN=CBL Print Assistant Local Root CA - $machine" `
    -FriendlyName $rootFriendlyName `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 `
    -KeyExportPolicy Exportable `
    -KeyUsage CertSign,CrlSign,DigitalSignature `
    -NotAfter (Get-Date).AddYears(10) `
    -TextExtension @("2.5.29.19={critical}{text}CA=true&pathlength=0")

$rootCer = Join-Path $data "CBL-LocalPrint-RootCA.cer"
Export-Certificate -Cert $rootCert -FilePath $rootCer -Force | Out-Null

# Confia na mesma raiz no proprio Windows para os testes via localhost/LAN.
Import-Certificate -FilePath $rootCer -CertStoreLocation "Cert:\LocalMachine\Root" | Out-Null

# Certificado de servidor separado, assinado pela CA raiz, com SAN para localhost,
# nome do computador e todos os IPv4 locais encontrados no momento do setup.
$serverCert = New-SelfSignedCertificate `
    -Type Custom `
    -Subject "CN=$machine" `
    -FriendlyName $serverFriendlyName `
    -Signer $rootCert `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 `
    -KeyExportPolicy Exportable `
    -KeyUsage DigitalSignature,KeyEncipherment `
    -NotAfter (Get-Date).AddDays(365) `
    -TextExtension @(
        $san,
        "2.5.29.19={critical}{text}CA=false",
        "2.5.29.37={text}1.3.6.1.5.5.7.3.1"
    )

$passwordPlain = [Convert]::ToBase64String((New-CblRandomBytes 24))
$password = ConvertTo-SecureString $passwordPlain -AsPlainText -Force
$pfx = Join-Path $data "localprint.pfx"
$pwd = Join-Path $data "localprint.pfx.password"
$configPath = Join-Path $data "localprint.json"

Export-PfxCertificate -Cert $serverCert -FilePath $pfx -Password $password -Force | Out-Null
Set-Content $pwd $passwordPlain -Encoding ASCII -NoNewline

# Remove o .cer legado para evitar instalar por engano o antigo certificado de servidor.
$legacyCer = Join-Path $data "localprint.cer"
if (Test-Path $legacyCer) { Remove-Item $legacyCer -Force }

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
Write-Host "Pairing ID preservado/criado: $($config.PairingId)"
Write-Host "CA raiz para instalar no iPad: $rootCer"
Write-Host "Certificado HTTPS do servidor: $($serverCert.Subject)"
Write-Host "Porta HTTPS liberada no Firewall (rede Privada): $Port"
Write-Host "Enderecos incluidos no certificado HTTPS: localhost, $machine, $($ips -join ', ')"
Write-Host "Feche e reabra o CBL Print Assistant depois deste passo."
