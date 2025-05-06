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
    private readonly HttpClient _httpClient = new();
    private readonly string _cacheDirectory;
    private readonly ConcurrentDictionary<string, GitMacro> _gitMacros = [];

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
        foreach (var macro in C.GitMacros)
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
    public async Task<GitMacro> AddGitMacro(string repositoryUrl, string filePath, string branch = "main")
    {
        var macro = new GitMacro
        {
            RepositoryUrl = repositoryUrl,
            FilePath = filePath,
            Branch = branch
        };

        await UpdateMacro(macro);
        _gitMacros[macro.Id] = macro;
        C.GitMacros.Add(macro);
        C.Save();

        return macro;
    }

    /// <summary>
    /// Checks for updates to a macro.
    /// </summary>
    /// <param name="macro">The macro to check.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CheckForUpdates(GitMacro macro)
    {
        try
        {
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
    public async Task<List<GitCommitInfo>> GetVersionHistory(GitMacro macro)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{macro.RepositoryUrl}/commits?path={macro.FilePath}&sha={macro.Branch}");

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
    public async Task<bool> DowngradeToVersion(GitMacro macro, string commitHash)
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
    public async Task UpdateToCommit(GitMacro macro, string commitHash)
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
    private async Task<string> GetLatestCommitHash(GitMacro macro)
    {
        var apiUrl = $"https://api.github.com/repos/{GetOwnerAndRepo(macro.RepositoryUrl)}/commits/{macro.Branch}";
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var commitInfo = JsonSerializer.Deserialize<GitCommitInfo>(content);
        return commitInfo?.CommitHash ?? throw new InvalidOperationException("Failed to get commit hash");
    }

    /// <summary>
    /// Downloads a file from a Git repository.
    /// </summary>
    /// <param name="macro">The macro to download.</param>
    /// <param name="commitHash">The commit hash to download from.</param>
    /// <returns>The file content.</returns>
    private async Task<string> DownloadFile(GitMacro macro, string commitHash)
    {
        var apiUrl = $"https://api.github.com/repos/{GetOwnerAndRepo(macro.RepositoryUrl)}/contents/{macro.FilePath}?ref={commitHash}";
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var fileInfo = JsonSerializer.Deserialize<GitHubFileInfo>(content);
        return fileInfo?.Content ?? throw new InvalidOperationException("Failed to get file content");
    }

    /// <summary>
    /// Gets the owner and repository name from a repository URL.
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <returns>The owner and repository name.</returns>
    private static string GetOwnerAndRepo(string repositoryUrl)
    {
        var uri = new Uri(repositoryUrl);
        var pathParts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var owner = pathParts[0];
        var repo = pathParts[1].Replace(".git", "");
        return $"{owner}/{repo}";
    }

    /// <summary>
    /// Updates a macro.
    /// </summary>
    /// <param name="macro">The macro to update.</param>
    /// <param name="specificCommit">The specific commit to update to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateMacro(GitMacro macro, string? specificCommit = null)
    {
        try
        {
            // Get the latest commit hash
            var commitHash = specificCommit ?? await GetLatestCommitHash(macro);

            // If the commit hash hasn't changed and we have a cached version, use that
            if (commitHash == macro.CommitHash && File.Exists(GetCachedFilePath(macro)))
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
            macro.CommitHash = commitHash;
            macro.Metadata = metadata;
            macro.LastUpdateCheck = DateTime.Now;

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
    private async Task UpdateDependencies(GitMacro macro)
    {
        foreach (var dependency in macro.Metadata.Dependencies)
        {
            if (dependency.Type == DependencyType.GitMacro)
            {
                var gitDependency = (GitMacroDependency)dependency;
                var depMacro = new GitMacro
                {
                    RepositoryUrl = gitDependency.RepositoryUrl,
                    FilePath = gitDependency.FilePath,
                    Branch = gitDependency.Branch
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

    private string GetCachedFilePath(GitMacro macro)
    {
        var fileName = $"{macro.Id}_{macro.CommitHash}.txt";
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
