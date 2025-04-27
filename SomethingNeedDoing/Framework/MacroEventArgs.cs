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

/// <summary>
/// Event arguments for macro control requests.
/// </summary>
public class MacroControlEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MacroControlEventArgs"/> class.
    /// </summary>
    /// <param name="macroId">The ID of the macro to control.</param>
    /// <param name="controlType">The type of control operation.</param>
    public MacroControlEventArgs(string macroId, MacroControlType controlType)
    {
        MacroId = macroId;
        ControlType = controlType;
    }

    /// <summary>
    /// Gets the ID of the macro to control.
    /// </summary>
    public string MacroId { get; }

    /// <summary>
    /// Gets the type of control operation.
    /// </summary>
    public MacroControlType ControlType { get; }
}

/// <summary>
/// Event arguments for macro loop checks.
/// </summary>
public class MacroLoopCheckEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MacroLoopCheckEventArgs"/> class.
    /// </summary>
    /// <param name="macroId">The ID of the macro to check.</param>
    public MacroLoopCheckEventArgs(string macroId)
    {
        MacroId = macroId;
    }

    /// <summary>
    /// Gets the ID of the macro to check.
    /// </summary>
    public string MacroId { get; }

    /// <summary>
    /// Gets or sets whether the macro should pause at the next loop point.
    /// </summary>
    public bool ShouldPause { get; set; }

    /// <summary>
    /// Gets or sets whether the macro should stop at the next loop point.
    /// </summary>
    public bool ShouldStop { get; set; }
}

/// <summary>
/// Event arguments for macro step completion.
/// </summary>
public class MacroStepCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MacroStepCompletedEventArgs"/> class.
    /// </summary>
    /// <param name="macroId">The ID of the macro.</param>
    /// <param name="stepIndex">The index of the completed step.</param>
    /// <param name="totalSteps">The total number of steps in the macro.</param>
    public MacroStepCompletedEventArgs(string macroId, int stepIndex, int totalSteps)
    {
        MacroId = macroId;
        StepIndex = stepIndex;
        TotalSteps = totalSteps;
    }

    /// <summary>
    /// Gets the ID of the macro.
    /// </summary>
    public string MacroId { get; }

    /// <summary>
    /// Gets the index of the completed step.
    /// </summary>
    public int StepIndex { get; }

    /// <summary>
    /// Gets the total number of steps in the macro.
    /// </summary>
    public int TotalSteps { get; }
}
