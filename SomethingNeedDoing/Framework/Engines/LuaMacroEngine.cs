using NLua;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework.Engines;

/// <summary>
/// Executes Lua script macros using NLua.
/// </summary>
public class LuaMacroEngine : IMacroEngine, IMacroScheduler
{
    private readonly LuaModuleManager _moduleManager = new();
    private bool _isDisposed;

    /// <inheritdoc/>
    public event EventHandler<MacroStateChangedEventArgs>? MacroStateChanged;

    /// <inheritdoc/>
    public event EventHandler<MacroErrorEventArgs>? MacroError;

    /// <summary>
    /// Represents the current state of a macro execution.
    /// </summary>
    private class MacroInstance(LuaMacroEngine engine, IMacro macro) : IDisposable
    {
        public IMacro Macro { get; } = macro;
        public LuaFunction? LuaGenerator { get; set; }
        public CancellationTokenSource CancellationSource { get; } = new();
        public ManualResetEventSlim PauseEvent { get; } = new(true);
        public Task? ExecutionTask { get; set; }
        public MacroState CurrentState
        {
            get; set
            {
                if (field != value)
                    engine.OnMacroStateChanged(Macro.Id, value, field);
            }
        } = MacroState.Ready;
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
        if (macro.Type != MacroType.Lua)
            throw new ArgumentException("This engine only supports Lua macros", nameof(macro));

        var state = new MacroInstance(this, macro);

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

        macro.CurrentState = MacroState.Running;

        try
        {
            using var lua = new Lua();
            lua.State.Encoding = Encoding.UTF8;

            lua.LoadCLRPackage();
            lua.LoadFStrings();
            lua.LoadErrorHandler();

            // Register modules and services
            LuaServiceProxy.RegisterServices(lua);
            _moduleManager.RegisterAll(lua);

            if (_moduleManager.GetModule<TriggerModule>() is { } triggerModule)
                triggerModule.SetTriggerArgs(triggerArgs);

            await Svc.Framework.RunOnTick(async () =>
            {
                try
                {
                    // Execute the script
                    var results = lua.LoadEntryPointWrappedScript(macro.Macro.Content);
                    if (results.Length == 0 || results[0] is not LuaFunction func)
                        throw new LuaException("Failed to load Lua script: No function returned");

                    macro.LuaGenerator = func;

                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            // Wait if paused
                            macro.PauseEvent.Wait(token);

                            var result = func.Call();
                            if (result.Length == 0)
                                break;

                            if (result.First() is not string text)
                            {
                                var valueType = result.First()?.GetType().Name ?? "null";
                                var valueStr = result.First()?.ToString() ?? "null";
                                throw new MacroException($"Lua Macro yielded a non-string value [{valueType}: {valueStr}]");
                            }

                            // Create a temporary macro with the text as content
                            var tempMacro = new TemporaryMacro(text);
                            var nativeMacroId = $"{macro.Macro.Id}_native_{Guid.NewGuid()}";

                            // Create a task completion source to wait for the native macro to complete
                            var tcs = new TaskCompletionSource<bool>();
                            void OnMacroStateChanged(object? sender, MacroStateChangedEventArgs e)
                            {
                                if (e.MacroId == nativeMacroId && e.NewState is MacroState.Completed or MacroState.Error)
                                {
                                    Service.MacroScheduler.MacroStateChanged -= OnMacroStateChanged;
                                    tcs.SetResult(e.NewState == MacroState.Completed);
                                }
                            }

                            Service.MacroScheduler.MacroStateChanged += OnMacroStateChanged;

                            // Start the native macro and wait for it to complete
                            _ = Service.MacroScheduler.StartMacro(tempMacro);
                            await tcs.Task;
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (LuaException ex)
                        {
                            Svc.Log.Error($"Lua execution error: {ex.Message}");
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

                            Svc.Log.Error($"Error executing Lua function: {errorDetails}", ex);
                            break;
                        }

                        await Svc.Framework.DelayTicks(1);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Svc.Log.Error($"{ex}");
                }
            }, cancellationToken: token).ConfigureAwait(false);

            macro.CurrentState = MacroState.Completed;
        }
        catch (OperationCanceledException)
        {
            macro.CurrentState = MacroState.Completed;
        }
        catch (Exception ex)
        {
            macro.CurrentState = MacroState.Error;
            OnMacroError(macro.Macro.Id, "Error executing macro", ex);
            throw;
        }
        finally
        {
            macro.Dispose();
        }
    }

    /// <inheritdoc/>
    public Task StartMacro(IMacro macro) => StartMacro(macro, CancellationToken.None);

    /// <inheritdoc/>
    public Task PauseMacro(string macroId)
    {
        // This method is now handled by the MacroScheduler
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ResumeMacro(string macroId)
    {
        // This method is now handled by the MacroScheduler
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopMacro(string macroId)
    {
        // This method is now handled by the MacroScheduler
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void CheckLoopPause(string macroId)
    {
        // This method is now handled by the MacroScheduler
    }

    /// <inheritdoc/>
    public void CheckLoopStop(string macroId)
    {
        // This method is now handled by the MacroScheduler
    }

    protected virtual void OnMacroStateChanged(string macroId, MacroState newState, MacroState oldState)
        => MacroStateChanged?.Invoke(this, new MacroStateChangedEventArgs(macroId, newState, oldState));

    protected virtual void OnMacroError(string macroId, string message, Exception? ex = null)
        => MacroError?.Invoke(this, new MacroErrorEventArgs(macroId, message, ex));

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
    }
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
