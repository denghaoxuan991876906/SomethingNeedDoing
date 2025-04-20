using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Represents a macro that is stored in the configuration.
/// </summary>
public class ConfigMacro : MacroBase
{
    /// <inheritdoc/>
    public override string Id { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public override string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override MacroType Type { get; set; }

    /// <inheritdoc/>
    public override string Content { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override MacroMetadata Metadata { get; set; } = new();

    /// <inheritdoc/>
    public override IReadOnlyList<IMacroCommand> Commands { get; set; } = [];

    /// <summary>
    /// Gets or sets the folder path of the macro.
    /// </summary>
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Updates the last modified timestamp.
    /// </summary>
    public void UpdateLastModified() => Metadata.LastModified = DateTime.Now;

    /// <inheritdoc/>
    protected override async Task ExecuteMacro(TriggerEventArgs? args)
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
    protected override async Task StopMacro()
    {
        if (State is not MacroState.Running and not MacroState.Paused)
            throw new InvalidOperationException($"Cannot stop macro in state {State}");

        State = MacroState.Ready;
        await StopMacro();
    }

    /// <inheritdoc/>
    protected override async Task PauseMacro()
    {
        if (State != MacroState.Running)
            throw new InvalidOperationException($"Cannot pause macro in state {State}");

        State = MacroState.Paused;
        await PauseMacro();
    }

    /// <inheritdoc/>
    protected override async Task ResumeMacro()
    {
        if (State != MacroState.Paused)
            throw new InvalidOperationException($"Cannot resume macro in state {State}");

        State = MacroState.Running;
        await ResumeMacro();
    }

    /// <summary>
    /// Handles a trigger event for the macro.
    /// </summary>
    /// <param name="args">The trigger event arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleTriggerEvent(TriggerEventArgs args)
    {
        if (State != MacroState.Ready)
            throw new InvalidOperationException($"Cannot handle trigger event in state {State}");

        await ExecuteMacro(args);
    }
}
