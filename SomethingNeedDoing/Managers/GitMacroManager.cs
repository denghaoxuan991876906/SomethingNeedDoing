using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.Core.Github;
using SomethingNeedDoing.Framework.Interfaces;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Text.Json;
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
    private readonly ConcurrentDictionary<string, ConfigMacro> _gitMacros = [];

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
    /// <param name="dependencyFactory">The dependency factory.</param>
    public GitMacroManager(IMacroScheduler scheduler)
    {
        _scheduler = scheduler;
        _cacheDirectory = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "GitMacros");
        Directory.CreateDirectory(_cacheDirectory);
        UpdateAllMacros();
    }

    /// <summary>
    /// Initializes all Git macros by loading them from config and checking for updates.
    /// </summary>
    public void UpdateAllMacros()
    {
        foreach (var macro in C.Macros)
        {
            _gitMacros[macro.Id] = macro;
            _ = CheckForUpdates(macro); // Fire and forget, let it run async
        }
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
        var macro = new ConfigMacro
        {
            GitInfo =
            {
                RepositoryUrl = repositoryUrl,
                FilePath = filePath,
                Branch = branch
            }
        };

        await UpdateMacro(macro);
        _gitMacros[macro.Id] = macro;
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
        try
        {
            if (macro.GitInfo.RepositoryUrl.IsNullOrEmpty()) return;

            // Store current trigger events
            var triggerEvents = macro.Metadata.TriggerEvents.ToList();

            // Stop the macro and unsubscribe from events
            _scheduler.StopMacro(macro.Id);
            foreach (var triggerEvent in triggerEvents)
                _scheduler.UnsubscribeFromTriggerEvent(macro, triggerEvent);

            await UpdateMacro(macro);

            // Resubscribe
            foreach (var triggerEvent in triggerEvents)
                _scheduler.SubscribeToTriggerEvent(macro, triggerEvent);

            MacroUpdated?.Invoke(this, new GitMacroUpdateEventArgs(macro));
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to update macro {macro.Name}");
            MacroUpdateFailed?.Invoke(this, new GitMacroUpdateEventArgs(macro, ex));
        }
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
            MacroUpdated?.Invoke(this, new GitMacroUpdateEventArgs(macro));
            return true;
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to downgrade Git macro {macro.Name}");
            MacroUpdateFailed?.Invoke(this, new GitMacroUpdateEventArgs(macro, ex));
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
            MacroUpdated?.Invoke(this, new GitMacroUpdateEventArgs(macro));
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to update macro {macro.Name} to commit {commitHash}");
            MacroUpdateFailed?.Invoke(this, new GitMacroUpdateEventArgs(macro, ex));
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
        Svc.Log.Debug($"Original file path: {macro.GitInfo.FilePath}");
        Svc.Log.Debug($"Original repository URL: {macro.GitInfo.RepositoryUrl}");

        var filePath = macro.GitInfo.FilePath;

        // Clean up the file path - remove any blob/branch/ prefix if it exists
        if (filePath.StartsWith("blob/"))
        {
            filePath = filePath[(filePath.IndexOf('/', 5) + 1)..];
            Svc.Log.Debug($"Cleaned up file path from blob prefix: {filePath}");
        }
        // If no file path is provided, try to extract it from the URL
        else if (string.IsNullOrEmpty(filePath) && macro.GitInfo.RepositoryUrl.Contains("/blob/"))
        {
            var parts = macro.GitInfo.RepositoryUrl.Split(["/blob/"], StringSplitOptions.None);
            if (parts.Length != 2)
            {
                Svc.Log.Error($"Invalid repository URL format: {macro.GitInfo.RepositoryUrl}");
                throw new ArgumentException($"Invalid repository URL format: {macro.GitInfo.RepositoryUrl}");
            }

            var afterBlob = parts[1];
            var pathParts = afterBlob.Split('/', 2);
            if (pathParts.Length != 2)
            {
                Svc.Log.Error($"Invalid URL format after blob: {afterBlob}");
                throw new ArgumentException($"Invalid URL format after blob: {afterBlob}");
            }

            filePath = pathParts[1];
            Svc.Log.Debug($"Extracted file path from web URL: {filePath}");
        }
        else if (string.IsNullOrEmpty(filePath))
        {
            Svc.Log.Error("No file path provided and could not extract from URL");
            throw new ArgumentException("No file path provided and could not extract from URL");
        }

        var (ownerAndRepo, _) = GetOwnerAndRepo(macro.GitInfo.RepositoryUrl);
        var apiUrl = $"https://api.github.com/repos/{ownerAndRepo}/contents/{filePath}?ref={commitHash}";
        Svc.Log.Debug($"Downloading file from: {apiUrl}");

        var response = await _httpClient.GetAsync(apiUrl);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Svc.Log.Error($"Failed to download file. Status: {response.StatusCode}, Response: {responseContent}");
            throw new HttpRequestException($"Failed to download file: {responseContent}");
        }

        var fileInfo = JsonSerializer.Deserialize<GitHubFileInfo>(responseContent);
        return fileInfo?.Content ?? throw new InvalidOperationException("Failed to get file content");
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
            // Handle GitHub URLs
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
                    Svc.Log.Debug($"Extracted from web URL - Owner: {owner}, Repo: {repo}, Branch: {branch}");
                    return ($"{owner}/{repo}", branch);
                }

                // For regular GitHub URLs, get the default branch
                Svc.Log.Debug($"Using repository URL - Owner: {owner}, Repo: {repo}");
                return ($"{owner}/{repo}", "master"); // Default to master for GitHub URLs
            }

            // Handle SSH URLs
            if (repositoryUrl.StartsWith("git@"))
            {
                var parts = repositoryUrl.Split(':');
                var path = parts[1].Replace(".git", "");
                Svc.Log.Debug($"Using default branch 'master' for SSH URL");
                return (path, "master");
            }

            // Handle other HTTPS URLs
            var uri2 = new Uri(repositoryUrl);
            var pathParts2 = uri2.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathParts2.Length < 2)
                throw new ArgumentException($"Invalid repository URL format: {repositoryUrl}");

            var owner2 = pathParts2[0];
            var repo2 = pathParts2[1].Replace(".git", "");
            Svc.Log.Debug($"Using default branch 'master' for HTTPS URL");
            return ($"{owner2}/{repo2}", "master");
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
        if (macro.GitInfo.RepositoryUrl.IsNullOrEmpty()) return;
        try
        {
            // Get the latest commit hash
            var commitHash = specificCommit ?? await GetLatestCommitHash(macro);

            // If the commit hash hasn't changed and we have a cached version, use that
            if (commitHash == macro.GitInfo.CommitHash && File.Exists(GetCachedFilePath(macro)))
            {
                macro.Content = await File.ReadAllTextAsync(GetCachedFilePath(macro));
                return;
            }

            // Download the new version
            var content = await DownloadFile(macro, commitHash);
            await File.WriteAllTextAsync(GetCachedFilePath(macro), content);

            // Parse metadata and update macro
            var metadata = GitMacroMetadataParser.ParseMetadata(content);
            macro.Content = content;
            macro.GitInfo.CommitHash = commitHash;
            macro.Metadata = metadata;
            macro.GitInfo.LastUpdateCheck = DateTime.Now;

            // Update dependencies if any
            await UpdateDependencies(macro);
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to update macro {macro.Name}");
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
            if (dependency.Type == DependencyType.GitMacro)
            {
                var gitDependency = (GitMacroDependency)dependency;
                var depMacro = new ConfigMacro
                {
                    GitInfo =
                    {
                        RepositoryUrl = gitDependency.RepositoryUrl,
                        FilePath = gitDependency.FilePath,
                        Branch = gitDependency.Branch
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

    private class GitHubFileInfo
    {
        public string Content { get; set; } = string.Empty;
        public string Encoding { get; set; } = string.Empty;
    }
}
