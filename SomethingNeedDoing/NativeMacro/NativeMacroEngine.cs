using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.Framework.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro;
/// <summary>
/// Executes native-style macros with command syntax similar to game macros.
/// </summary>
public class NativeMacroEngine(MacroParser parser) : IMacroEngine
{
    /// <inheritdoc/>
    public event EventHandler<MacroErrorEventArgs>? MacroError;

    /// <inheritdoc/>
    public event EventHandler<MacroControlEventArgs>? MacroControlRequested;

    /// <inheritdoc/>
    public event EventHandler<MacroStepCompletedEventArgs>? MacroStepCompleted;

    /// <inheritdoc/>
    public IMacroScheduler? Scheduler { get; set; }

    /// <summary>
    /// Represents the current execution state of a macro.
    /// </summary>
    private class MacroExecutionState(IMacro macro)
    {
        public IMacro Macro { get; } = macro;
        public CancellationTokenSource CancellationSource { get; } = new CancellationTokenSource();
        public Task? ExecutionTask { get; set; }
        public ManualResetEventSlim PauseEvent { get; } = new ManualResetEventSlim(true);
        public bool PauseAtLoop { get; set; } = false;
        public bool StopAtLoop { get; set; } = false;
        public List<IMacroCommand> Commands { get; set; } = [];

        public void Dispose()
        {
            CancellationSource.Dispose();
            PauseEvent.Dispose();
        }
    }

    /// <inheritdoc/>
    public async Task StartMacro(IMacro macro, CancellationToken token, TriggerEventArgs? triggerArgs = null)
    {
        if (Scheduler == null)
            throw new InvalidOperationException("Scheduler must be set before starting a macro");

        var state = new MacroExecutionState(macro) { Commands = parser.Parse(macro.Content, Scheduler) };

        try
        {
            state.ExecutionTask = ExecuteMacro(state, token);
            await state.ExecutionTask;
        }
        catch (Exception ex)
        {
            OnMacroError(macro.Id, "Macro execution failed", ex);
            throw;
        }
    }

    private async Task ExecuteMacro(MacroExecutionState state, CancellationToken externalToken)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, state.CancellationSource.Token);
        var token = linkedCts.Token;

        try
        {
            var totalSteps = state.Commands.Count;
            var currentStep = 0;

            foreach (var command in state.Commands)
            {
                token.ThrowIfCancellationRequested();

                // Wait if paused
                state.PauseEvent.Wait(token);

                // Check for loop pause/stop
                if (state.PauseAtLoop)
                {
                    state.PauseAtLoop = false;
                    state.PauseEvent.Reset();
                }

                if (state.StopAtLoop)
                {
                    state.StopAtLoop = false;
                    state.CancellationSource.Cancel();
                    return;
                }

                if (command.RequiresFrameworkThread)
                    await Svc.Framework.RunOnTick(() => command.Execute(new MacroContext(state.Macro), token), cancellationToken: token);
                else
                    await command.Execute(new MacroContext(state.Macro), token);

                currentStep++;
                MacroStepCompleted?.Invoke(this, new MacroStepCompletedEventArgs(state.Macro.Id, currentStep, totalSteps));
            }
        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception ex)
        {
            OnMacroError(state.Macro.Id, "Error executing macro command", ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task StartMacro(IMacro macro) => StartMacro(macro, CancellationToken.None);

    protected virtual void OnMacroError(string macroId, string message, Exception? ex = null)
        => MacroError?.Invoke(this, new MacroErrorEventArgs(macroId, message, ex));

    /// <inheritdoc/>
    public IMacro? GetTemporaryMacro(string macroId) => null; // Native engine doesn't create temporary macros

    public void Dispose() { }
}
