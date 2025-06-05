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
    public string Source => repositoryUrl;

    public async Task<string> GetContentAsync()
    {
        if (string.IsNullOrEmpty(path))
            throw new InvalidOperationException("No file path specified for Git dependency");

        return await gitService.GetFileContentAsync(repositoryUrl, branch, path);
    }

    public async Task<bool> IsAvailableAsync()
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return await gitService.FileExistsAsync(repositoryUrl, branch, path);
    }

    public async Task<DependencyValidationResult> ValidateAsync()
    {
        if (string.IsNullOrEmpty(path))
            return DependencyValidationResult.Failure("No file path specified");

        try
        {
            var exists = await gitService.FileExistsAsync(repositoryUrl, branch, path);
            if (!exists)
                return DependencyValidationResult.Failure($"File not found in repository: {path}");

            // Try to get the content to validate it
            await gitService.GetFileContentAsync(repositoryUrl, branch, path);
            return DependencyValidationResult.Success();
        }
        catch (Exception ex)
        {
            return DependencyValidationResult.Failure($"Error validating Git dependency: {ex.Message}");
        }
    }
}