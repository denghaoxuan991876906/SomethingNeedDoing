using SomethingNeedDoing.Core.Interfaces;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Represents a Git repository dependency.
/// </summary>
public class GitDependency : CachedDependency
{
    private IGitService? _gitService;

    public override string Id { get; } = Guid.NewGuid().ToString();
    public override string Name { get; set; } = string.Empty;
    public override DependencyType Type => DependencyType.Remote;
    public override string Source { get; set; } = string.Empty;
    public GitInfo GitInfo { get; set; } = new();

    /// <summary>
    /// Parameterless constructor for JSON deserialization.
    /// </summary>
    public GitDependency() : base() { }

    /// <summary>
    /// Sets the Git service for this dependency.
    /// </summary>
    /// <param name="gitService">The Git service to use.</param>
    /// <remarks>The shit we do to ensure parameterless constructors</remarks>
    public void SetGitService(IGitService gitService) => _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));

    /// <inheritdoc/>
    protected override async Task<string> DownloadContentAsync()
    {
        if (_gitService == null)
            throw new InvalidOperationException("GitService not set. Call SetGitService() before using this dependency.");

        FrameworkLogger.Debug($"Downloading content from repo: [{GitInfo.RepositoryUrl}], branch: [{GitInfo.Branch}], path: [{GitInfo.FilePath}]");

        if (string.IsNullOrEmpty(GitInfo.FilePath))
            throw new InvalidOperationException("No file path specified for Git dependency");

        return await _gitService.GetFileContentAsync(GitInfo.RepositoryUrl, GitInfo.Branch, GitInfo.FilePath);
    }

    /// <inheritdoc/>
    public override async Task<bool> IsAvailableAsync()
    {
        if (_gitService == null)
            throw new InvalidOperationException("GitService not set. Call SetGitService() before using this dependency.");

        if (string.IsNullOrEmpty(GitInfo.FilePath))
            return false;

        return await _gitService.FileExistsAsync(GitInfo.RepositoryUrl, GitInfo.Branch, GitInfo.FilePath);
    }

    /// <inheritdoc/>
    public override async Task<DependencyValidationResult> ValidateAsync()
    {
        if (_gitService == null)
            throw new InvalidOperationException("GitService not set. Call SetGitService() before using this dependency.");

        if (string.IsNullOrEmpty(GitInfo.FilePath))
            return DependencyValidationResult.Failure("No file path specified");

        try
        {
            var exists = await _gitService.FileExistsAsync(GitInfo.RepositoryUrl, GitInfo.Branch, GitInfo.FilePath);
            if (!exists)
                return DependencyValidationResult.Failure($"File not found in repository: {GitInfo.FilePath}");

            await _gitService.GetFileContentAsync(GitInfo.RepositoryUrl, GitInfo.Branch, GitInfo.FilePath);
            return DependencyValidationResult.Success();
        }
        catch (Exception ex)
        {
            return DependencyValidationResult.Failure($"Error validating Git dependency: {ex.Message}");
        }
    }
}
