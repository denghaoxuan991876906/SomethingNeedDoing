using SomethingNeedDoing.Core.Interfaces;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;
/// <summary>
/// Represents a Git macro dependency.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GitMacroDependency"/> class.
/// </remarks>
/// <param name="id">The unique identifier.</param>
/// <param name="name">The name.</param>
/// <param name="version">The version.</param>
/// <param name="repositoryUrl">The repository URL.</param>
/// <param name="branch">The branch name.</param>
/// <param name="filePath">The file path.</param>
/// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
public class GitMacroDependency() : IMacroDependency
{
    /// <summary>
    /// Gets the unique identifier of the dependency.
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the name of the dependency.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the type of the dependency.
    /// </summary>
    public DependencyType Type => DependencyType.GitMacro;

    /// <summary>
    /// Gets the version of the dependency.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets the repository URL.
    /// </summary>
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets the branch name.
    /// </summary>
    public string Branch { get; set; } = string.Empty;

    /// <summary>
    /// Gets the file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <inheritdoc/>
    public Task<string> GetContentAsync() => throw new NotImplementedException("Git macro dependencies are handled by the GitMacroManager.");

    /// <inheritdoc/>
    public Task<bool> IsAvailableAsync() => throw new NotImplementedException("Git macro dependencies are handled by the GitMacroManager.");
}
