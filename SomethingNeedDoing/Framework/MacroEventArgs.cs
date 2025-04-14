namespace SomethingNeedDoing.Framework;
/// <summary>
/// Event arguments for macro state changes.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MacroStateChangedEventArgs"/> class.
/// </remarks>
public class MacroStateChangedEventArgs(string macroId, MacroState newState, MacroState previousState) : EventArgs
{
    /// <summary>
    /// Gets the ID of the macro whose state changed.
    /// </summary>
    public string MacroId { get; } = macroId;

    /// <summary>
    /// Gets the new state of the macro.
    /// </summary>
    public MacroState NewState { get; } = newState;

    /// <summary>
    /// Gets the previous state of the macro.
    /// </summary>
    public MacroState PreviousState { get; } = previousState;
}

/// <summary>
/// Event arguments for macro errors.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MacroErrorEventArgs"/> class.
/// </remarks>
public class MacroErrorEventArgs(string macroId, string errorMessage, Exception? exception = null) : EventArgs
{
    /// <summary>
    /// Gets the ID of the macro that encountered an error.
    /// </summary>
    public string MacroId { get; } = macroId;

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string ErrorMessage { get; } = errorMessage;

    /// <summary>
    /// Gets the exception that caused the error, if any.
    /// </summary>
    public Exception? Exception { get; } = exception;
}
