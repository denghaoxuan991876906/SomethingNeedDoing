using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;

/// <summary>
/// Interface for the macro engine that executes macros.
/// </summary>
public interface IMacroEngine : IDisposable
{
    /// <summary>
    /// Starts executing a macro.
    /// </summary>
    /// <param name="macro">The macro to execute.</param>
    /// <param name="token">A token to cancel execution.</param>
    Task StartMacro(IMacro macro, CancellationToken token, TriggerEventArgs? triggerEventArgs = null);

    /// <summary>
    /// Pauses execution of a macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to pause.</param>
    Task PauseMacro(string macroId);

    /// <summary>
    /// Resumes execution of a paused macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to resume.</param>
    Task ResumeMacro(string macroId);

    /// <summary>
    /// Stops execution of a macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to stop.</param>
    Task StopMacro(string macroId);

    /// <summary>
    /// Event raised when a macro's state changes.
    /// </summary>
    event EventHandler<MacroStateChangedEventArgs> MacroStateChanged;

    /// <summary>
    /// Event raised when a macro encounters an error.
    /// </summary>
    event EventHandler<MacroErrorEventArgs> MacroError;
}
