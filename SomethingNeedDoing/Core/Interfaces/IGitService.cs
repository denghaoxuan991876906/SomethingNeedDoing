using System.Threading.Tasks;

namespace SomethingNeedDoing.Core.Interfaces;

/// <summary>
/// Interface for Git operations.
/// </summary>
public interface IGitService
{
    /// <summary>
    /// Gets the content of a file from a Git repository.
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <param name="branch">The branch name.</param>
    /// <param name="path">The path within the repository.</param>
    /// <returns>The file content.</returns>
    Task<string> GetFileContentAsync(string repositoryUrl, string branch, string path);

    /// <summary>
    /// Checks if a file exists in a Git repository.
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <param name="branch">The branch name.</param>
    /// <param name="path">The path within the repository.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    Task<bool> FileExistsAsync(string repositoryUrl, string branch, string path);
}
