using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.Core.Interfaces;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;
/// <summary>
/// Provides context for macro execution.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MacroContext"/> class.
/// </remarks>
public class MacroContext(IMacro macro)
{
    /// <summary>
    /// Gets the macro being executed.
    /// </summary>
    public IMacro Macro { get; } = macro;

    /// <summary>
    /// Gets the current step in the macro.
    /// </summary>
    public int CurrentStep { get; private set; } = 0;

    /// <summary>
    /// Event raised when a macro execution is requested.
    /// </summary>
    public event EventHandler<MacroExecutionRequestedEventArgs>? MacroExecutionRequested;

    /// <summary>
    /// Raises the MacroExecutionRequested event.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    public void OnMacroExecutionRequested(object sender, MacroExecutionRequestedEventArgs e)
        => MacroExecutionRequested?.Invoke(sender, e);

    /// <summary>
    /// Runs an action on the framework thread.
    /// </summary>
    public Task RunOnFramework(Action action)
    {
        if (Svc.Framework.IsInFrameworkUpdateThread)
        {
            action();
            return Task.CompletedTask;
        }
        return Task.Run(() => Svc.Framework.RunOnFrameworkThread(action));
    }

    /// <summary>
    /// Waits for a condition to be met with timeout.
    /// </summary>
    public async Task WaitForCondition(Func<bool> condition, int timeout, int interval = 250)
    {
        var elapsed = 0;
        while (!condition() && elapsed < timeout)
        {
            await Task.Delay(interval);
            elapsed += interval;
        }
        if (elapsed >= timeout)
            throw new MacroTimeoutException("Condition wait timed out");
    }

    /// <summary>
    /// Moves to the next step in the macro.
    /// </summary>
    public void NextStep() => CurrentStep++;

    /// <summary>
    /// Loops back to the beginning of the macro.
    /// </summary>
    public void Loop() => CurrentStep = -1;
}
