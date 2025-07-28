using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.Core.Events;

/// <summary>
/// Event arguments for when a macro execution is requested.
/// </summary>
/// <remarks>
/// For engine to scheduler communication
/// </remarks>
/// <param name="macro">The macro to execute.</param>
/// <param name="triggerArgs">Optional trigger event arguments.</param>
/// <param name="loopCount">The loop count for the macro execution.</param>
public class MacroExecutionRequestedEventArgs(IMacro macro, TriggerEventArgs? triggerArgs = null, int loopCount = 0) : EventArgs
{
    /// <summary>
    /// Gets the macro to execute.
    /// </summary>
    public IMacro Macro { get; } = macro;

    /// <summary>
    /// Gets the trigger event arguments, if any.
    /// </summary>
    public TriggerEventArgs? TriggerArgs { get; } = triggerArgs;

    /// <summary>
    /// Gets the loop count for the macro execution.
    /// </summary>
    public int LoopCount { get; } = loopCount;
}
