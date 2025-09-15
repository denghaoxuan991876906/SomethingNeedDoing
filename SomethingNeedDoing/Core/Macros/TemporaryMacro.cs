using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Managers;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Represents a temporary macro that is not persisted in configuration.
/// This class is used for one-off macros that don't need to be saved to the config file.
/// It shares the same execution logic as ConfigMacro but differs in persistence.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TemporaryMacro"/> class.
/// </remarks>
/// <param name="content">The macro content.</param>
/// <param name="id">The optional ID for the macro.</param>
public class TemporaryMacro : MacroBase
{
    /// <inheritdoc/>
    public override string Id { get; }

    /// <inheritdoc/>
    public override string Name { get; set; } = "Temporary Macro";

    /// <inheritdoc/>
    public override MacroType Type { get; set; } = MacroType.Native;

    /// <inheritdoc/>
    public override MacroMetadata Metadata { get; set; } = new();

    public TemporaryMacro(string content, string? id = null)
    {
        Id = id ?? Guid.NewGuid().ToString();
        Content = content;
    }

    public TemporaryMacro(IMacro parent, string content, MacroHierarchyManager hierarchyManager, string? id = null)
    {
        Id = id ?? Guid.NewGuid().ToString();
        Content = content;
        hierarchyManager.RegisterTemporaryMacro(parent, this);
    }

    public async Task Run(EventHandler<MacroExecutionRequestedEventArgs> executionRequested, int loopCount = 0, TriggerEventArgs? triggerArgs = null)
    {
        var tcs = new TaskCompletionSource<bool>();
        void Handler(object? sender, MacroStateChangedEventArgs e)
        {
            if (e.MacroId == Id && (e.NewState == MacroState.Completed || e.NewState == MacroState.Error))
            {
                StateChanged -= Handler;
                tcs.TrySetResult(true);
            }
        }
        StateChanged += Handler;
        executionRequested?.Invoke(this, new MacroExecutionRequestedEventArgs(this, triggerArgs, loopCount));
        await tcs.Task;
    }

    /// <inheritdoc/>
    public override void Delete()
    {
        // Temporary macros aren't stored in the configuration, so no action is needed
        // This primarily just informs any references that the macro is no longer valid.
        State = MacroState.Completed;
    }
}
