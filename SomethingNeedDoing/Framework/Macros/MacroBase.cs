using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;

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
    protected abstract Task ExecuteMacro(TriggerEventArgs? args);

    /// <summary>
    /// Stops the macro execution.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task StopMacro();

    /// <summary>
    /// Pauses the macro execution.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task PauseMacro();

    /// <summary>
    /// Resumes the macro execution.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task ResumeMacro();
}
