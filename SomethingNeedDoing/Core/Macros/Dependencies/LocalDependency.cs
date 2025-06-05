using SomethingNeedDoing.Core.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Represents a local dependency (file or macro).
/// </summary>
/// <param name="filePath">The path to the file.</param>
/// <param name="name">The name of the dependency.</param>
public class LocalDependency(string filePath, string name) : IMacroDependency
{
    /// <inheritdoc/>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public string Name { get; } = name;

    /// <inheritdoc/>
    public DependencyType Type => DependencyType.Local;

    /// <inheritdoc/>
    public string Source => filePath;

    /// <inheritdoc/>
    public async Task<string> GetContentAsync() => await File.ReadAllTextAsync(filePath);

    /// <inheritdoc/>
    public Task<bool> IsAvailableAsync() => Task.FromResult(File.Exists(filePath));

    /// <inheritdoc/>
    public async Task<DependencyValidationResult> ValidateAsync()
    {
        if (!File.Exists(filePath))
            return DependencyValidationResult.Failure($"File not found: {filePath}");

        try
        {
            // Try to read the file to validate it
            await File.ReadAllTextAsync(filePath);
            return DependencyValidationResult.Success();
        }
        catch (Exception ex)
        {
            return DependencyValidationResult.Failure($"Error reading file: {ex.Message}");
        }
    }
}