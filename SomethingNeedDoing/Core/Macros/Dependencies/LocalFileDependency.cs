using SomethingNeedDoing.Core.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Represents a dependency on a local file.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="LocalFileDependency"/> class.
/// </remarks>
/// <param name="filePath">The path to the file.</param>
/// <param name="name">The name of the dependency.</param>
public class LocalFileDependency(string filePath, string name) : IMacroDependency
{

    /// <inheritdoc/>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public string Name { get; } = name;

    /// <inheritdoc/>
    public DependencyType Type { get; } = DependencyType.LocalFile;

    /// <inheritdoc/>
    public string Version { get; } = File.GetLastWriteTimeUtc(filePath).ToString("O");

    /// <summary>
    /// Gets the path to the file.
    /// </summary>
    private string FilePath { get; } = filePath;

    /// <inheritdoc/>
    public async Task<string> GetContentAsync() => await File.ReadAllTextAsync(FilePath);

    /// <inheritdoc/>
    public Task<bool> IsAvailableAsync() => Task.FromResult(File.Exists(FilePath));
}
