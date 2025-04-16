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
    private readonly ConcurrentDictionary<string, MacroInstance> _runningMacros = [];
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
    public async Task StartMacro(IMacro macro, CancellationToken token)
    {
        if (macro.Type != MacroType.Lua)
            throw new ArgumentException("This engine only supports Lua macros", nameof(macro));

        var state = new MacroInstance(this, macro);
        if (!_runningMacros.TryAdd(macro.Id, state))
            throw new InvalidOperationException($"Macro {macro.Id} is already running");

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
        finally
        {
            if (_runningMacros.TryRemove(macro.Id, out var removedState))
                removedState.Dispose();
        }
    }

    private async Task ExecuteMacro(MacroInstance macro, CancellationToken externalToken)
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

            await Svc.Framework.RunOnFrameworkThread(() =>
            {
                try
                {
                    // Execute the script
                    var results = lua.LoadEntryPointWrappedScript(macro.Macro.Content);
                    if (results.Length == 0 || results[0] is not LuaFunction func)
                        throw new LuaException("Failed to load Lua script: No function returned");

                    macro.LuaGenerator = func;

                    try
                    {
                        var result = func.Call();
                        if (result.Length == 0)
                            return;

                        if (result.First() is not string text)
                        {
                            var valueType = result.First()?.GetType().Name ?? "null";
                            var valueStr = result.First()?.ToString() ?? "null";
                            throw new MacroException($"Lua Macro yielded a non-string value [{valueType}: {valueStr}]");
                        }

                        // TODO: add text as the next step. Must be parsed by the native macro engine.
                    }
                    catch (LuaException ex)
                    {
                        Svc.Log.Error($"Lua execution error: {ex.Message}");
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
                    }
                }
                catch (Exception ex)
                {
                    Svc.Log.Error($"{ex}");
                }
            });

            macro.CurrentState = MacroState.Completed;
        }
        catch (Exception ex)
        {
            macro.CurrentState = MacroState.Error;
            OnMacroError(macro.Macro.Id, "Error executing macro", ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task StartMacro(IMacro macro) => StartMacro(macro, CancellationToken.None);

    /// <inheritdoc/>
    public Task PauseMacro(string macroId)
    {
        if (_runningMacros.TryGetValue(macroId, out var macro))
        {
            macro.PauseEvent.Reset();
            macro.CurrentState = MacroState.Paused;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ResumeMacro(string macroId)
    {
        if (_runningMacros.TryGetValue(macroId, out var macro))
        {
            macro.PauseEvent.Set();
            macro.CurrentState = MacroState.Running;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopMacro(string macroId)
    {
        if (_runningMacros.TryGetValue(macroId, out var macro))
            macro.CancellationSource.Cancel();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void CheckLoopPause(string macroId)
    {
        if (_runningMacros.TryGetValue(macroId, out var macro) && macro.PauseAtLoop)
        {
            macro.PauseAtLoop = false;
            macro.PauseEvent.Reset();
            macro.CurrentState = MacroState.Paused;
        }
    }

    /// <inheritdoc/>
    public void CheckLoopStop(string macroId)
    {
        if (_runningMacros.TryGetValue(macroId, out var macro) && macro.StopAtLoop)
        {
            macro.StopAtLoop = false;
            macro.CancellationSource.Cancel();
        }
    }

    protected virtual void OnMacroStateChanged(string macroId, MacroState newState, MacroState oldState)
        => MacroStateChanged?.Invoke(this, new MacroStateChangedEventArgs(macroId, newState, oldState));

    protected virtual void OnMacroError(string macroId, string message, Exception? ex = null)
        => MacroError?.Invoke(this, new MacroErrorEventArgs(macroId, message, ex));

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
