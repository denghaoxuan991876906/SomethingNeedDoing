using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Executes native-style macros with command syntax similar to game macros.
/// </summary>
public class NativeMacroEngine(IMacroScheduler scheduler) : IMacroEngine
{
    private readonly ConcurrentDictionary<string, MacroExecutionState> _runningMacros = [];
    private readonly IMacroScheduler _scheduler = scheduler;

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
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, state.CancellationSource.Token);
        var token = linkedCts.Token;

        var context = new MacroContext(state.Macro, _scheduler);

        try
        {
            foreach (var command in state.Macro.Commands)
            {
                token.ThrowIfCancellationRequested();

                // Wait if paused
                state.PauseEvent.Wait(token);

                if (command.RequiresFrameworkThread)
                    await Svc.Framework.RunOnTick(() => command.Execute(context, token), cancellationToken: token);
                else
                    await command.Execute(context, token);

                context.NextStep();
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

    public void Dispose()
    {
        // Nothing to dispose in this implementation
    }
}
