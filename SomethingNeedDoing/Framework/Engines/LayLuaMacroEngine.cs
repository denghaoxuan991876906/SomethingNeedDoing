using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin.Services;
using Laylua;
using Laylua.Marshaling;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;

/// <summary>
/// Executes Lua script macros using NLua.
/// </summary>
public class LayLuaMacroEngine() : IMacroEngine
{
    /// <inheritdoc/>
    public event EventHandler<MacroErrorEventArgs>? MacroError;

    /// <inheritdoc/>
    public event EventHandler<MacroControlEventArgs>? MacroControlRequested;

    /// <inheritdoc/>
    public event EventHandler<MacroStepCompletedEventArgs>? MacroStepCompleted;

    /// <inheritdoc/>
    public IMacroScheduler? Scheduler { get; set; }

    private readonly Dictionary<string, TemporaryMacro> _temporaryMacros = [];

    /// <inheritdoc/>
    public IMacro? GetTemporaryMacro(string macroId) => _temporaryMacros.TryGetValue(macroId, out var macro) ? macro : null;

    /// <summary>
    /// Represents the current state of a macro execution.
    /// </summary>
    private class MacroInstance(IMacro macro) : IDisposable
    {
        public IMacro Macro { get; } = macro;
        public LuaFunction? LuaGenerator { get; set; }
        public CancellationTokenSource CancellationSource { get; } = new();
        public ManualResetEventSlim PauseEvent { get; } = new(true);
        public Task? ExecutionTask { get; set; }
        public EventHandler<MacroStateChangedEventArgs>? StateChangedHandler { get; set; }

        public void Dispose()
        {
            CancellationSource.Dispose();
            PauseEvent.Dispose();
        }
    }

    private class LimitedServiceClass()
    {
        public IClientState ClientState => Svc.ClientState;
        public ICondition Condition => Svc.Condition;
        public IObjectTable Objects => Svc.Objects;
        public IPartyList Party => Svc.Party;
        public ITargetManager Targets => Svc.Targets;
        public void Log(object message) => Svc.Log.Info(message.ToString() ?? string.Empty);
    }

    /// <inheritdoc/>
    public async Task StartMacro(IMacro macro, CancellationToken token, TriggerEventArgs? triggerArgs = null)
    {
        if (macro.Type != MacroType.Lua)
            throw new ArgumentException("This engine only supports Lua macros", nameof(macro));

        var state = new MacroInstance(macro);

        try
        {
            state.ExecutionTask = ExecuteMacro(state, token, triggerArgs);
            await state.ExecutionTask;
        }
        catch (Exception ex)
        {
            OnMacroError(macro.Id, "Macro execution failed", ex);
            throw;
        }
    }

