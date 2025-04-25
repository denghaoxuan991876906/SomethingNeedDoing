using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AutoRetainerAPI;
using ECommons.Logging;
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
    private readonly TriggerEventManager _triggerEventManager;

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
        _nativeEngine.MacroError += OnEngineError;
        _luaEngine.MacroError += OnEngineError;
        _triggerEventManager = new TriggerEventManager();
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
        var eventData = new Dictionary<string, object>
        {
            ["flag"] = flag,
            ["value"] = value
        };

        var args = new TriggerEventArgs(TriggerEvent.OnConditionChange, eventData);
        C.Macros.Where(m => m.Metadata.TriggerEvents.Contains(TriggerEvent.OnConditionChange)).Each(m => m.Start(args));
    }

    private void OnTerritoryChanged(ushort territoryType)
    {
        var args = new TriggerEventArgs(TriggerEvent.OnTerritoryChange, territoryType);
        C.Macros.Where(m => m.Metadata.TriggerEvents.Contains(TriggerEvent.OnTerritoryChange)).Each(m => m.Start(args));
    }

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var eventData = new Dictionary<string, object>
        {
            ["type"] = type,
            ["timestamp"] = timestamp,
            ["sender"] = sender,
            ["message"] = message,
            ["isHandled"] = isHandled
        };

        var args = new TriggerEventArgs(TriggerEvent.OnChatMessage, eventData);
        C.Macros.Where(m => m.Metadata.TriggerEvents.Contains(TriggerEvent.OnChatMessage)).Each(m => m.Start(args));
    }

    private void OnLogin()
        => C.Macros.Where(m => m.Metadata.TriggerEvents.Contains(TriggerEvent.OnLogin)).Each(m => m.Start());

    private void OnLogout(int type, int code)
    {
        var eventData = new Dictionary<string, object>
        {
            ["type"] = type,
            ["code"] = code
        };

        var args = new TriggerEventArgs(TriggerEvent.OnLogout, eventData);
        C.Macros.Where(m => m.Metadata.TriggerEvents.Contains(TriggerEvent.OnLogout)).Each(m => m.Start(args));
    }

    /// <summary>
    /// Gets all currently running macros.
    /// </summary>
    public IEnumerable<IMacro> GetRunningMacros() => _macroStates.Values.Where(s => s.Macro.State is MacroState.Running or MacroState.Paused).Select(s => s.Macro);

    /// <summary>
    /// Gets the current state of a macro.
    /// </summary>
    public MacroState GetMacroState(string macroId) => _macroStates.TryGetValue(macroId, out var state) ? state.Macro.State : MacroState.Ready;

    public async Task StartMacro(IMacro macro) => await StartMacro(macro, null);

    /// <summary>
    /// Forces cleanup of a macro's state.
    /// </summary>
    /// <param name="macroId">The ID of the macro to clean up.</param>
    public void CleanupMacro(string macroId)
    {
        if (_macroStates.TryRemove(macroId, out var state))
        {
            if (state.Macro is ConfigMacro configMacro)
            {
                _triggerEventManager.UnregisterAllTriggers(configMacro);
            }
            state.CancellationSource.Cancel();
            state.CancellationSource.Dispose();
        }

        _enginesByMacroId.TryRemove(macroId, out _);
    }

    /// <summary>
    /// Starts execution of a macro.
    /// </summary>
    /// <param name="macro">The macro to execute.</param>
    public async Task StartMacro(IMacro macro, TriggerEventArgs? triggerArgs = null)
    {
        if (_macroStates.ContainsKey(macro.Id))
        {
            Svc.Log.Warning($"Macro {macro.Name} is already running.");
            return;
        }

        var state = new MacroExecutionState(macro);

        state.ExecutionTask = Task.Run(async () =>
        {
            try
            {
                IMacroEngine engine = state.Macro.Type switch
                {
                    MacroType.Native => _nativeEngine,
                    MacroType.Lua => _luaEngine,
                    _ => throw new NotSupportedException($"Macro type {state.Macro.Type} is not supported.")
                };

                _enginesByMacroId[macro.Id] = engine;
                _macroStates[macro.Id] = state;

                // Set initial state to Running
                state.Macro.State = MacroState.Running;

                await Svc.Framework.RunOnTick(async () =>
                {
                    try
                    {
                        await engine.StartMacro(macro, state.CancellationSource.Token, triggerArgs);
                    }
                    catch (Exception ex)
                    {
                        Svc.Log.Error(ex, $"Error executing macro {macro.Name}");
                        state.Macro.State = MacroState.Error;
                    }
                });
            }
            finally
            {
                if (_macroStates.TryRemove(macro.Id, out var removedState))
                    removedState.Dispose();
                _enginesByMacroId.TryRemove(macro.Id, out _);
            }
        });
    }

    /// <summary>
    /// Pauses execution of a macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to pause.</param>
    public void PauseMacro(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state))
        {
            state.PauseEvent.Reset();
            state.Macro.State = MacroState.Paused;
        }
    }

    /// <summary>
    /// Resumes execution of a paused macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to resume.</param>
    public void ResumeMacro(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state))
        {
            state.PauseEvent.Set();
            state.Macro.State = MacroState.Running;
        }
    }

    /// <summary>
    /// Stops execution of a macro.
    /// </summary>
    /// <param name="macroId">The ID of the macro to stop.</param>
    public void StopMacro(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state))
        {
            state.CancellationSource.Cancel();
            state.Macro.State = MacroState.Completed;
        }
    }

    /// <summary>
    /// Stops all running macros.
    /// </summary>
    public void StopAllMacros()
    {
        _enginesByMacroId.Keys.Each(StopMacro);
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
            state.Macro.State = MacroState.Paused;
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
            state.Macro.State = MacroState.Completed;
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
        _arApis.Values.Each(a => a.Dispose());
        _arApis.Clear();
        _addonEvents.Clear();

        foreach (var state in _macroStates.Values)
        {
            state.CancellationSource.Cancel();
            state.CancellationSource.Dispose();
        }
        _macroStates.Clear();
        _triggerEventManager.Dispose();
    }
}
