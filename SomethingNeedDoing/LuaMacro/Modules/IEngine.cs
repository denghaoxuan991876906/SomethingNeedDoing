using SomethingNeedDoing.Core.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.LuaMacro.Modules;

/// <summary>
/// Interface for engines that can execute content.
/// </summary>
public interface IEngine
{
    /// <summary>
    /// Gets the name of the engine.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Event raised when a macro execution is requested.
    /// </summary>
    event EventHandler<MacroExecutionRequestedEventArgs>? MacroExecutionRequested;

    /// <summary>
    /// Executes content asynchronously.
    /// </summary>
    /// <param name="content">The content to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the execution.</returns>
    Task ExecuteAsync(string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if this engine can execute the given content.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns>True if this engine can execute the content.</returns>
    bool CanExecute(string content);
}
