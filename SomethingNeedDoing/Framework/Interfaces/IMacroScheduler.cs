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
    /// Starts execution of a macro.
    /// </summary>
    /// <param name="macro">The macro to execute.</param>
    Task StartMacro(IMacro macro);

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
}
