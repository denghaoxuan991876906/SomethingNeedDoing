using SomethingNeedDoing.Core.Interfaces;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Represents a Git repository dependency.
/// </summary>
/// <param name="gitService">The Git service.</param>
/// <param name="repositoryUrl">The repository URL.</param>
/// <param name="branch">The branch name.</param>
/// <param name="path">The file path in the repository.</param>
/// <param name="name">The dependency name.</param>
public class GitDependency(IGitService gitService, string repositoryUrl, string branch, string? path, string name) : CachedDependency
{
    public override string Id { get; } = Guid.NewGuid().ToString();
    public override string Name { get; } = name;
    public override DependencyType Type => DependencyType.Remote;
    public override string Source => GitInfo.RepositoryUrl;
    public GitInfo GitInfo { get; } = new()
    {
        RepositoryUrl = repositoryUrl,
        Branch = branch,
        FilePath = path ?? string.Empty
    };

    /// <inheritdoc/>
    protected override async Task<string> DownloadContentAsync()
    {
        if (string.IsNullOrEmpty(GitInfo.FilePath))
            throw new InvalidOperationException("No file path specified for Git dependency");

        return await gitService.GetFileContentAsync(GitInfo.RepositoryUrl, GitInfo.Branch, GitInfo.FilePath);
    }

    /// <inheritdoc/>
    public override async Task<bool> IsAvailableAsync()
    {
        if (string.IsNullOrEmpty(GitInfo.FilePath))
            return false;

        return await gitService.FileExistsAsync(GitInfo.RepositoryUrl, GitInfo.Branch, GitInfo.FilePath);
    }

    /// <inheritdoc/>
    public override async Task<DependencyValidationResult> ValidateAsync()
    {
        if (string.IsNullOrEmpty(GitInfo.FilePath))
            return DependencyValidationResult.Failure("No file path specified");

        try
        {
            var exists = await gitService.FileExistsAsync(GitInfo.RepositoryUrl, GitInfo.Branch, GitInfo.FilePath);
            if (!exists)
                return DependencyValidationResult.Failure($"File not found in repository: {GitInfo.FilePath}");

            await gitService.GetFileContentAsync(GitInfo.RepositoryUrl, GitInfo.Branch, GitInfo.FilePath);
            return DependencyValidationResult.Success();
        }
        catch (Exception ex)
        {
            return DependencyValidationResult.Failure($"Error validating Git dependency: {ex.Message}");
        }
    }
}
