using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework.Interfaces;

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
    /// Gets the version or identifier of the dependency.
    /// </summary>
    string Version { get; }

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
}

/// <summary>
/// Enum representing the different types of dependencies.
/// </summary>
public enum DependencyType
{
    /// <summary>
    /// A local file dependency.
    /// </summary>
    LocalFile,

    /// <summary>
    /// A Git repository dependency.
    /// </summary>
    GitRepository,

    /// <summary>
    /// A ConfigMacro dependency.
    /// </summary>
    ConfigMacro,

    /// <summary>
    /// A GitMacro dependency.
    /// </summary>
    GitMacro,

    /// <summary>
    /// An HTTP dependency.
    /// </summary>
    Http,
}
