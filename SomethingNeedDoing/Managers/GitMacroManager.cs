using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.Core.Github;
using SomethingNeedDoing.Core.Interfaces;
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
    public GitMacroManager(IMacroScheduler scheduler, IGitService gitService)
    {
        _scheduler = scheduler;
        _ = UpdateAllMacros();

        // TODO: This is like this because having a parametered construct meant that IMacroDependencies couldn't be deserialized by the json deserializer
        foreach (var macro in C.Macros)
            foreach (var dependency in macro.Metadata.Dependencies)
                if (dependency is GitDependency gitDependency)
                    gitDependency.SetGitService(gitService);
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
    /// Adds a new Git macro from a full GitHub URL.
    /// </summary>
    /// <param name="githubUrl">The full GitHub URL (e.g., https://github.com/owner/repo/blob/branch/path)</param>
    /// <returns>The created Git macro.</returns>
    public async Task<ConfigMacro> AddGitMacroFromUrl(string githubUrl)
    {
        var (repositoryUrl, filePath, branch) = ParseGitHubUrl(githubUrl);
        if (string.IsNullOrEmpty(repositoryUrl) || string.IsNullOrEmpty(filePath))
            throw new ArgumentException("Invalid GitHub URL. Please use format: https://github.com/owner/repo/blob/branch/path");

        // Store the full GitHub URL in RepositoryUrl for UI consistency
        var macro = new ConfigMacro
        {
            GitInfo =
            {
                RepositoryUrl = githubUrl, // Store the full URL
                FilePath = filePath,
                Branch = branch
            }
        };

        await UpdateMacro(macro);
        C.Macros.Add(macro);
        C.Save();

        return macro;
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
        var (owner, repo) = GetOwnerAndRepo(repositoryUrl);
        if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            throw new ArgumentException("Invalid repository URL");

        var macro = new ConfigMacro
        {
            GitInfo =
            {
                RepositoryUrl = repositoryUrl,
                FilePath = filePath,
                Branch = branch,
                Owner = owner,
                Repo = repo
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
        if (DateTime.Now - macro.GitInfo.LastUpdateCheck < _updateCooldown)
        {
            Svc.Log.Info($"Skipping update check for {macro.Name} (last checked {macro.GitInfo.LastUpdateCheck})");
            return;
        }

        var (owner, repo) = GetOwnerAndRepo(macro.GitInfo.RepositoryUrl);
        if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            return;

        // Remove .git suffix if present
        repo = repo.EndsWith(".git") ? repo[..^4] : repo;

        var filePath = macro.GitInfo.FilePath;
        Svc.Log.Debug($"Checking for updates for {macro.Name}");
        Svc.Log.Debug($"File: {filePath}");

        var url = $"https://api.github.com/repos/{owner}/{repo}/commits?path={Uri.EscapeDataString(filePath)}&per_page=1";
        Svc.Log.Debug($"Requesting: {url}");

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Svc.Log.Error($"Failed to check for updates: {response.StatusCode} - {error}");
            return;
        }

        var content = await response.Content.ReadAsStringAsync();
        var commits = JsonSerializer.Deserialize<List<JsonElement>>(content) ?? [];

        if (commits.Count > 0)
        {
            var latestCommit = commits[0].GetProperty("sha").GetString();
            if (latestCommit != macro.GitInfo.CommitHash)
            {
                macro.GitInfo.HasUpdate = true;
                Svc.Log.Info($"Update available for {macro.Name} ({macro.GitInfo.CommitHash} → {latestCommit})");
            }
            else
            {
                macro.GitInfo.HasUpdate = false;
                Svc.Log.Info($"No updates available for {macro.Name} ({macro.GitInfo.CommitHash} == {latestCommit})");
            }
        }

        macro.GitInfo.LastUpdateCheck = DateTime.Now;
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

    public class GitCommit
    {
        public string Hash { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Url { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
    }

    public async Task<List<GitCommit>> GetCommitHistory(ConfigMacro macro)
    {
        if (!macro.IsGitMacro)
            return [];

        var (owner, repo) = GetOwnerAndRepo(macro.GitInfo.RepositoryUrl);
        if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
        {
            Svc.Log.Error($"Invalid repository URL: {macro.GitInfo.RepositoryUrl}");
            return [];
        }

        Svc.Log.Debug($"Getting commit history for {macro.Name}");
        Svc.Log.Debug($"Repository URL: {macro.GitInfo.RepositoryUrl}");
        Svc.Log.Debug($"Extracted - Owner: {owner}, Repo: {repo}");

        // Remove .git suffix if present
        repo = repo.EndsWith(".git") ? repo[..^4] : repo;

        var filePath = macro.GitInfo.FilePath;
        Svc.Log.Debug($"File path: {filePath}");

        var url = $"https://api.github.com/repos/{owner}/{repo}/commits?path={Uri.EscapeDataString(filePath)}";
        Svc.Log.Debug($"Requesting: {url}");

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Svc.Log.Error($"Failed to get commit history: {response.StatusCode} - {error}");
            Svc.Log.Error($"Request URL: {url}");
            Svc.Log.Error($"Repository URL: {macro.GitInfo.RepositoryUrl}");
            Svc.Log.Error($"Owner: {owner}, Repo: {repo}, File: {filePath}");
            throw new Exception($"Failed to get commit history: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var commits = JsonSerializer.Deserialize<List<JsonElement>>(content) ?? [];

        Svc.Log.Debug($"Found {commits.Count} commits for file {filePath}");

        return [.. commits.Select(c => new GitCommit
        {
            Hash = c.GetProperty("sha").GetString() ?? string.Empty,
            Message = c.GetProperty("commit").GetProperty("message").GetString() ?? string.Empty,
            Author = c.GetProperty("author").GetProperty("login").GetString() ?? string.Empty,
            Date = c.GetProperty("commit").GetProperty("author").GetProperty("date").GetDateTime(),
            Url = c.GetProperty("url").GetString() ?? string.Empty,
            HtmlUrl = c.GetProperty("html_url").GetString() ?? string.Empty
        })];
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
    /// Gets the owner and repository name from a repository URL.
    /// </summary>
    /// <param name="url">The repository URL (can be full GitHub URL or base repo URL).</param>
    /// <returns>The owner and repository name.</returns>
    private (string owner, string repo) GetOwnerAndRepo(string url)
    {
        if (string.IsNullOrEmpty(url))
            return (string.Empty, string.Empty);

        try
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 2)
                return (string.Empty, string.Empty);

            // Always return the first two segments (owner/repo) regardless of URL type
            return (segments[0], segments[1]);
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to parse repository URL: {url}");
            return (string.Empty, string.Empty);
        }
    }

    /// <summary>
    /// Parses a full GitHub URL to extract repository URL, file path, and branch.
    /// </summary>
    /// <param name="url">The full GitHub URL (e.g., https://github.com/owner/repo/blob/branch/path)</param>
    /// <returns>The repository URL, file path, and branch.</returns>
    private (string repositoryUrl, string filePath, string branch) ParseGitHubUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 5 || segments[2] != "blob")
                return (string.Empty, string.Empty, "main");

            var owner = segments[0];
            var repo = segments[1];
            var branch = segments[3];
            var filePath = string.Join("/", segments[4..]);

            var repositoryUrl = $"https://github.com/{owner}/{repo}";
            return (repositoryUrl, filePath, branch);
        }
        catch
        {
            return (string.Empty, string.Empty, "main");
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
        if (!macro.IsGitMacro)
            return;

        var (owner, repo) = GetOwnerAndRepo(macro.GitInfo.RepositoryUrl);
        if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            return;

        repo = repo.EndsWith(".git") ? repo[..^4] : repo;

        var filePath = macro.GitInfo.FilePath;
        Svc.Log.Debug($"Updating macro {macro.Name}");
        Svc.Log.Debug($"File: {filePath}");

        string commitHash;
        if (specificCommit != null)
            commitHash = specificCommit;
        else
        {
            var url = $"https://api.github.com/repos/{owner}/{repo}/commits?path={Uri.EscapeDataString(filePath)}&per_page=1";
            Svc.Log.Debug($"Requesting: {url}");

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Svc.Log.Error($"Failed to get latest commit: {response.StatusCode} - {error}");
                throw new Exception($"Failed to get latest commit: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var commits = JsonSerializer.Deserialize<List<JsonElement>>(content) ?? [];
            if (commits.Count == 0)
            {
                Svc.Log.Error("No commits found for file");
                throw new Exception("No commits found for file");
            }

            commitHash = commits[0].GetProperty("sha").GetString() ?? throw new Exception("Failed to get commit hash");
        }

        // Download the file content
        var fileUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{macro.GitInfo.Branch}/{filePath}";
        Svc.Log.Debug($"Downloading file from: {fileUrl}");

        var fileResponse = await _httpClient.GetAsync(fileUrl);
        if (!fileResponse.IsSuccessStatusCode)
        {
            var error = await fileResponse.Content.ReadAsStringAsync();
            Svc.Log.Error($"Failed to download file: {fileResponse.StatusCode} - {error} from url [{fileUrl}] for macro [{macro.Name}]");
            throw new Exception($"Failed to download file: {fileResponse.StatusCode}");
        }

        var fileContent = await fileResponse.Content.ReadAsStringAsync();

        // Store current trigger events, unsubscribe, update, and resubscribe
        var currentTriggers = macro.Metadata.TriggerEvents.ToList();
        _scheduler.StopMacro(macro.Id);
        currentTriggers.ForEach(t => _scheduler.UnsubscribeFromTriggerEvent(macro, t));

        // Update the macro
        macro.Content = fileContent;
        macro.GitInfo.CommitHash = commitHash;
        macro.GitInfo.HasUpdate = false;
        macro.GitInfo.LastUpdateCheck = DateTime.Now;

        // Update dependencies
        await UpdateDependencies(macro);

        currentTriggers.ForEach(t => _scheduler.SubscribeToTriggerEvent(macro, t));

        C.Save();
        Svc.Log.Info($"Successfully updated macro {macro.Name} to commit {commitHash}");
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

    /// <inheritdoc/>
    public void Dispose() => _httpClient.Dispose();
}
