using SomethingNeedDoing.Core.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework.Interfaces;

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
    /// Event raised when a macro encounters an error.
    /// </summary>
    event EventHandler<MacroErrorEventArgs> MacroError;

    /// <summary>
    /// Event raised when a macro requests control of another macro.
    /// </summary>
    event EventHandler<MacroControlEventArgs> MacroControlRequested;

    /// <summary>
    /// Event raised when a macro step is completed.
    /// </summary>
    event EventHandler<MacroStepCompletedEventArgs> MacroStepCompleted;

    /// <summary>
    /// Gets or sets the macro scheduler used by this engine.
    /// </summary>
    IMacroScheduler? Scheduler { get; set; } // TODO: only doing this because of macro command dependencies. Fix in the future

    /// <summary>
    /// Gets a temporary macro by its ID.
    /// </summary>
    /// <param name="macroId">The ID of the temporary macro.</param>
    /// <returns>The temporary macro, or null if not found.</returns>
    IMacro? GetTemporaryMacro(string macroId);
}
