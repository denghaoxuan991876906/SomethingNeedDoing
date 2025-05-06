using SomethingNeedDoing.Framework.Interfaces;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;

/// <summary>
/// Represents a dependency on a Git repository.
/// </summary>
/// <param name="gitService">The Git service.</param>
/// <param name="repositoryUrl">The URL of the Git repository.</param>
/// <param name="branch">The branch to use.</param>
/// <param name="path">The path to the file within the repository.</param>
/// <param name="name">The name of the dependency.</param>
public class GitRepositoryDependency(IGitService gitService, string repositoryUrl, string branch, string path, string name) : IMacroDependency
{
    /// <inheritdoc/>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public string Name { get; } = name;

    /// <inheritdoc/>
    public DependencyType Type { get; } = DependencyType.GitRepository;

    /// <inheritdoc/>
    public string Version { get; } = $"{repositoryUrl}#{branch}";

    /// <inheritdoc/>
    public async Task<string> GetContentAsync() => await gitService.GetFileContentAsync(repositoryUrl, branch, path);

    /// <inheritdoc/>
    public async Task<bool> IsAvailableAsync() => await gitService.FileExistsAsync(repositoryUrl, branch, path);
}
