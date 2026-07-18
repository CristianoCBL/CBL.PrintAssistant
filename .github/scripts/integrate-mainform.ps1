$ErrorActionPreference = 'Stop'
$path = 'CBL.PrintAssistant/MainForm.cs'
$text = Get-Content $path -Raw

$field = '        private readonly UpdateService _updateService = new UpdateService();'
$fieldReplacement = "        private readonly UpdateService _updateService = new UpdateService();`r`n        private readonly JobPrintCoordinator _jobPrintCoordinator = new JobPrintCoordinator();"
if (-not $text.Contains('_jobPrintCoordinator')) {
    if (-not $text.Contains($field)) { throw 'Campo de referencia nao encontrado.' }
    $text = $text.Replace($field, $fieldReplacement)
}

$oldConfigPath = '            _configPath = Path.Combine(Application.StartupPath, "appconfig.json");'
$newConfigPath = '            _configPath = AppPaths.ResolveConfigPath(Application.StartupPath);'
if ($text.Contains($oldConfigPath)) {
    $text = $text.Replace($oldConfigPath, $newConfigPath)
}
elseif (-not $text.Contains($newConfigPath)) {
    throw 'Inicializacao de _configPath nao encontrada.'
}

$oldPrint = @'
                if (string.IsNullOrWhiteSpace(job.ImageUrl))
                    throw new Exception("O job não trouxe image_url.");

                bool isStripProfile = string.Equals(profileName, ProfileStrip, StringComparison.OrdinalIgnoreCase);

                for (int i = 0; i < Math.Max(1, job.Copies); i++)
                {
                    await PrintImageFromUrlInternalAsync(job.ImageUrl, profile, isStripProfile, cancellationToken);
                }
'@
$newPrint = @'
                int copiesPrinted = await _jobPrintCoordinator.PrintAsync(
                    profileName,
                    generalConfig,
                    profile,
                    job,
                    AddLog,
                    cancellationToken);
'@
if (-not $text.Contains('int copiesPrinted = await _jobPrintCoordinator.PrintAsync(')) {
    if (-not $text.Contains($oldPrint)) { throw 'Bloco legado de impressao nao encontrado.' }
    $text = $text.Replace($oldPrint, $newPrint)
}

$oldCopies = '                        CopiesPrinted = Math.Max(1, job.Copies)'
$newCopies = '                        CopiesPrinted = copiesPrinted'
if ($text.Contains($oldCopies)) {
    $text = $text.Replace($oldCopies, $newCopies)
}
elseif (-not $text.Contains($newCopies)) {
    throw 'Campo CopiesPrinted nao encontrado.'
}

$oldSave = '            File.WriteAllText(_configPath, JsonConvert.SerializeObject(config, Formatting.Indented));'
$newSave = @'
            AppConfig storageConfig = ConfigSecurity.CreateStorageCopy(config);
            File.WriteAllText(_configPath, JsonConvert.SerializeObject(storageConfig, Formatting.Indented));
'@
if ($text.Contains($oldSave)) {
    $text = $text.Replace($oldSave, $newSave.TrimEnd())
}
elseif (-not $text.Contains('AppConfig storageConfig = ConfigSecurity.CreateStorageCopy(config);')) {
    throw 'Persistencia de configuracao nao encontrada.'
}

$oldLoad = @'
                if (config == null)
                    return;

                _currentConfig = config;
'@
$newLoad = @'
                if (config == null)
                    return;

                bool migratedLegacyTokens = ConfigSecurity.UnprotectInPlace(config);
                if (migratedLegacyTokens)
                {
                    AppConfig storageConfig = ConfigSecurity.CreateStorageCopy(config);
                    File.WriteAllText(_configPath, JsonConvert.SerializeObject(storageConfig, Formatting.Indented));
                }

                _currentConfig = config;
'@
if ($text.Contains($oldLoad)) {
    $text = $text.Replace($oldLoad, $newLoad)
}
elseif (-not $text.Contains('bool migratedLegacyTokens = ConfigSecurity.UnprotectInPlace(config);')) {
    throw 'Carregamento de configuracao nao encontrado.'
}

Set-Content $path $text -Encoding utf8

dotnet restore CBL.PrintAssistant/CBL.PrintAssistant.csproj
dotnet build CBL.PrintAssistant/CBL.PrintAssistant.csproj -c Release --no-restore

if (git diff --quiet -- $path) {
    Write-Host 'MainForm ja estava integrado e protegido.'
    exit 0
}

git config user.name 'github-actions[bot]'
git config user.email '41898282+github-actions[bot]@users.noreply.github.com'
git add $path
git commit -m 'Migra configuracao e protege tokens locais'
git push origin HEAD:feature/professional-v2
