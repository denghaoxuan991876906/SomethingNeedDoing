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

/// <summary>
/// Base class for all macro implementations, providing common state management logic.
/// </summary>
public abstract class MacroBase : IMacro
{
    /// <inheritdoc/>
    public abstract string Id { get; }

    /// <inheritdoc/>
    public abstract string Name { get; set; }

    /// <inheritdoc/>
    public abstract MacroType Type { get; set; }

    /// <inheritdoc/>
    public abstract string Content { get; set; }

    /// <inheritdoc/>
    public MacroState State
    {
        get; set
        {
            if (field != value)
                StateChanged?.Invoke(this, new MacroStateChangedEventArgs(Id, value, field));
        }
    } = MacroState.Ready;

    /// <inheritdoc/>
    public event EventHandler<MacroStateChangedEventArgs>? StateChanged;

    /// <inheritdoc/>
    public abstract MacroMetadata Metadata { get; set; }

    /// <inheritdoc/>
    public abstract IReadOnlyList<IMacroCommand> Commands { get; set; }

    /// <inheritdoc/>
    public virtual async Task Start(TriggerEventArgs? args = null)
    {
        if (State is not MacroState.Ready and not MacroState.Paused)
            throw new InvalidOperationException($"Cannot start macro in state {State}");

        State = MacroState.Running;
        try
        {
            await ExecuteMacro(args);
        }
        catch (Exception ex)
        {
            State = MacroState.Error;
            throw new MacroException($"Failed to execute macro: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public virtual async Task Stop()
    {
        if (State is not MacroState.Running and not MacroState.Paused)
            throw new InvalidOperationException($"Cannot stop macro in state {State}");

        State = MacroState.Ready;
        await StopMacro();
    }

    /// <inheritdoc/>
    public virtual async Task Pause()
    {
        if (State != MacroState.Running)
            throw new InvalidOperationException($"Cannot pause macro in state {State}");

        State = MacroState.Paused;
        await PauseMacro();
    }

    /// <inheritdoc/>
    public virtual async Task Resume()
    {
        if (State != MacroState.Paused)
            throw new InvalidOperationException($"Cannot resume macro in state {State}");

        State = MacroState.Running;
        await ResumeMacro();
    }

    /// <inheritdoc/>
    public virtual async Task HandleTriggerEvent(TriggerEventArgs args)
    {
        if (State != MacroState.Ready)
            throw new InvalidOperationException($"Cannot handle trigger event in state {State}");

        await Start(args);
    }

    /// <summary>
    /// Executes the macro with the given trigger event arguments.
    /// </summary>
    /// <param name="args">Optional trigger event arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task ExecuteMacro(TriggerEventArgs? args)
    {
        // Default implementation for ExecuteMacro
        // This will be overridden by specific macro implementations
        await Service.MacroScheduler.StartMacro(this, args);
    }

    /// <summary>
    /// Stops the macro execution.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task StopMacro()
    {
        // Default implementation for StopMacro
        // This will be overridden by specific macro implementations
        await Service.MacroScheduler.StopMacro(Id);
    }

    /// <summary>
    /// Pauses the macro execution.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task PauseMacro()
    {
        // Default implementation for PauseMacro
        // This will be overridden by specific macro implementations
        await Service.MacroScheduler.PauseMacro(Id);
    }

    /// <summary>
    /// Resumes the macro execution.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task ResumeMacro()
    {
        // Default implementation for ResumeMacro
        // This will be overridden by specific macro implementations
        await Service.MacroScheduler.ResumeMacro(Id);
    }
}
