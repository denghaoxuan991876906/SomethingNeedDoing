using System.Threading.Tasks;

namespace SomethingNeedDoing.Core.Interfaces;

/// <summary>
/// Interface for macro dependencies that can be implemented by different dependency types.
/// </summary>
public interface IMacroDependency
{
    /// <summary>
    /// Gets the unique identifier for this dependency.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the name of the dependency.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the type of dependency.
    /// </summary>
    DependencyType Type { get; }

    /// <summary>
    /// Gets the source of the dependency (URL, file path, etc.).
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Gets the content of the dependency.
    /// </summary>
    /// <returns>The content as a string.</returns>
    Task<string> GetContentAsync();

    /// <summary>
    /// Checks if the dependency is available.
    /// </summary>
    /// <returns>True if the dependency is available, false otherwise.</returns>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// Validates the dependency.
    /// </summary>
    /// <returns>A validation result indicating if the dependency is valid and any error messages.</returns>
    Task<DependencyValidationResult> ValidateAsync();
}

/// <summary>
/// Enum representing the different types of dependencies.
/// </summary>
public enum DependencyType
{
    /// <summary>
    /// A local dependency (file or macro).
    /// </summary>
    Local,

    /// <summary>
    /// A remote dependency (Git or HTTP).
    /// </summary>
    Remote
}

/// <summary>
/// Enum for local dependency types.
/// </summary>
public enum LocalDependencyType
{
    /// <summary>
    /// A local file dependency.
    /// </summary>
    File,

    /// <summary>
    /// A local macro dependency.
    /// </summary>
    Macro
}

/// <summary>
/// Represents the result of validating a dependency.
/// </summary>
public class DependencyValidationResult
{
    /// <summary>
    /// Gets whether the dependency is valid.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets any error messages associated with the validation.
    /// </summary>
    public string[] Errors { get; init; } = [];

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static DependencyValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with the specified errors.
    /// </summary>
    public static DependencyValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors
    };
}