    private async Task ExecuteMacro(MacroInstance macro, CancellationToken externalToken, TriggerEventArgs? triggerArgs = null)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, macro.CancellationSource.Token);
        var token = linkedCts.Token;
        UserDataDescriptorProvider.Default.SetInstanceDescriptor<LimitedServiceClass>();

        try
        {
            Svc.Log.Debug($"Starting Lua macro execution for macro {macro.Macro.Id}");
            using var lua = new Lua();
            lua.SetGlobal("Svc", new LimitedServiceClass());
            macro.LuaGenerator = lua.Load(macro.Macro.Content);
            if (macro.LuaGenerator == null)
                throw new LuaException("Failed to load Lua script: No function returned");
            await Svc.Framework.RunOnTick(() =>
            {

            });
        }

        //    await Svc.Framework.RunOnTick(async () =>
        //    {
        //        try
        //        {
        //            while (!token.IsCancellationRequested)
        //            {
        //                try
        //                {
        //                    // Wait if paused
        //                    macro.PauseEvent.Wait(token);

        //                    if (macro.LuaGenerator == null)
        //                        break;

        //                    var result = macro.LuaGenerator.Call();
        //                    if (result.Count == 0) // completed
        //                        break;

        //                    if (result.First.Value is not string text)
        //                    {
        //                        var valueType = result.First.Value?.GetType().Name ?? "null";
        //                        var valueStr = result.First.Value?.ToString() ?? "null";
        //                        throw new MacroException($"Lua Macro yielded a non-string value [{valueType}: {valueStr}]");
        //                    }

        //                    var tempMacro = new TemporaryMacro(text);
        //                    var nativeMacroId = $"{macro.Macro.Id}_native_{Guid.NewGuid()}";
        //                    _temporaryMacros[nativeMacroId] = tempMacro;
        //                    Svc.Log.Debug($"Created temporary macro {nativeMacroId} with content: {text}");

        //                    // TCS to wait for the native macro to complete
        //                    var tcs = new TaskCompletionSource<bool>();
        //                    var actualMacroId = string.Empty;
        //                    var firstStateChange = true;
        //                    macro.StateChangedHandler = (sender, e) =>
        //                    {
        //                        // On the first Running state, capture the actual macro ID
        //                        if (firstStateChange && e.NewState == MacroState.Running)
        //                        {
        //                            firstStateChange = false;
        //                            actualMacroId = e.MacroId;
        //                        }

        //                        // If we have an actual macro ID, use it for matching
        //                        if (!string.IsNullOrEmpty(actualMacroId) && e.MacroId == actualMacroId && e.NewState is MacroState.Completed or MacroState.Error)
        //                            tcs.TrySetResult(e.NewState == MacroState.Completed);
        //                    };

        //                    if (Scheduler is { })
        //                        Scheduler.MacroStateChanged += macro.StateChangedHandler;

        //                    MacroControlRequested?.Invoke(this, new MacroControlEventArgs(nativeMacroId, MacroControlType.Start));

        //                    var completedSuccessfully = await tcs.Task;

        //                    if (Scheduler is { } && macro.StateChangedHandler is { })
        //                    {
        //                        Scheduler.MacroStateChanged -= macro.StateChangedHandler;
        //                        macro.StateChangedHandler = null;
        //                    }

        //                    _temporaryMacros.Remove(nativeMacroId);

        //                    // Must propagate the macro error back to the caller
        //                    if (!completedSuccessfully)
        //                        throw new MacroException($"Temporary macro {nativeMacroId} failed");

        //                    MacroStepCompleted?.Invoke(this, new MacroStepCompletedEventArgs(macro.Macro.Id, 1, 1));

        //                    await Svc.Framework.DelayTicks(1);
        //                }
        //                catch (OperationCanceledException)
        //                {
        //                    Svc.Log.Debug($"Operation cancelled for macro {macro.Macro.Id}");
        //                    break;
        //                }
        //                catch (LuaException ex)
        //                {
        //                    Svc.Log.Error(ex, $"Lua execution error for macro {macro.Macro.Id}");
        //                    break;
        //                }
        //                catch (Exception ex)
        //                {
        //                    var errorDetails = "Unknown error";
        //                    Svc.Log.Error(ex, $"Error executing Lua function for macro {macro.Macro.Id}: {errorDetails}");
        //                    break;
        //                }
        //            }
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            Svc.Log.Debug($"Operation cancelled for macro {macro.Macro.Id}");
        //        }
        //        catch (Exception ex)
        //        {
        //            Svc.Log.Error(ex, $"Error in Lua macro execution for {macro.Macro.Id}");
        //        }
        //        finally
        //        {
        //            // Ensure we unsubscribe from the event
        //            if (Scheduler is { } scheduler && macro.StateChangedHandler is { })
        //            {
        //                scheduler.MacroStateChanged -= macro.StateChangedHandler;
        //                macro.StateChangedHandler = null;
        //            }
        //        }
        //    }, cancellationToken: token).ConfigureAwait(false);
        //}
        catch (OperationCanceledException)
        {
            Svc.Log.Info($"Operation cancelled for macro {macro.Macro.Id}");
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error executing macro {macro.Macro.Id}: {ex}");
            OnMacroError(macro.Macro.Id, "Error executing macro", ex);
            throw;
        }
        finally
        {
            macro.Dispose();
        }
    }

    protected virtual void OnMacroError(string macroId, string message, Exception? ex = null)
        => MacroError?.Invoke(this, new MacroErrorEventArgs(macroId, message, ex));

    /// <inheritdoc/>
    public void Dispose() { }
}
