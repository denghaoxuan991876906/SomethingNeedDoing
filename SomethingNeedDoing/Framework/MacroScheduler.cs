using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AutoRetainerAPI;
using ECommons.Logging;
using SomethingNeedDoing.Framework.Engines;
using Dalamud.Game.Text;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;

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
    private readonly Dictionary<string, AddonEventConfig> _addonEvents = [];
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
        SubscribeToTriggerEvents();
    }

    private void SubscribeToTriggerEvents()
    {
        C.Macros.ForEach(SubscribeCustomTriggers);

        Svc.Framework.Update += OnFrameworkUpdate;
        Svc.Condition.ConditionChange += OnConditionChange;
        Svc.ClientState.TerritoryChanged += OnTerritoryChanged;
        Svc.Chat.ChatMessage += OnChatMessage;
        Svc.ClientState.Login += OnLogin;
        Svc.ClientState.Logout += OnLogout;
    }

    private void SubscribeCustomTriggers(ConfigMacro macro)
    {
        foreach (var triggerEvent in macro.Metadata.TriggerEvents)
        {
            switch (triggerEvent)
            {
                case TriggerEvent.OnAutoRetainerCharacterPostProcess:
                    if (!_arApis.ContainsKey(macro.Id))
                    {
                        _arApis.TryAdd(macro.Id, new AutoRetainerApi());
                        _arApis[macro.Id].OnCharacterPostprocessStep += () => CheckCharacterPostProcess(macro);
                        _arApis[macro.Id].OnCharacterReadyToPostProcess += () => DoCharacterPostProcess(macro);
                    }
                    break;
                case TriggerEvent.OnAddonEvent:
                    if (macro.Metadata.AddonEventConfig is { } cfg)
                    {
                        if (!_addonEvents.ContainsKey(macro.Id))
                        {
                            _addonEvents.TryAdd(macro.Id, cfg);
                            Svc.AddonLifecycle.RegisterListener(cfg.EventType, cfg.AddonName, OnAddonEvent);
                        }
                    }
                    break;
            }
        }
    }

    private void OnAddonEvent(AddonEvent type, AddonArgs args)
    {
        foreach (var kvp in _addonEvents)
            if (kvp.Value is { } cfg && cfg.EventType == type && cfg.AddonName == args.AddonName && C.GetMacro(kvp.Key) is { } macro)
                macro.Start();
    }

    private void OnFrameworkUpdate(IFramework framework) => C.Macros.Where(m => m.Metadata.TriggerEvents.Contains(TriggerEvent.OnUpdate)).Each(m => m.Start());

    private void OnConditionChange(ConditionFlag flag, bool value)
    {
        var args = new TriggerEventArgs(TriggerEvent.OnConditionChange) { EventData = { ["flag"] = flag, ["value"] = value } };
        C.Macros.Where(m => m.Metadata.TriggerEvents.Contains(TriggerEvent.OnConditionChange)).Each(m => m.Start(args));
    }

    private void OnTerritoryChanged(ushort territoryType)
    {
        var args = new TriggerEventArgs(TriggerEvent.OnTerritoryChange) { EventData = { ["territoryType"] = territoryType } };
        C.Macros.Where(m => m.Metadata.TriggerEvents.Contains(TriggerEvent.OnTerritoryChange)).Each(m => m.Start(args));
    }

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var args = new TriggerEventArgs(TriggerEvent.OnChatMessage);
        args.EventData["type"] = type;
        args.EventData["timestamp"] = timestamp;
        args.EventData["sender"] = sender.TextValue;
        args.EventData["message"] = message.TextValue;

        C.Macros.Where(m => m.Metadata.TriggerEvents.Contains(TriggerEvent.OnChatMessage)).Each(m => m.Start(args));
    }

    private void OnLogin()
        => C.Macros.Where(m => m.Metadata.TriggerEvents.Contains(TriggerEvent.OnLogin)).Each(m => m.Start());

    private void OnLogout(int type, int code)
    {
        var args = new TriggerEventArgs(TriggerEvent.OnLogout) { EventData = { ["type"] = type, ["code"] = code } };
        C.Macros.Where(m => m.Metadata.TriggerEvents.Contains(TriggerEvent.OnLogout)).Each(m => m.Start(args));
    }

    /// <summary>
    /// Gets all currently running macros.
    /// </summary>
    public IEnumerable<IMacro> GetRunningMacros() => _macroStates.Values.Where(s => s.State is MacroState.Running or MacroState.Paused).Select(s => s.Macro);

    /// <summary>
    /// Gets the current state of a macro.
    /// </summary>
    public MacroState GetMacroState(string macroId) => _macroStates.TryGetValue(macroId, out var state) ? state.State : MacroState.Ready;

    /// <summary>
    /// Checks if a macro is actually running.
    /// </summary>
    /// <param name="macroId">The ID of the macro to check.</param>
    /// <returns>True if the macro is running, false otherwise.</returns>
    private bool IsMacroActuallyRunning(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state))
        {
            // Check if the execution task is still running
            if (state.ExecutionTask != null && !state.ExecutionTask.IsCompleted)
                return true;

            // If the task is completed but the macro is still in the dictionary, remove it
            ForceCleanupMacro(macroId);
        }
        return false;
    }

    public async Task StartMacro(IMacro macro) => await StartMacro(macro, null);

    /// <summary>
    /// Forces cleanup of a macro's state.
    /// </summary>
    /// <param name="macroId">The ID of the macro to clean up.</param>
    private void ForceCleanupMacro(string macroId)
    {
        _enginesByMacroId.TryRemove(macroId, out _);
        _macroStates.TryRemove(macroId, out _);
    }

    /// <summary>
    /// Starts execution of a macro.
    /// </summary>
    /// <param name="macro">The macro to execute.</param>
    public async Task StartMacro(IMacro macro, TriggerEventArgs? triggerArgs = null)
    {
        try
        {
            // Check if the macro is already running
            if (IsMacroActuallyRunning(macro.Id))
                throw new InvalidOperationException($"Macro {macro.Id} is already running");

            // Force cleanup of any existing state for this macro
            ForceCleanupMacro(macro.Id);

            // Give a small delay to ensure cleanup is complete
            await Task.Delay(100);

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
                    var state = new MacroExecutionState(macro);
                    _macroStates.TryAdd(macro.Id, state);
                    state.ExecutionTask = engine.StartMacro(macro, state.CancellationSource.Token, triggerArgs);
                }
                catch
                {
                    ForceCleanupMacro(macro.Id);
                    throw;
                }
            }
            else
                throw new InvalidOperationException($"Macro {macro.Id} is already running");
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Failed to start macro {macro.Id}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Pauses execution of a macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to pause.</param>
    public async Task PauseMacro(string macroId)
    {
        if (_enginesByMacroId.TryGetValue(macroId, out var engine) && _macroStates.TryGetValue(macroId, out var state))
        {
            state.PauseEvent.Reset();
            state.State = MacroState.Paused;
            await engine.PauseMacro(macroId);
        }
    }

    /// <summary>
    /// Resumes execution of a paused macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to resume.</param>
    public async Task ResumeMacro(string macroId)
    {
        if (_enginesByMacroId.TryGetValue(macroId, out var engine) && _macroStates.TryGetValue(macroId, out var state))
        {
            state.PauseEvent.Set();
            state.State = MacroState.Running;
            await engine.ResumeMacro(macroId);
        }
    }

    /// <summary>
    /// Stops execution of a macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to stop.</param>
    public async Task StopMacro(string macroId)
    {
        if (_enginesByMacroId.TryGetValue(macroId, out var engine) && _macroStates.TryGetValue(macroId, out var state))
        {
            state.CancellationSource.Cancel();
            state.State = MacroState.Completed;
            await engine.StopMacro(macroId);
            ForceCleanupMacro(macroId);
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
        public Task? ExecutionTask { get; set; }

        public void Dispose()
        {
            CancellationSource.Dispose();
            PauseEvent.Dispose();
        }
    }

    private void OnEngineStateChanged(object? sender, MacroStateChangedEventArgs e)
    {
        PluginLog.Debug($"Macro state changed for {e.MacroId}: {e.NewState}");

        if (_macroStates.TryGetValue(e.MacroId, out var state))
            state.State = e.NewState;
        else if (e.NewState is MacroState.Running)
            // If we don't have a state but the macro is running, something went wrong
            PluginLog.Warning($"Received running state for macro {e.MacroId} but no state exists");

        if (e.NewState is MacroState.Completed or MacroState.Error)
        {
            ForceCleanupMacro(e.MacroId);
        }

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

        Svc.Framework.Update -= OnFrameworkUpdate;
        Svc.Condition.ConditionChange -= OnConditionChange;
        Svc.ClientState.TerritoryChanged -= OnTerritoryChanged;
        Svc.Chat.ChatMessage -= OnChatMessage;
        Svc.ClientState.Login -= OnLogin;
        Svc.ClientState.Logout -= OnLogout;
        Svc.AddonLifecycle.UnregisterListener(OnAddonEvent);

        _nativeEngine.Dispose();
        _luaEngine.Dispose();
        _enginesByMacroId.Clear();
        _arApis.Values.ForEach(a => a.Dispose());
        _arApis.Clear();
        _addonEvents.Clear();

        _isDisposed = true;
    }
}
