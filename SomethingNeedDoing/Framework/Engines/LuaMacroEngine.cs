using NLua;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;

/// <summary>
/// Executes Lua script macros using NLua.
/// </summary>
public class LuaMacroEngine(LuaModuleManager moduleManager) : IMacroEngine
{
    /// <inheritdoc/>
    public event EventHandler<MacroErrorEventArgs>? MacroError;

    /// <inheritdoc/>
    public event EventHandler<MacroControlEventArgs>? MacroControlRequested;

    /// <inheritdoc/>
    public event EventHandler<MacroStepCompletedEventArgs>? MacroStepCompleted;

    /// <inheritdoc/>
    public IMacroScheduler? Scheduler { get; set; }

    private readonly Dictionary<string, TemporaryMacro> _temporaryMacros = new();

    /// <inheritdoc/>
    public IMacro? GetTemporaryMacro(string macroId)
    {
        return _temporaryMacros.TryGetValue(macroId, out var macro) ? macro : null;
    }

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

        try
        {
            Svc.Log.Info($"Starting Lua macro execution for macro {macro.Macro.Id}");
            using var lua = new Lua();
            lua.State.Encoding = Encoding.UTF8;

            lua.LoadCLRPackage();
            lua.LoadFStrings();
            lua.LoadErrorHandler();

            // Register modules and services
            LuaServiceProxy.RegisterServices(lua);
            moduleManager.RegisterAll(lua);
            lua.SetTriggerEventData(triggerArgs);

            await Svc.Framework.RunOnTick(async () =>
            {
                try
                {
                    Svc.Log.Info($"Loading Lua script for macro {macro.Macro.Id}");
                    // Execute the script
                    var results = lua.LoadEntryPointWrappedScript(macro.Macro.Content);
                    if (results.Length == 0 || results[0] is not LuaFunction func)
                        throw new LuaException("Failed to load Lua script: No function returned");

                    macro.LuaGenerator = func;
                    Svc.Log.Info($"Lua script loaded successfully for macro {macro.Macro.Id}");

                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            // Wait if paused
                            macro.PauseEvent.Wait(token);

                            Svc.Log.Info($"Starting Lua function call for macro {macro.Macro.Id}");
                            if (macro.LuaGenerator == null)
                            {
                                Svc.Log.Error($"Lua generator is null for macro {macro.Macro.Id}");
                                break;
                            }

                            Svc.Log.Info($"Calling Lua function for macro {macro.Macro.Id}");
                            var result = macro.LuaGenerator.Call();
                            Svc.Log.Info($"Lua function call completed for macro {macro.Macro.Id}, result length: {result.Length}");

                            if (result.Length == 0)
                            {
                                Svc.Log.Info($"Lua function completed for macro {macro.Macro.Id}");
                                break;
                            }

                            if (result.First() is not string text)
                            {
                                var valueType = result.First()?.GetType().Name ?? "null";
                                var valueStr = result.First()?.ToString() ?? "null";
                                Svc.Log.Warning($"Lua Macro yielded a non-string value [{valueType}: {valueStr}]");
                                throw new MacroException($"Lua Macro yielded a non-string value [{valueType}: {valueStr}]");
                            }

                            Svc.Log.Info($"Lua macro {macro.Macro.Id} yielded command: {text}");

                            // Create a temporary macro with the text as content
                            var tempMacro = new TemporaryMacro(text);
                            var nativeMacroId = $"{macro.Macro.Id}_native_{Guid.NewGuid()}";
                            Svc.Log.Info($"Created temporary macro {nativeMacroId} with content: {text}");

                            // Store the temporary macro
                            _temporaryMacros[nativeMacroId] = tempMacro;

                            // Create a task completion source to wait for the native macro to complete
                            var tcs = new TaskCompletionSource<bool>();
                            var actualMacroId = string.Empty;
                            var firstStateChange = true;
                            macro.StateChangedHandler = (sender, e) =>
                            {
                                Svc.Log.Info($"[LuaMacroEngine] Received MacroStateChanged event for {e.MacroId} with state {e.NewState}, expecting {nativeMacroId}");

                                // On the first Running state, capture the actual macro ID
                                if (firstStateChange && e.NewState == MacroState.Running)
                                {
                                    firstStateChange = false;
                                    actualMacroId = e.MacroId;
                                    Svc.Log.Info($"[LuaMacroEngine] Found actual macro ID: {actualMacroId} for temporary macro {nativeMacroId}");
                                }

                                // If we have an actual macro ID, use it for matching
                                if (!string.IsNullOrEmpty(actualMacroId) && e.MacroId == actualMacroId && e.NewState is MacroState.Completed or MacroState.Error)
                                {
                                    Svc.Log.Info($"[LuaMacroEngine] Setting task completion for {nativeMacroId} (actual ID: {actualMacroId}) to {e.NewState == MacroState.Completed}");
                                    var success = tcs.TrySetResult(e.NewState == MacroState.Completed);
                                    Svc.Log.Info($"[LuaMacroEngine] Task completion set result: {success}");
                                }
                                // If we don't have an actual macro ID yet, try to match by prefix
                                else if (string.IsNullOrEmpty(actualMacroId) && e.MacroId.StartsWith($"{nativeMacroId}_") && e.NewState is MacroState.Completed or MacroState.Error)
                                {
                                    Svc.Log.Info($"[LuaMacroEngine] Setting task completion for {nativeMacroId} (matched by prefix) to {e.NewState == MacroState.Completed}");
                                    var success = tcs.TrySetResult(e.NewState == MacroState.Completed);
                                    Svc.Log.Info($"[LuaMacroEngine] Task completion set result: {success}");
                                }
                            };

                            // Subscribe to the event
                            if (Scheduler is { } scheduler)
                            {
                                Svc.Log.Info($"[LuaMacroEngine] Subscribing to MacroStateChanged for {nativeMacroId}");
                                scheduler.MacroStateChanged += macro.StateChangedHandler;
                            }

                            Svc.Log.Info($"[LuaMacroEngine] Raising MacroControlRequested event for {nativeMacroId}");
                            MacroControlRequested?.Invoke(this, new MacroControlEventArgs(nativeMacroId, MacroControlType.Start));

                            Svc.Log.Info($"[LuaMacroEngine] Waiting for temporary macro {nativeMacroId} to complete");
                            var completedSuccessfully = await tcs.Task;
                            Svc.Log.Info($"[LuaMacroEngine] Temporary macro {nativeMacroId} (actual ID: {actualMacroId}) completed with success: {completedSuccessfully}");

                            // Unsubscribe from the event
                            if (Scheduler is { } scheduler2 && macro.StateChangedHandler != null)
                            {
                                Svc.Log.Info($"[LuaMacroEngine] Unsubscribing from MacroStateChanged for {nativeMacroId} (actual ID: {actualMacroId})");
                                scheduler2.MacroStateChanged -= macro.StateChangedHandler;
                                macro.StateChangedHandler = null;
                            }

                            // Clean up the temporary macro
                            _temporaryMacros.Remove(nativeMacroId);

                            // If the temporary macro failed, propagate the error
                            if (!completedSuccessfully)
                            {
                                Svc.Log.Error($"[LuaMacroEngine] Temporary macro {nativeMacroId} (actual ID: {actualMacroId}) failed");
                                throw new MacroException($"Temporary macro {nativeMacroId} failed");
                            }

                            // Raise step completed event
                            MacroStepCompleted?.Invoke(this, new MacroStepCompletedEventArgs(macro.Macro.Id, 1, 1));

                            Svc.Log.Info($"[LuaMacroEngine] Continuing Lua macro execution for {macro.Macro.Id}");
                            await Svc.Framework.DelayTicks(1);

                            Svc.Log.Info($"[LuaMacroEngine] Starting next iteration of Lua macro {macro.Macro.Id}");
                        }
                        catch (OperationCanceledException)
                        {
                            Svc.Log.Info($"Operation cancelled for macro {macro.Macro.Id}");
                            throw;
                        }
                        catch (LuaException ex)
                        {
                            Svc.Log.Error($"Lua execution error for macro {macro.Macro.Id}: {ex.Message}");
                            Svc.Log.Error($"Lua stack trace: {ex.StackTrace}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            var errorDetails = "Unknown error";
                            try
                            {
                                errorDetails = lua.GetLuaErrorDetails();
                                Svc.Log.Error($"Lua error details: {errorDetails}");
                            }
                            catch
                            {
                                errorDetails = ex.Message;
                            }

                            Svc.Log.Error($"Error executing Lua function for macro {macro.Macro.Id}: {errorDetails}", ex);
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Svc.Log.Info($"Operation cancelled for macro {macro.Macro.Id}");
                }
                catch (Exception ex)
                {
                    Svc.Log.Error($"Error in Lua macro execution for {macro.Macro.Id}: {ex}");
                }
                finally
                {
                    // Ensure we unsubscribe from the event
                    if (Scheduler is { } scheduler && macro.StateChangedHandler != null)
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
