using SomethingNeedDoing.Core.Interfaces;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Represents a local macro dependency (references another config macro).
/// </summary>
public class LocalMacroDependency : IMacroDependency
{
    /// <inheritdoc/>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public DependencyType Type => DependencyType.Local;

    /// <inheritdoc/>
    public string Source { get; set; } = string.Empty; // This will store the macro ID

    /// <inheritdoc/>
    public async Task<string> GetContentAsync()
    {
        var macro = C.GetMacro(Source);
        return macro == null ? throw new InvalidOperationException($"Macro with ID {Source} not found") : macro.Content;
    }

    /// <inheritdoc/>
    public Task<bool> IsAvailableAsync()
    {
        var macro = C.GetMacro(Source);
        return Task.FromResult(macro != null);
    }

    /// <inheritdoc/>
    public async Task<DependencyValidationResult> ValidateAsync()
    {
        var macro = C.GetMacro(Source);
        if (macro == null)
            return DependencyValidationResult.Failure($"Macro with ID {Source} not found");

        try
        {
            await GetContentAsync();
            return DependencyValidationResult.Success();
        }
        catch (Exception ex)
        {
            return DependencyValidationResult.Failure($"Error reading macro content: {ex.Message}");
        }
    }
}
