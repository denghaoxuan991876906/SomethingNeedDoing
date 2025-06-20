using NLua;
using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Utils;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.LuaMacro;

/// <summary>
/// Executes Lua script macros using NLua.
/// </summary>
public class NLuaMacroEngine(LuaModuleManager moduleManager) : IMacroEngine
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

    /// <inheritdoc/>
    public async Task StartMacro(IMacro macro, CancellationToken token, TriggerEventArgs? triggerArgs = null, int _ = 0)
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

    private async Task ExecuteMacro(MacroInstance macro, CancellationToken externalToken, TriggerEventArgs? triggerArgs = null, int _ = 0)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, macro.CancellationSource.Token);
        var token = linkedCts.Token;

        try
        {
            Svc.Log.Debug($"Starting Lua macro execution for macro {macro.Macro.Id}");
            using var lua = new Lua();
            lua.State.Encoding = Encoding.UTF8;

            lua.LoadCLRPackage();
            lua.LoadFStrings();
            lua.LoadPackageSearcherSnippet();
            lua.LoadRequirePaths();
            lua.LoadErrorHandler();
            lua.ApplyPrintOverride();
            lua.SetTriggerEventData(triggerArgs);
            lua.RegisterClass<Svc>();
            lua.DoString("luanet.load_assembly('FFXIVClientStructs')");
            moduleManager.RegisterAll(lua);

            await Svc.Framework.RunOnTick(async () =>
            {
                try
                {
                    // Execute the script
                    var results = lua.LoadEntryPointWrappedScript(macro.Macro.ContentSansMetadata());
                    if (results.Length == 0 || results[0] is not LuaFunction func)
                        throw new LuaException("Failed to load Lua script: No function returned");

                    macro.LuaGenerator = func;

                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            // Wait if paused
                            macro.PauseEvent.Wait(token);

                            if (macro.LuaGenerator == null)
                                break;

                            var (macroComplete, result) = await Svc.Framework.RunOnTick(() =>
                            {
                                var result = macro.LuaGenerator.Call();
                                return (result.Length == 0, result);
                            });

                            if (macroComplete)
                                break;

                            if (result.First() is not string text)
                            {
                                var valueType = result.First()?.GetType().Name ?? "null";
                                var valueStr = result.First()?.ToString() ?? "null";
                                throw new MacroException($"Lua Macro yielded a non-string value [{valueType}: {valueStr}]");
                            }

                            var tempMacro = new TemporaryMacro(text);
                            var nativeMacroId = $"{macro.Macro.Id}_native_{Guid.NewGuid()}";
                            _temporaryMacros[nativeMacroId] = tempMacro;
                            Svc.Log.Debug($"Created temporary macro {nativeMacroId} with content: {text}");

                            // TCS to wait for the native macro to complete
                            var tcs = new TaskCompletionSource<bool>();
                            var actualMacroId = string.Empty;
                            var firstStateChange = true;
                            macro.StateChangedHandler = (sender, e) =>
                            {
                                // On the first Running state, capture the actual macro ID
                                if (firstStateChange && e.NewState == MacroState.Running)
                                {
                                    firstStateChange = false;
                                    actualMacroId = e.MacroId;
                                }

                                // If we have an actual macro ID, use it for matching
                                if (!string.IsNullOrEmpty(actualMacroId) && e.MacroId == actualMacroId && e.NewState is MacroState.Completed or MacroState.Error)
                                    tcs.TrySetResult(e.NewState == MacroState.Completed);
                            };

                            if (Scheduler is { })
                                Scheduler.MacroStateChanged += macro.StateChangedHandler;

                            MacroControlRequested?.Invoke(this, new MacroControlEventArgs(nativeMacroId, MacroControlType.Start));

                            var completedSuccessfully = await tcs.Task;

                            if (Scheduler is { } && macro.StateChangedHandler is { })
                            {
                                Scheduler.MacroStateChanged -= macro.StateChangedHandler;
                                macro.StateChangedHandler = null;
                            }

                            _temporaryMacros.Remove(nativeMacroId);

                            // Must propagate the macro error back to the caller
                            if (!completedSuccessfully)
                                throw new MacroException($"Temporary macro {nativeMacroId} failed");

                            MacroStepCompleted?.Invoke(this, new MacroStepCompletedEventArgs(macro.Macro.Id, 1, 1));

                            await Svc.Framework.DelayTicks(1);
                        }
                        catch (OperationCanceledException)
                        {
                            Svc.Log.Debug($"Operation cancelled for macro {macro.Macro.Id}");
                            break;
                        }
                        catch (LuaException ex)
                        {
                            Svc.Log.Error(ex, $"Lua execution error for macro {macro.Macro.Id}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            var errorDetails = "Unknown error";
                            //try
                            //{
                            //    errorDetails = lua.GetLuaErrorDetails();
                            //    Svc.Log.Error($"Lua error details: {errorDetails}");
                            //}
                            //catch
                            //{
                            //    errorDetails = ex.Message;
                            //}

                            Svc.Log.Error(ex, $"Error executing Lua function for macro {macro.Macro.Id}: {errorDetails}");
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Svc.Log.Debug($"Operation cancelled for macro {macro.Macro.Id}");
                }
                catch (Exception ex)
                {
                    Svc.Log.Error(ex, $"Error in Lua macro execution for {macro.Macro.Id}");
                }
                finally
                {
                    // Ensure we unsubscribe from the event
                    if (Scheduler is { } scheduler && macro.StateChangedHandler is { })
                    {
                        scheduler.MacroStateChanged -= macro.StateChangedHandler;
                        macro.StateChangedHandler = null;
                    }
                }
            }, cancellationToken: token).ConfigureAwait(false);
        }
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

/// <summary>
/// Exception thrown by Lua code.
/// </summary>
public class LuaException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LuaException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public LuaException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LuaException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LuaException(string message, Exception innerException) : base(message, innerException) { }
}
