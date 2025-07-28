namespace SomethingNeedDoing.Core.Events;

/// <summary>
/// Event arguments for loop control requests.
/// </summary>
/// <param name="macroId">The ID of the macro requesting loop control.</param>
/// <param name="controlType">The type of loop control being requested.</param>
public class LoopControlEventArgs(string macroId, LoopControlType controlType) : EventArgs
{
    /// <summary>
    /// Gets the ID of the macro requesting loop control.
    /// </summary>
    public string MacroId { get; } = macroId;

    /// <summary>
    /// Gets the type of loop control being requested.
    /// </summary>
    public LoopControlType ControlType { get; } = controlType;
}

/// <summary>
/// Types of loop control that can be requested.
/// </summary>
public enum LoopControlType
{
    /// <summary>
    /// Request to pause at the next loop.
    /// </summary>
    Pause,

    /// <summary>
    /// Request to stop at the next loop.
    /// </summary>
    Stop
}
