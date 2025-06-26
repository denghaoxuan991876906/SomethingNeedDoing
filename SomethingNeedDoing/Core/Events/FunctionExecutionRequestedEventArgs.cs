namespace SomethingNeedDoing.Core.Events;

/// <summary>
/// Event arguments for function execution requests.
/// </summary>
/// <param name="macroId">The ID of the macro containing the function.</param>
/// <param name="functionName">The name of the function to execute.</param>
/// <param name="triggerArgs">The trigger event arguments that caused the function execution.</param>
public class FunctionExecutionRequestedEventArgs(string macroId, string functionName, TriggerEventArgs triggerArgs) : EventArgs
{
    /// <summary>
    /// Gets the ID of the macro containing the function.
    /// </summary>
    public string MacroId { get; } = macroId;

    /// <summary>
    /// Gets the name of the function to execute.
    /// </summary>
    public string FunctionName { get; } = functionName;

    /// <summary>
    /// Gets the trigger event arguments that caused the function execution.
    /// </summary>
    public TriggerEventArgs TriggerArgs { get; } = triggerArgs;
}
