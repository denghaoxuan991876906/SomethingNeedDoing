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

                            Svc.Log.Info($"Calling Lua function for macro {macro.Macro.Id}");
                            var result = func.Call();
                            if (result.Length == 0)
                            {
                                Svc.Log.Info($"Lua function completed for macro {macro.Macro.Id}");
                                break;
                            }

                            if (result.First() is not string text)
                            {
                                var valueType = result.First()?.GetType().Name ?? "null";
                                var valueStr = result.First()?.ToString() ?? "null";
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
                            void OnMacroStateChanged(object? sender, MacroStateChangedEventArgs e)
                            {
                                Svc.Log.Info($"Received MacroStateChanged event for {e.MacroId} with state {e.NewState}");
                                if (e.MacroId == nativeMacroId && e.NewState is MacroState.Completed or MacroState.Error)
                                {
                                    Svc.Log.Info($"Setting task completion for {nativeMacroId} to {e.NewState == MacroState.Completed}");
                                    tcs.SetResult(e.NewState == MacroState.Completed);
                                }
                            }

                            Svc.Log.Info($"Raising MacroControlRequested event for {nativeMacroId}");
                            MacroControlRequested?.Invoke(this, new MacroControlEventArgs(nativeMacroId, MacroControlType.Start));

                            Svc.Log.Info($"Waiting for temporary macro {nativeMacroId} to complete");
                            await tcs.Task;
                            Svc.Log.Info($"Temporary macro {nativeMacroId} completed");

                            // Clean up the temporary macro
                            _temporaryMacros.Remove(nativeMacroId);

                            // Raise step completed event
                            MacroStepCompleted?.Invoke(this, new MacroStepCompletedEventArgs(macro.Macro.Id, 1, 1));
                        }
                        catch (OperationCanceledException)
                        {
                            Svc.Log.Info($"Operation cancelled for macro {macro.Macro.Id}");
                            throw;
                        }
                        catch (LuaException ex)
                        {
                            Svc.Log.Error($"Lua execution error for macro {macro.Macro.Id}: {ex.Message}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            var errorDetails = "Unknown error";
                            try
                            {
                                errorDetails = lua.GetLuaErrorDetails();
                            }
                            catch
                            {
                                errorDetails = ex.Message;
                            }

                            Svc.Log.Error($"Error executing Lua function for macro {macro.Macro.Id}: {errorDetails}", ex);
                            break;
                        }

                        await Svc.Framework.DelayTicks(1);
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
