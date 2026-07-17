$ErrorActionPreference = 'Stop'
$path = 'CBL.PrintAssistant/MainForm.cs'
$text = Get-Content $path -Raw

$field = '        private readonly UpdateService _updateService = new UpdateService();'
$fieldReplacement = "        private readonly UpdateService _updateService = new UpdateService();`r`n        private readonly JobPrintCoordinator _jobPrintCoordinator = new JobPrintCoordinator();"
if (-not $text.Contains('_jobPrintCoordinator')) {
    if (-not $text.Contains($field)) { throw 'Campo de referencia nao encontrado.' }
    $text = $text.Replace($field, $fieldReplacement)
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

Set-Content $path $text -Encoding utf8

dotnet restore CBL.PrintAssistant/CBL.PrintAssistant.csproj
dotnet build CBL.PrintAssistant/CBL.PrintAssistant.csproj -c Release --no-restore

if (git diff --quiet -- $path) {
    Write-Host 'MainForm ja estava integrado.'
    exit 0
}

git config user.name 'github-actions[bot]'
git config user.email '41898282+github-actions[bot]@users.noreply.github.com'
git add $path
git commit -m 'Integra motor profissional ao processamento de jobs'
git push origin HEAD:feature/professional-v2
