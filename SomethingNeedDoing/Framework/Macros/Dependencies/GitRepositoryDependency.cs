using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;

/// <summary>
/// Represents a dependency on a Git repository.
/// </summary>
public class GitRepositoryDependency : IMacroDependency
{
    private readonly IGitService _gitService;
    private readonly string _repositoryUrl;
    private readonly string _branch;
    private readonly string _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitRepositoryDependency"/> class.
    /// </summary>
    /// <param name="gitService">The Git service.</param>
    /// <param name="repositoryUrl">The URL of the Git repository.</param>
    /// <param name="branch">The branch to use.</param>
    /// <param name="path">The path to the file within the repository.</param>
    /// <param name="name">The name of the dependency.</param>
    public GitRepositoryDependency(
        IGitService gitService,
        string repositoryUrl,
        string branch,
        string path,
        string name)
    {
        Id = Guid.NewGuid().ToString();
        _gitService = gitService;
        _repositoryUrl = repositoryUrl;
        _branch = branch;
        _path = path;
        Name = name;
        Type = DependencyType.GitRepository;
        Version = $"{_repositoryUrl}#{_branch}";
    }

    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public DependencyType Type { get; }

    /// <inheritdoc/>
    public string Version { get; }

    /// <inheritdoc/>
    public async Task<string> GetContentAsync()
    {
        return await _gitService.GetFileContentAsync(_repositoryUrl, _branch, _path);
    }

    /// <inheritdoc/>
    public async Task<bool> IsAvailableAsync()
    {
        return await _gitService.FileExistsAsync(_repositoryUrl, _branch, _path);
    }
}
