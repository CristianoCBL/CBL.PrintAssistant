using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CBL.PrintAssistant
{
    public class UpdateService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public UpdateService()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CBL.PrintAssistant");
        }

        public async Task<UpdateCheckResult> CheckForUpdateAsync(
            string owner,
            string repo,
            Version currentVersion)
        {
            string url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";

            using var response = await _httpClient.GetAsync(url);
            string body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro ao consultar GitHub Releases: HTTP {(int)response.StatusCode} - {body}");

            var release = JsonSerializer.Deserialize<GitHubRelease>(body);

            if (release == null)
                throw new Exception("Não foi possível ler a release mais recente.");

            string versionText = NormalizeVersion(release.TagName);

            if (!Version.TryParse(versionText, out var latestVersion))
                throw new Exception($"Tag da release inválida: {release.TagName}");

            var zipAsset = release.Assets?
                .FirstOrDefault(a =>
                    a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
                    a.Name.Contains("CBL.PrintAssistant", StringComparison.OrdinalIgnoreCase));

            return new UpdateCheckResult
            {
                HasUpdate = latestVersion > currentVersion,
                LatestVersion = latestVersion,
                ReleaseName = release.Name ?? release.TagName,
                ReleaseNotes = release.Body ?? "",
                DownloadUrl = zipAsset?.BrowserDownloadUrl
            };
        }

        public async Task<string> DownloadAndPrepareUpdateAsync(
            string downloadUrl,
            string appDirectory,
            string exeName,
            Version targetVersion)
        {
            if (string.IsNullOrWhiteSpace(downloadUrl))
                throw new Exception("A release não possui asset ZIP para download.");

            string updatesRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CBL.PrintAssistant",
                "updates"
            );

            Directory.CreateDirectory(updatesRoot);

            string versionFolder = Path.Combine(updatesRoot, targetVersion.ToString());
            if (Directory.Exists(versionFolder))
                Directory.Delete(versionFolder, true);

            Directory.CreateDirectory(versionFolder);

            string zipPath = Path.Combine(versionFolder, "update.zip");
            string extractPath = Path.Combine(versionFolder, "extracted");

            using (var response = await _httpClient.GetAsync(downloadUrl))
            {
                response.EnsureSuccessStatusCode();

                await using var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);
            }

            ZipFile.ExtractToDirectory(zipPath, extractPath, true);

            string contentRoot = ResolveExtractedContentRoot(extractPath, exeName);

            string updateCmdPath = Path.Combine(versionFolder, "apply-update.cmd");

            string script = $@"@echo off
setlocal
timeout /t 2 /nobreak >nul

set ""SOURCE={contentRoot}""
set ""TARGET={appDirectory}""
set ""EXE={exeName}""

robocopy ""%SOURCE%"" ""%TARGET%"" /E /R:2 /W:1 /XF appconfig.json
start """" ""%TARGET%\%EXE%""
exit
";

            File.WriteAllText(updateCmdPath, script);

            return updateCmdPath;
        }

        public void LaunchUpdateScript(string scriptPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{scriptPath}\"",
                UseShellExecute = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? Environment.CurrentDirectory
            };

            Process.Start(psi);
        }

        private static string ResolveExtractedContentRoot(string extractPath, string exeName)
        {
            string exeAtRoot = Path.Combine(extractPath, exeName);
            if (File.Exists(exeAtRoot))
                return extractPath;

            var directories = Directory.GetDirectories(extractPath);
            foreach (var dir in directories)
            {
                string nestedExe = Path.Combine(dir, exeName);
                if (File.Exists(nestedExe))
                    return dir;
            }

            return extractPath;
        }

        private static string NormalizeVersion(string? tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return "0.0.0";

            string value = tag.Trim();

            if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                value = value.Substring(1);

            return value;
        }

        private class GitHubRelease
        {
            [JsonPropertyName("tag_name")]
            public string TagName { get; set; } = "";

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("body")]
            public string? Body { get; set; }

            [JsonPropertyName("assets")]
            public GitHubAsset[]? Assets { get; set; }
        }

        private class GitHubAsset
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = "";

            [JsonPropertyName("browser_download_url")]
            public string BrowserDownloadUrl { get; set; } = "";
        }
    }

    public class UpdateCheckResult
    {
        public bool HasUpdate { get; set; }
        public Version LatestVersion { get; set; } = new Version(0, 0, 0);
        public string ReleaseName { get; set; } = "";
        public string ReleaseNotes { get; set; } = "";
        public string? DownloadUrl { get; set; }
    }
}