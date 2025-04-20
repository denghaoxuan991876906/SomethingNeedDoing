using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Represents a macro that can be executed by the macro engine.
/// </summary>
public interface IMacro
{
    /// <summary>
    /// Gets the unique identifier for this macro.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the display name of the macro.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the type of macro (Native or Lua).
    /// </summary>
    MacroType Type { get; }

    /// <summary>
    /// Gets the raw content/script of the macro.
    /// </summary>
    string Content { get; }

    /// <summary>
    /// Gets or sets the current state of the macro.
    /// </summary>
    MacroState State { get; set; }

    /// <summary>
    /// Gets the commands that make up this macro.
    /// </summary>
    IReadOnlyList<IMacroCommand> Commands { get; }

    /// <summary>
    /// Event raised when the macro's state changes.
    /// </summary>
    event EventHandler<MacroStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Gets the metadata for the macro.
    /// </summary>
    MacroMetadata Metadata { get; }

    /// <summary>
    /// Starts the macro execution.
    /// </summary>
    /// <param name="args">Optional trigger event arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Start(TriggerEventArgs? args = null);

    /// <summary>
    /// Stops the macro execution.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Stop();

    /// <summary>
    /// Pauses the macro execution.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Pause();

    /// <summary>
    /// Resumes the macro execution.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Resume();

    /// <summary>
    /// Handles a trigger event for the macro.
    /// </summary>
    /// <param name="args">The trigger event arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleTriggerEvent(TriggerEventArgs args);
}

public interface IMacroInstance : IDisposable
{
    public IMacro Macro { get; }
    public CancellationTokenSource CancellationSource { get; }
    public ManualResetEventSlim PauseEvent { get; }
    public Task? ExecutionTask { get; set; }
    public bool PauseAtLoop { get; set; }
    public bool StopAtLoop { get; set; }

    public new void Dispose()
    {
        CancellationSource.Dispose();
        PauseEvent.Dispose();
    }
}
