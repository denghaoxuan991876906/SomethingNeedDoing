using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Executes native-style macros with command syntax similar to game macros.
/// </summary>
public class NativeMacroEngine : IMacroEngine, IMacroScheduler
{
    private readonly ConcurrentDictionary<string, MacroExecutionState> _runningMacros = [];
    private bool _isDisposed;

    /// <inheritdoc/>
    public event EventHandler<MacroStateChangedEventArgs>? MacroStateChanged;

    /// <inheritdoc/>
    public event EventHandler<MacroErrorEventArgs>? MacroError;

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
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            externalToken,
            state.CancellationSource.Token);
        var token = linkedCts.Token;

        var context = new MacroContext(state.Macro, this);
        state.Macro.State = MacroState.Running;

        try
        {
            foreach (var command in state.Macro.Commands)
            {
                // Check for cancellation
                token.ThrowIfCancellationRequested();

                // Wait if paused
                state.PauseEvent.Wait(token);

                // Execute the command
                if (command.RequiresFrameworkThread)
                {
                    await Svc.Framework.RunOnTick(() =>
                        command.Execute(context, token));
                }
                else
                {
                    await command.Execute(context, token);
                }

                context.NextStep();
            }

            state.Macro.State = MacroState.Completed;
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, don't treat as error
            throw;
        }
        catch (Exception ex)
        {
            state.Macro.State = MacroState.Error;
            OnMacroError(state.Macro.Id, "Error executing macro command", ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task StartMacro(IMacro macro) => StartMacro(macro, CancellationToken.None);

    /// <inheritdoc/>
    public Task PauseMacro(string macroId)
    {
        if (_runningMacros.TryGetValue(macroId, out var state))
        {
            state.PauseEvent.Reset();
            state.Macro.State = MacroState.Paused;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ResumeMacro(string macroId)
    {
        if (_runningMacros.TryGetValue(macroId, out var state))
        {
            state.PauseEvent.Set();
            state.Macro.State = MacroState.Running;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopMacro(string macroId)
    {
        if (_runningMacros.TryGetValue(macroId, out var state))
        {
            state.CancellationSource.Cancel();
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void CheckLoopPause(string macroId)
    {
        if (_runningMacros.TryGetValue(macroId, out var state) && state.PauseAtLoop)
        {
            state.PauseAtLoop = false;
            state.PauseEvent.Reset();
            state.Macro.State = MacroState.Paused;
        }
    }

    /// <inheritdoc/>
    public void CheckLoopStop(string macroId)
    {
        if (_runningMacros.TryGetValue(macroId, out var state) && state.StopAtLoop)
        {
            state.StopAtLoop = false;
            state.CancellationSource.Cancel();
        }
    }

    protected virtual void OnMacroStateChanged(string macroId, MacroState newState, MacroState oldState)
    {
        MacroStateChanged?.Invoke(this, new MacroStateChangedEventArgs(macroId, newState, oldState));
    }

    protected virtual void OnMacroError(string macroId, string message, Exception? ex = null)
    {
        MacroError?.Invoke(this, new MacroErrorEventArgs(macroId, message, ex));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_isDisposed) return;

        foreach (var state in _runningMacros.Values)
        {
            state.CancellationSource.Cancel();
            state.Dispose();
        }
        _runningMacros.Clear();

        _isDisposed = true;
    }
}
