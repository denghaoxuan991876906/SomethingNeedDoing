using SomethingNeedDoing.Core.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Represents a local dependency (file or macro).
/// </summary>
public class LocalDependency : IMacroDependency
{
    /// <inheritdoc/>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public DependencyType Type => DependencyType.Local;

    /// <inheritdoc/>
    public string Source { get; set; } = string.Empty;

    /// <inheritdoc/>
    public async Task<string> GetContentAsync() => await File.ReadAllTextAsync(Source);

    /// <inheritdoc/>
    public Task<bool> IsAvailableAsync() => Task.FromResult(File.Exists(Source));

    /// <inheritdoc/>
    public async Task<DependencyValidationResult> ValidateAsync()
    {
        if (!File.Exists(Source))
            return DependencyValidationResult.Failure($"File not found: {Source}");

        try
        {
            // Try to read the file to validate it
            await File.ReadAllTextAsync(Source);
            return DependencyValidationResult.Success();
        }
        catch (Exception ex)
        {
            return DependencyValidationResult.Failure($"Error reading file: {ex.Message}");
        }
    }
}