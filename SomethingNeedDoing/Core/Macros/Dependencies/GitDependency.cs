using SomethingNeedDoing.Core.Interfaces;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Represents a Git repository dependency.
/// </summary>
public class GitDependency(IGitService gitService, string repositoryUrl, string branch, string? path, string name) : IMacroDependency
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; } = name;
    public DependencyType Type => DependencyType.Remote;
    public string Source => GitInfo.RepositoryUrl;
    public GitInfo GitInfo { get; } = new()
    {
        RepositoryUrl = repositoryUrl,
        Branch = branch,
        FilePath = path ?? string.Empty
    };

    public async Task<string> GetContentAsync()
    {
        if (string.IsNullOrEmpty(GitInfo.FilePath))
            throw new InvalidOperationException("No file path specified for Git dependency");

        return await gitService.GetFileContentAsync(GitInfo.RepositoryUrl, GitInfo.Branch, GitInfo.FilePath);
    }

    public async Task<bool> IsAvailableAsync()
    {
        if (string.IsNullOrEmpty(GitInfo.FilePath))
            return false;

        return await gitService.FileExistsAsync(GitInfo.RepositoryUrl, GitInfo.Branch, GitInfo.FilePath);
    }

    public async Task<DependencyValidationResult> ValidateAsync()
    {
        if (string.IsNullOrEmpty(GitInfo.FilePath))
            return DependencyValidationResult.Failure("No file path specified");

        try
        {
            var exists = await gitService.FileExistsAsync(GitInfo.RepositoryUrl, GitInfo.Branch, GitInfo.FilePath);
            if (!exists)
                return DependencyValidationResult.Failure($"File not found in repository: {GitInfo.FilePath}");

            // Try to get the content to validate it
            await gitService.GetFileContentAsync(GitInfo.RepositoryUrl, GitInfo.Branch, GitInfo.FilePath);
            return DependencyValidationResult.Success();
        }
        catch (Exception ex)
        {
            return DependencyValidationResult.Failure($"Error validating Git dependency: {ex.Message}");
        }
    }
}
