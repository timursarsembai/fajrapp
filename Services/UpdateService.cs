using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace FajrApp.Services;

public class UpdateInfo
{
    public string Version { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public string ReleaseNotes { get; set; } = "";
    public string HtmlUrl { get; set; } = "";
    public bool IsNewVersion { get; set; }
}

public static class UpdateService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };
    
    private const string GitHubApiUrl = "https://api.github.com/repos/timursarsembai/fajrapp/releases/latest";
    private const string CurrentVersion = "1.3.0";
    
    static UpdateService()
    {
        // GitHub API requires User-Agent header
        HttpClient.DefaultRequestHeaders.Add("User-Agent", "FajrApp");
    }
    
    /// <summary>
    /// Check for updates from GitHub releases
    /// </summary>
    public static async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            var response = await HttpClient.GetStringAsync(GitHubApiUrl);
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            
            var tagName = root.GetProperty("tag_name").GetString() ?? "";
            var version = tagName.TrimStart('v', 'V');
            
            var htmlUrl = root.GetProperty("html_url").GetString() ?? "";
            var body = root.TryGetProperty("body", out var bodyProp) ? bodyProp.GetString() ?? "" : "";
            
            // Find the installer download URL
            var downloadUrl = "";
            if (root.TryGetProperty("assets", out var assets))
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString() ?? "";
                    if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && 
                        name.Contains("Setup", StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                        break;
                    }
                }
            }
            
            return new UpdateInfo
            {
                Version = version,
                DownloadUrl = downloadUrl,
                ReleaseNotes = body,
                HtmlUrl = htmlUrl,
                IsNewVersion = IsNewerVersion(version, CurrentVersion)
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update check failed: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Compare two version strings (e.g., "1.2.0" vs "1.1.0")
    /// </summary>
    private static bool IsNewerVersion(string remoteVersion, string currentVersion)
    {
        try
        {
            var remote = Version.Parse(remoteVersion);
            var current = Version.Parse(currentVersion);
            return remote > current;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Download and run the installer
    /// </summary>
    public static async Task<bool> DownloadAndInstallAsync(string downloadUrl, Action<int>? progressCallback = null)
    {
        try
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), "FajrApp_Update");
            Directory.CreateDirectory(tempFolder);
            
            var fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
            var filePath = Path.Combine(tempFolder, fileName);
            
            // Download with progress
            using var response = await HttpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            var downloadedBytes = 0L;
            
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            
            var buffer = new byte[8192];
            int bytesRead;
            
            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                downloadedBytes += bytesRead;
                
                if (totalBytes > 0)
                {
                    var progress = (int)(downloadedBytes * 100 / totalBytes);
                    progressCallback?.Invoke(progress);
                }
            }
            
            // Run the installer
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
            
            // Close the application to allow update
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Download failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Open the releases page in browser
    /// </summary>
    public static void OpenReleasesPage(string? url = null)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url ?? "https://github.com/timursarsembai/fajrapp/releases",
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore
        }
    }
    
    public static string GetCurrentVersion() => CurrentVersion;
}
