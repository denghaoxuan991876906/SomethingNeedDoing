using System.Threading.Tasks;

namespace SomethingNeedDoing.Core.Interfaces;

/// <summary>
/// Interface for managing macro dependencies.
/// </summary>
public interface IDependencyManager
{
    /// <summary>
    /// Gets a dependency by its ID.
    /// </summary>
    /// <param name="id">The dependency ID.</param>
    /// <returns>The dependency, or null if not found.</returns>
    Task<IMacroDependency?> GetDependencyAsync(string id);

    /// <summary>
    /// Adds a new dependency.
    /// </summary>
    /// <param name="source">The source of the dependency.</param>
    /// <returns>The created dependency.</returns>
    Task<IMacroDependency> AddDependencyAsync(string source);

    /// <summary>
    /// Removes a dependency.
    /// </summary>
    /// <param name="id">The dependency ID.</param>
    Task RemoveDependencyAsync(string id);

    /// <summary>
    /// Updates a dependency.
    /// </summary>
    /// <param name="id">The dependency ID.</param>
    /// <returns>True if the dependency was updated, false otherwise.</returns>
    Task<bool> UpdateDependencyAsync(string id);

    /// <summary>
    /// Validates all dependencies.
    /// </summary>
    /// <returns>A dictionary of dependency IDs to validation results.</returns>
    Task<Dictionary<string, DependencyValidationResult>> ValidateAllAsync();

    /// <summary>
    /// Resolves all dependencies for a macro.
    /// </summary>
    /// <param name="macroId">The macro ID.</param>
    /// <returns>A list of resolved dependencies.</returns>
    Task<List<IMacroDependency>> ResolveDependenciesAsync(string macroId);
}