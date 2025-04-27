using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Executes native-style macros with command syntax similar to game macros.
/// </summary>
public class NativeMacroEngine : IMacroEngine
{
    private readonly ConcurrentDictionary<string, MacroExecutionState> _runningMacros = [];

    /// <inheritdoc/>
    public event EventHandler<MacroErrorEventArgs>? MacroError;

    /// <inheritdoc/>
    public event EventHandler<MacroControlEventArgs>? MacroControlRequested;

    /// <inheritdoc/>
    public event EventHandler<MacroStepCompletedEventArgs>? MacroStepCompleted;

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

        public void Dispose()
        {
            CancellationSource.Dispose();
            PauseEvent.Dispose();
        }
    }

    /// <inheritdoc/>
    public async Task StartMacro(IMacro macro, CancellationToken token, TriggerEventArgs? triggerArgs = null)
    {
        if (macro.Type != MacroType.Native)
            throw new ArgumentException("This engine only supports native macros", nameof(macro));

        var state = new MacroExecutionState(macro);
        if (!_runningMacros.TryAdd(macro.Id, state))
            throw new InvalidOperationException($"Macro {macro.Id} is already running");

        try
        {
            state.ExecutionTask = ExecuteMacroAsync(state, token);
            await state.ExecutionTask;
        }
        catch (Exception ex)
        {
            OnMacroError(macro.Id, "Macro execution failed", ex);
            throw;
        }
        finally
        {
            if (_runningMacros.TryRemove(macro.Id, out var removedState))
                removedState.Dispose();
        }
    }

    private async Task ExecuteMacroAsync(MacroExecutionState state, CancellationToken externalToken)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, state.CancellationSource.Token);
        var token = linkedCts.Token;

        try
        {
            var totalSteps = state.Macro.Commands.Count;
            var currentStep = 0;

            foreach (var command in state.Macro.Commands)
            {
                token.ThrowIfCancellationRequested();

                // Wait if paused
                state.PauseEvent.Wait(token);

                // Check for loop pause/stop
                if (state.PauseAtLoop)
                {
                    state.PauseAtLoop = false;
                    state.PauseEvent.Reset();
                    state.Macro.State = MacroState.Paused;
                }

                if (state.StopAtLoop)
                {
                    state.StopAtLoop = false;
                    state.CancellationSource.Cancel();
                    state.Macro.State = MacroState.Completed;
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
            // Normal cancellation, don't treat as error
            throw;
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

    /// <summary>
    /// Handles a control request for a macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to control.</param>
    /// <param name="controlType">The type of control operation.</param>
    public void HandleControlRequest(string macroId, MacroControlType controlType)
    {
        if (!_runningMacros.TryGetValue(macroId, out var state))
            return;

        switch (controlType)
        {
            case MacroControlType.Pause:
                state.PauseEvent.Reset();
                state.Macro.State = MacroState.Paused;
                break;
            case MacroControlType.Resume:
                state.PauseEvent.Set();
                state.Macro.State = MacroState.Running;
                break;
            case MacroControlType.Stop:
                state.CancellationSource.Cancel();
                state.Macro.State = MacroState.Completed;
                break;
        }
    }

    /// <summary>
    /// Sets a macro to pause at the next loop point.
    /// </summary>
    /// <param name="macroId">The ID of the macro.</param>
    public void PauseAtNextLoop(string macroId)
    {
        if (_runningMacros.TryGetValue(macroId, out var state))
        {
            state.PauseAtLoop = true;
            state.StopAtLoop = false;
        }
    }

    /// <summary>
    /// Sets a macro to stop at the next loop point.
    /// </summary>
    /// <param name="macroId">The ID of the macro.</param>
    public void StopAtNextLoop(string macroId)
    {
        if (_runningMacros.TryGetValue(macroId, out var state))
        {
            state.PauseAtLoop = false;
            state.StopAtLoop = true;
        }
    }

    public void Dispose()
    {
        foreach (var state in _runningMacros.Values)
        {
            state.CancellationSource.Cancel();
            state.Dispose();
        }
        _runningMacros.Clear();
    }
}
