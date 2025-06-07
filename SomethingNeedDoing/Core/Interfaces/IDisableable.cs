using System.Threading.Tasks;

namespace SomethingNeedDoing.Core.Interfaces;

/// <summary>
/// Interface for plugins that can be disabled and enabled.
/// </summary>
public interface IDisableable
{
    /// <summary>
    /// Gets the internal name of the plugin.
    /// </summary>
    string InternalName { get; }

    /// <summary>
    /// Enables the plugin.
    /// </summary>
    /// <returns>True if the plugin was successfully enabled.</returns>
    Task<bool> EnableAsync();

    /// <summary>
    /// Disables the plugin.
    /// </summary>
    /// <returns>True if the plugin was successfully disabled.</returns>
    Task<bool> DisableAsync();
}
