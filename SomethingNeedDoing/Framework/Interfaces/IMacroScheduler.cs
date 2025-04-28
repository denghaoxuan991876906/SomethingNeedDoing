using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Interface for macro scheduling and control.
/// This is the central coordinator for all macro operations.
/// Individual macro engines should not implement control operations directly.
/// </summary>
public interface IMacroScheduler
{
    /// <summary>
    /// Event raised when a macro's state changes.
    /// </summary>
    event EventHandler<MacroStateChangedEventArgs>? MacroStateChanged;

    /// <summary>
    /// Starts execution of a macro.
    /// </summary>
    /// <param name="macro">The macro to execute.</param>
    Task StartMacro(IMacro macro);

    /// <summary>
    /// Starts execution of a macro with trigger event arguments.
    /// </summary>
    /// <param name="macro">The macro to execute.</param>
    /// <param name="triggerArgs">Optional trigger event arguments.</param>
    Task StartMacro(IMacro macro, TriggerEventArgs? triggerArgs);

    /// <summary>
    /// Pauses execution of a macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to pause.</param>
    void PauseMacro(string macroId);

    /// <summary>
    /// Resumes execution of a paused macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to resume.</param>
    void ResumeMacro(string macroId);

    /// <summary>
    /// Stops execution of a macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to stop.</param>
    void StopMacro(string macroId);

    /// <summary>
    /// Checks if the macro should pause at the current loop point.
    /// </summary>
    /// <param name="macroId">The ID of the macro to check.</param>
    void CheckLoopPause(string macroId);

    /// <summary>
    /// Checks if the macro should stop at the current loop point.
    /// </summary>
    /// <param name="macroId">The ID of the macro to check.</param>
    void CheckLoopStop(string macroId);

    /// <summary>
    /// Gets all currently running macros.
    /// </summary>
    /// <returns>An enumerable of currently running macros.</returns>
    IEnumerable<IMacro> GetMacros();

    /// <summary>
    /// Gets the current state of a macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to get the state for.</param>
    /// <returns>The current state of the macro.</returns>
    MacroState GetMacroState(string macroId);

    /// <summary>
    /// Subscribes a macro to a trigger event.
    /// </summary>
    /// <param name="macro">The macro to subscribe.</param>
    /// <param name="triggerEvent">The trigger event to subscribe to.</param>
    void SubscribeToTriggerEvent(IMacro macro, TriggerEvent triggerEvent);

    /// <summary>
    /// Unsubscribes a macro from a trigger event.
    /// </summary>
    /// <param name="macro">The macro to unsubscribe.</param>
    /// <param name="triggerEvent">The trigger event to unsubscribe from.</param>
    void UnsubscribeFromTriggerEvent(IMacro macro, TriggerEvent triggerEvent);
}
