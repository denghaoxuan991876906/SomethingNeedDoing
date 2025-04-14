using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AutoRetainerAPI;
using ECommons.Logging;
using SomethingNeedDoing.Framework.Engines;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Manages and coordinates execution of multiple macros.
/// </summary>
public class MacroScheduler : IMacroScheduler, IDisposable
{
    private readonly ConcurrentDictionary<string, IMacroEngine> _enginesByMacroId = [];
    private readonly ConcurrentDictionary<string, MacroExecutionState> _macroStates = [];
    private readonly Dictionary<string, AutoRetainerApi> _arApis = [];
    private readonly NativeMacroEngine _nativeEngine = new();
    private readonly LuaMacroEngine _luaEngine = new();
    private bool _isDisposed;

    /// <summary>
    /// Event raised when any macro's state changes.
    /// </summary>
    public event EventHandler<MacroStateChangedEventArgs>? MacroStateChanged;

    /// <summary>
    /// Event raised when any macro encounters an error.
    /// </summary>
    public event EventHandler<MacroErrorEventArgs>? MacroError;

    public MacroScheduler()
    {
        _nativeEngine.MacroStateChanged += OnEngineStateChanged;
        _nativeEngine.MacroError += OnEngineError;
        _luaEngine.MacroStateChanged += OnEngineStateChanged;
        _luaEngine.MacroError += OnEngineError;

        C.Macros.ForEach(m =>
        {
            if (m.Metadata.TriggerEvents.Contains(TriggerEvent.AutoRetainerCharacterPostProcess))
            {
                _arApis.TryAdd(m.Id, new AutoRetainerApi());
                _arApis[m.Id].OnCharacterPostprocessStep += () => CheckCharacterPostProcess(m);
                _arApis[m.Id].OnCharacterReadyToPostProcess += () => DoCharacterPostProcess(m);
            }
        });
    }

    /// <summary>
    /// Starts execution of a macro.
    /// </summary>
    /// <param name="macro">The macro to execute.</param>
    public async Task StartMacro(IMacro macro)
    {
        IMacroEngine engine = macro.Type switch
        {
            MacroType.Native => _nativeEngine,
            MacroType.Lua => _luaEngine,
            _ => throw new ArgumentException($"Unsupported macro type: {macro.Type}")
        };

        if (_enginesByMacroId.TryAdd(macro.Id, engine))
        {
            try
            {
                await engine.StartMacro(macro, CancellationToken.None);
            }
            catch
            {
                _enginesByMacroId.TryRemove(macro.Id, out _);
                throw;
            }
        }
        else
            throw new InvalidOperationException($"Macro {macro.Id} is already running");
    }

    /// <summary>
    /// Pauses execution of a macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to pause.</param>
    public async Task PauseMacro(string macroId)
    {
        if (_enginesByMacroId.TryGetValue(macroId, out var engine))
        {
            await engine.PauseMacro(macroId);
        }
    }

    /// <summary>
    /// Resumes execution of a paused macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to resume.</param>
    public async Task ResumeMacro(string macroId)
    {
        if (_enginesByMacroId.TryGetValue(macroId, out var engine))
        {
            await engine.ResumeMacro(macroId);
        }
    }

    /// <summary>
    /// Stops execution of a macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to stop.</param>
    public async Task StopMacro(string macroId)
    {
        if (_enginesByMacroId.TryGetValue(macroId, out var engine))
        {
            await engine.StopMacro(macroId);
            _enginesByMacroId.TryRemove(macroId, out _);
        }
    }

    /// <summary>
    /// Stops all running macros.
    /// </summary>
    public async Task StopAllMacros()
    {
        var tasks = _enginesByMacroId.Keys.Select(StopMacro);
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Checks if the macro should pause at the current loop point.
    /// </summary>
    public void CheckLoopPause(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state) && state.PauseAtLoop)
        {
            state.PauseAtLoop = false;
            state.PauseEvent.Reset();
            state.State = MacroState.Paused;
        }
    }

    /// <summary>
    /// Checks if the macro should stop at the current loop point.
    /// </summary>
    public void CheckLoopStop(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state) && state.StopAtLoop)
        {
            state.StopAtLoop = false;
            state.CancellationSource.Cancel();
        }
    }

    /// <summary>
    /// Sets a macro to pause at the next loop point.
    /// </summary>
    public Task PauseAtNextLoop(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state))
        {
            state.PauseAtLoop = true;
            state.StopAtLoop = false;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets a macro to stop at the next loop point.
    /// </summary>
    public Task StopAtNextLoop(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state))
        {
            state.PauseAtLoop = false;
            state.StopAtLoop = true;
        }
        return Task.CompletedTask;
    }

    private class MacroExecutionState(IMacro macro)
    {
        public IMacro Macro { get; } = macro;
        public MacroState State { get; set; } = MacroState.Ready;
        public bool PauseAtLoop { get; set; }
        public bool StopAtLoop { get; set; }
        public CancellationTokenSource CancellationSource { get; } = new CancellationTokenSource();
        public ManualResetEventSlim PauseEvent { get; } = new ManualResetEventSlim(true);

        public void Dispose()
        {
            CancellationSource.Dispose();
            PauseEvent.Dispose();
        }
    }

    private void OnEngineStateChanged(object? sender, MacroStateChangedEventArgs e)
    {
        if (e.NewState is MacroState.Completed or MacroState.Error)
            _enginesByMacroId.TryRemove(e.MacroId, out _);
        MacroStateChanged?.Invoke(this, e);
    }

    private void OnEngineError(object? sender, MacroErrorEventArgs e) => MacroError?.Invoke(this, e);

    private void CheckCharacterPostProcess(ConfigMacro macro)
    {
        if (C.ARCharacterPostProcessExcludedCharacters.Any(x => x == Svc.ClientState.LocalContentId))
            Svc.Log.Info($"Skipping post process macro {macro.Name} for current character.");
        else
            _arApis[macro.Id].RequestCharacterPostprocess();
    }

    private void DoCharacterPostProcess(ConfigMacro macro)
    {
        MacroStateChanged += (sender, e) => OnPostProcessMacroCompleted(sender, e, macro);
        _ = StartMacro(macro);
    }

    private void OnPostProcessMacroCompleted(object? sender, MacroStateChangedEventArgs e, ConfigMacro macro)
    {
        if (e.NewState is MacroState.Completed)
        {
            Svc.Framework.RunOnFrameworkThread(() =>
            {
                PluginLog.Debug($"Finishing post process macro {macro.Name} for current character.");
                if (_arApis.TryGetValue(e.MacroId, out var arApi))
                    arApi.FinishCharacterPostProcess();
            });
            MacroStateChanged -= (sender, e) => OnPostProcessMacroCompleted(sender, e, macro);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_isDisposed) return;

        _nativeEngine.Dispose();
        _luaEngine.Dispose();
        _enginesByMacroId.Clear();
        _arApis.Values.ForEach(a => a.Dispose());
        _arApis.Clear();

        _isDisposed = true;
    }
}
