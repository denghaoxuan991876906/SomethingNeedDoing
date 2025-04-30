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
                                    tcs.SetResult(e.NewState == MacroState.Completed);
                                }
                            }

                            // Raise control event to start the macro
                            MacroControlRequested?.Invoke(this, new MacroControlEventArgs(nativeMacroId, MacroControlType.Start));
                            await tcs.Task;

                            // Raise step completed event
                            MacroStepCompleted?.Invoke(this, new MacroStepCompletedEventArgs(macro.Macro.Id, 1, 1));
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

                }
                catch (Exception ex)
                {
                    Svc.Log.Error($"{ex}");
                }
            }, cancellationToken: token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception ex)
        {
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
