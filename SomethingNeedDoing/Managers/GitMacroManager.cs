using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.Core.Github;
using SomethingNeedDoing.Core.Interfaces;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Managers;

/// <summary>
/// Manages Git macros, including downloading, updating, and version control.
/// </summary>
public class GitMacroManager : IDisposable
{
    private readonly IMacroScheduler _scheduler;
    private readonly HttpClient _httpClient = new()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "SomethingNeedDoing/1.0" }
        }
    };
    private readonly string _cacheDirectory;
    private readonly GitMacroMetadataParser _metadataParser;
    private readonly TimeSpan _updateCooldown = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Event raised when a macro is updated.
    /// </summary>
    public event EventHandler<GitMacroUpdateEventArgs>? MacroUpdated;

    /// <summary>
    /// Event raised when a macro update fails.
    /// </summary>
    public event EventHandler<GitMacroUpdateEventArgs>? MacroUpdateFailed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitMacroManager"/> class.
    /// </summary>
    /// <param name="scheduler">The macro scheduler.</param>
    /// <param name="dependencyFactory">The dependency factory.</param>
    public GitMacroManager(IMacroScheduler scheduler, GitMacroMetadataParser metadataParser)
    {
        _scheduler = scheduler;
        _metadataParser = metadataParser;
        _cacheDirectory = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "GitMacros");
        Directory.CreateDirectory(_cacheDirectory);
        _ = UpdateAllMacros();
    }

    /// <summary>
    /// Initializes all Git macros by loading them from config and checking for updates.
    /// </summary>
    public async Task UpdateAllMacros()
    {
        await CheckForUpdates();
        await Task.WhenAll([.. C.Macros.Where(m => m.GitInfo.HasUpdate).Select(m => UpdateMacro(m, null))]);
    }

    /// <summary>
    /// Adds a new Git macro.
    /// </summary>
    /// <param name="repositoryUrl">The Git repository URL.</param>
    /// <param name="filePath">The file path within the repository.</param>
    /// <param name="branch">The branch name.</param>
    /// <returns>The created Git macro.</returns>
    public async Task<ConfigMacro> AddGitMacro(string repositoryUrl, string filePath, string branch = "main")
    {
        var (ownerAndRepo, _) = GetOwnerAndRepo(repositoryUrl);
        var parts = ownerAndRepo.Split('/');

        var macro = new ConfigMacro
        {
            GitInfo =
            {
                RepositoryUrl = repositoryUrl,
                FilePath = filePath,
                Branch = branch,
                Owner = parts[0],
                Repo = parts[1]
            }
        };

        await UpdateMacro(macro);
        C.Macros.Add(macro);
        C.Save();

        return macro;
    }

    /// <summary>
    /// Checks for updates to a macro.
    /// </summary>
    /// <param name="macro">The macro to check.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CheckForUpdates(ConfigMacro macro)
    {
        if (!macro.IsGitMacro) return;
        if (DateTime.Now - macro.GitInfo.LastUpdateCheck > _updateCooldown)
        {
            try
            {
                var (ownerAndRepo, _) = GetOwnerAndRepo(macro.GitInfo.RepositoryUrl);
                var parts = ownerAndRepo.Split('/');
                macro.GitInfo.Owner = parts[0];
                macro.GitInfo.Repo = parts[1];

                var latestCommit = await GetLatestCommitHash(macro);
                macro.GitInfo.HasUpdate = latestCommit != macro.GitInfo.CommitHash;
                macro.GitInfo.LastUpdateCheck = DateTime.Now;

                if (macro.GitInfo.HasUpdate)
                    Svc.Log.Debug($"Update available for {macro.Name} ({macro.GitInfo.CommitHash} → {latestCommit})");
                else
                    Svc.Log.Debug($"No updates available for {macro.Name} ({macro.GitInfo.CommitHash} == {latestCommit})");
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, $"Failed to check for updates for macro {macro.Name}");
            }
        }
        else
            Svc.Log.Debug($"Skipping update check for {macro.Name} due to cooldown. Next check available in {_updateCooldown - (DateTime.Now - macro.GitInfo.LastUpdateCheck)}");
    }

    /// <summary>
    /// Checks for updates to all macros.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CheckForUpdates()
    {
        foreach (var macro in C.Macros.Where(m => m.IsGitMacro))
            await CheckForUpdates(macro);
    }

    public async Task UpdateMacro(ConfigMacro macro)
    {
        await CheckForUpdates(macro);
        if (macro.GitInfo.HasUpdate)
            await UpdateMacro(macro, null);
    }

    /// <summary>
    /// Gets the version history for a macro.
    /// </summary>
    /// <param name="macro">The macro to get version history for.</param>
    /// <returns>A list of commit information.</returns>
    public async Task<List<GitCommitInfo>> GetVersionHistory(ConfigMacro macro)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{macro.GitInfo.RepositoryUrl}/commits?path={macro.GitInfo.FilePath}&sha={macro.GitInfo.Branch}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to get version history: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            var commits = JsonSerializer.Deserialize<List<GitCommitInfo>>(content);

            return commits ?? [];
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to get version history for {macro.Name}");
            return [];
        }
    }

    /// <summary>
    /// Downgrades a macro to a specific version.
    /// </summary>
    /// <param name="macro">The macro to downgrade.</param>
    /// <param name="commitHash">The commit hash to downgrade to.</param>
    /// <returns>True if the downgrade was successful.</returns>
    public async Task<bool> DowngradeToVersion(ConfigMacro macro, string commitHash)
    {
        try
        {
            await UpdateMacro(macro, commitHash);
            return true;
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to downgrade Git macro {macro.Name}");
            return false;
        }
    }

    /// <summary>
    /// Updates a macro to a specific commit.
    /// </summary>
    /// <param name="macro">The macro to update.</param>
    /// <param name="commitHash">The commit hash to update to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateToCommit(ConfigMacro macro, string commitHash)
    {
        try
        {
            await UpdateMacro(macro, commitHash);
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to update macro {macro.Name} to commit {commitHash}");
        }
    }

    /// <summary>
    /// Gets the latest commit hash for a macro.
    /// </summary>
    /// <param name="macro">The macro to get the commit hash for.</param>
    /// <returns>The latest commit hash.</returns>
    private async Task<string> GetLatestCommitHash(ConfigMacro macro)
    {
        var (ownerAndRepo, branch) = GetOwnerAndRepo(macro.GitInfo.RepositoryUrl);
        Svc.Log.Debug($"Getting latest commit for {ownerAndRepo} on branch {branch}");

        // First try to get the default branch
        var apiUrl = $"https://api.github.com/repos/{ownerAndRepo}";
        var response = await _httpClient.GetAsync(apiUrl);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Svc.Log.Error($"Failed to get repository info. Status: {response.StatusCode}, Response: {responseContent}");
            throw new HttpRequestException($"Failed to get repository info: {responseContent}");
        }

        using var jsonDoc = JsonDocument.Parse(responseContent);
        var root = jsonDoc.RootElement;
        var defaultBranch = root.GetProperty("default_branch").GetString();

        // Use the default branch if the specified branch wasn't found
        if (branch != defaultBranch)
        {
            Svc.Log.Debug($"Branch {branch} not found, using default branch {defaultBranch}");
            branch = defaultBranch;
        }

        // Now get the latest commit for the branch
        apiUrl = $"https://api.github.com/repos/{ownerAndRepo}/branches/{branch}";
        Svc.Log.Debug($"Getting branch info from: {apiUrl}");

        response = await _httpClient.GetAsync(apiUrl);
        responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Svc.Log.Error($"GitHub API request failed. Status: {response.StatusCode}, Response: {responseContent}");
            throw new HttpRequestException($"GitHub API request failed: {responseContent}");
        }

        using var branchDoc = JsonDocument.Parse(responseContent);
        var branchRoot = branchDoc.RootElement;
        var commit = branchRoot.GetProperty("commit");
        var sha = commit.GetProperty("sha").GetString();

        return sha ?? throw new InvalidOperationException("Failed to get commit hash");
    }

    /// <summary>
    /// Downloads a file from a Git repository.
    /// </summary>
    /// <param name="macro">The macro to download.</param>
    /// <param name="commitHash">The commit hash to download from.</param>
    /// <returns>The file content.</returns>
    private async Task<string> DownloadFile(ConfigMacro macro, string commitHash)
    {
        var filePath = macro.GitInfo.FilePath;
        Svc.Log.Debug($"Original file path: {filePath}");

        // First decode any existing URL encoding to avoid double-encoding
        var decodedPath = Uri.UnescapeDataString(filePath);
        Svc.Log.Debug($"Decoded file path: {decodedPath}");

        // Then encode each segment properly
        var encodedPath = string.Join("/", decodedPath.Split('/').Select(Uri.EscapeDataString));
        Svc.Log.Debug($"Encoded file path: {encodedPath}");

        var apiUrl = $"https://api.github.com/repos/{macro.GitInfo.Owner}/{macro.GitInfo.Repo}/contents/{encodedPath}?ref={commitHash}";
        Svc.Log.Debug($"Downloading file from: {apiUrl}");

        var response = await _httpClient.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Svc.Log.Error($"Failed to download file: {response.StatusCode} - {error}");
            throw new InvalidOperationException($"Failed to download file: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var fileContent = JsonSerializer.Deserialize<GitHubFileContent>(content);

        if (fileContent?.Content == null)
        {
            Svc.Log.Error("Failed to parse file content from GitHub response");
            throw new InvalidOperationException("Failed to parse file content from GitHub response");
        }

        var decodedContent = Encoding.UTF8.GetString(Convert.FromBase64String(fileContent.Content));
        Svc.Log.Debug($"Downloaded content length: {decodedContent.Length}");
        return decodedContent;
    }

    /// <summary>
    /// Gets the owner and repository name from a repository URL.
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <returns>The owner and repository name.</returns>
    private static (string OwnerAndRepo, string Branch) GetOwnerAndRepo(string repositoryUrl)
    {
        try
        {
            if (repositoryUrl.Contains("github.com"))
            {
                var uri = new Uri(repositoryUrl);
                var pathParts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                if (pathParts.Length < 2)
                    throw new ArgumentException($"Invalid GitHub URL format: {repositoryUrl}");

                var owner = pathParts[0];
                var repo = pathParts[1];

                // If it's a web interface URL (contains /blob/)
                if (repositoryUrl.Contains("/blob/"))
                {
                    if (pathParts.Length < 4)
                        throw new ArgumentException($"Invalid GitHub URL format: {repositoryUrl}");

                    var branch = pathParts[3]; // The branch is after /blob/
                    var filePath = Uri.UnescapeDataString(string.Join("/", pathParts[4..])); // Everything after the branch is the file path
                    Svc.Log.Debug($"Extracted from web URL - Owner: {owner}, Repo: {repo}, Branch: {branch}, FilePath: {filePath}");
                    return ($"{owner}/{repo}", branch);
                }

                // For regular GitHub URLs, get the default branch
                Svc.Log.Debug($"Using repository URL - Owner: {owner}, Repo: {repo}");
                return ($"{owner}/{repo}", "main"); // Default to main for GitHub URLs
            }

            if (repositoryUrl.StartsWith("git@"))
            {
                var parts = repositoryUrl.Split(':');
                var path = parts[1].Replace(".git", "");
                Svc.Log.Debug($"Using default branch 'main' for SSH URL");
                return (path, "main");
            }

            var uri2 = new Uri(repositoryUrl);
            var pathParts2 = uri2.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathParts2.Length < 2)
                throw new ArgumentException($"Invalid repository URL format: {repositoryUrl}");

            var owner2 = pathParts2[0];
            var repo2 = pathParts2[1].Replace(".git", "");
            Svc.Log.Debug($"Using default branch 'main' for HTTPS URL");
            return ($"{owner2}/{repo2}", "main");
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to parse repository URL: {repositoryUrl}", ex);
        }
    }

    /// <summary>
    /// Updates a macro.
    /// </summary>
    /// <param name="macro">The macro to update.</param>
    /// <param name="specificCommit">The specific commit to update to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateMacro(ConfigMacro macro, string? specificCommit = null)
    {
        if (!macro.IsGitMacro || !macro.GitInfo.HasUpdate) return;
        try
        {
            Svc.Log.Debug($"Starting update for macro {macro.Name}");

            // Store current trigger events
            var triggerEvents = macro.Metadata.TriggerEvents.ToList();

            // Stop the macro and unsubscribe from events
            _scheduler.StopMacro(macro.Id);
            foreach (var triggerEvent in triggerEvents)
                _scheduler.UnsubscribeFromTriggerEvent(macro, triggerEvent);

            var commitHash = specificCommit ?? await GetLatestCommitHash(macro);
            Svc.Log.Debug($"Got commit hash: {commitHash}");

            var cachePath = GetCachedFilePath(macro);
            var useCache = commitHash == macro.GitInfo.CommitHash && File.Exists(cachePath);

            if (useCache)
            {
                var cachedContent = await File.ReadAllTextAsync(cachePath);
                if (!string.IsNullOrEmpty(cachedContent))
                {
                    Svc.Log.Debug("Using cached version");
                    macro.Content = cachedContent;
                    return;
                }
                Svc.Log.Debug("Cache file is empty, downloading new version");
            }

            Svc.Log.Debug("Downloading new version");
            var content = await DownloadFile(macro, commitHash);
            Svc.Log.Debug($"Downloaded content length: {content.Length}");

            if (string.IsNullOrEmpty(content))
            {
                Svc.Log.Error("Downloaded content is empty");
                throw new InvalidOperationException("Downloaded content is empty");
            }

            Svc.Log.Debug($"Caching to: {cachePath}");
            await File.WriteAllTextAsync(cachePath, content);

            Svc.Log.Debug("Parsing metadata");
            var metadata = _metadataParser.ParseMetadata(content);
            macro.Content = content;
            macro.GitInfo.CommitHash = commitHash;
            macro.Metadata = metadata;
            macro.GitInfo.LastUpdateCheck = DateTime.Now;
            Svc.Log.Debug("Update complete");

            await UpdateDependencies(macro);

            // Resubscribe to trigger events
            foreach (var triggerEvent in triggerEvents)
                _scheduler.SubscribeToTriggerEvent(macro, triggerEvent);

            MacroUpdated?.Invoke(this, new GitMacroUpdateEventArgs(macro));
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to update macro {macro.Name}");
            MacroUpdateFailed?.Invoke(this, new GitMacroUpdateEventArgs(macro, ex));
            throw;
        }
    }

    /// <summary>
    /// Updates the dependencies of a macro.
    /// </summary>
    /// <param name="macro">The macro to update dependencies for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateDependencies(ConfigMacro macro)
    {
        foreach (var dependency in macro.Metadata.Dependencies)
        {
            if (dependency.Type == DependencyType.Remote && dependency.Source.StartsWith("git://"))
            {
                var depMacro = new ConfigMacro
                {
                    GitInfo =
                    {
                        RepositoryUrl = dependency.Source[6..], // Remove "git://" prefix
                        FilePath = dependency.Name,
                        Branch = "main" // Default to main branch
                    }
                };

                await UpdateMacro(depMacro);
            }
            else
            {
                // For non-Git dependencies, just ensure they're available
                if (!await dependency.IsAvailableAsync())
                    throw new InvalidOperationException($"Dependency {dependency.Name} is not available");
            }
        }
    }

    private string GetCachedFilePath(ConfigMacro macro)
    {
        var fileName = $"{macro.Id}_{macro.GitInfo.CommitHash}.txt";
        return Path.Combine(_cacheDirectory, fileName);
    }

    /// <inheritdoc/>
    public void Dispose() => _httpClient.Dispose();

    private class GitHubFileContent
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("encoding")]
        public string? Encoding { get; set; }

        [JsonPropertyName("sha")]
        public string? Sha { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
