using AutoRetainerAPI;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.LuaMacro;
using SomethingNeedDoing.LuaMacro.Wrappers;
using SomethingNeedDoing.NativeMacro;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Managers;
/// <summary>
/// Manages and coordinates execution of multiple macros.
/// </summary>
public class MacroScheduler : IMacroScheduler, IDisposable
{
    private readonly ConcurrentDictionary<string, IMacroEngine> _enginesByMacroId = [];
    private readonly ConcurrentDictionary<string, MacroExecutionState> _macroStates = [];
    private readonly Dictionary<string, AutoRetainerApi> _arApis = [];
    private readonly Dictionary<string, AddonEventConfig> _addonEvents = [];
    private readonly MacroHierarchyManager _macroHierarchy = new();
    private readonly Dictionary<string, IDisableable> _disableablePlugins = [];
    private readonly CleanupManager _cleanupManager = new();

    private readonly NativeMacroEngine _nativeEngine;
    private readonly NLuaMacroEngine _luaEngine;
    private readonly TriggerEventManager _triggerEventManager;

    private readonly HashSet<string> _functionTriggersRegistered = [];

    /// <inheritdoc/>
    public event EventHandler<MacroStateChangedEventArgs>? MacroStateChanged;

    /// <summary>
    /// Event raised when any macro encounters an error.
    /// </summary>
    public event EventHandler<MacroErrorEventArgs>? MacroError;

    public MacroScheduler(NativeMacroEngine nativeEngine, NLuaMacroEngine luaEngine, TriggerEventManager triggerEventManager, IEnumerable<IDisableable> disableablePlugins)
    {
        _nativeEngine = nativeEngine;
        _luaEngine = luaEngine;
        _triggerEventManager = triggerEventManager;

        _nativeEngine.Scheduler = this; // TODO: find a way around this
        _luaEngine.Scheduler = this;

        _nativeEngine.MacroError += OnEngineError;
        _luaEngine.MacroError += OnEngineError;
        _triggerEventManager.TriggerEventOccurred += OnTriggerEventOccurred;

        _nativeEngine.MacroControlRequested += OnMacroControlRequested;
        _luaEngine.MacroControlRequested += OnMacroControlRequested;
        _nativeEngine.MacroStepCompleted += OnMacroStepCompleted;
        _luaEngine.MacroStepCompleted += OnMacroStepCompleted;

        _cleanupManager.CleanupFunctionRequested += OnCleanupFunctionRequested;

        foreach (var plugin in disableablePlugins)
            _disableablePlugins[plugin.InternalName] = plugin;

        SubscribeToTriggerEvents();
    }

    /// <inheritdoc/>
    public IEnumerable<IMacro> GetMacros() => _macroStates.Values.Select(s => s.Macro);

    /// <inheritdoc/>
    public MacroState GetMacroState(string macroId) => _macroStates.TryGetValue(macroId, out var state) ? state.Macro.State : MacroState.Unknown;

    /// <inheritdoc/>
    public void SubscribeToTriggerEvent(IMacro macro, TriggerEvent triggerEvent)
    {
        ArgumentNullException.ThrowIfNull(macro);

        switch (triggerEvent)
        {
            case TriggerEvent.OnAutoRetainerCharacterPostProcess:
                if (!_arApis.ContainsKey(macro.Id))
                {
                    _arApis.TryAdd(macro.Id, new AutoRetainerApi());
                    _arApis[macro.Id].OnCharacterPostprocessStep += () => CheckCharacterPostProcess(macro);
                    _arApis[macro.Id].OnCharacterReadyToPostProcess += () => DoCharacterPostProcess(macro);
                }
                _triggerEventManager.RegisterTrigger(macro, triggerEvent);
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
            default:
                // For all other events, we just need to register with the trigger event manager
                _triggerEventManager.RegisterTrigger(macro, triggerEvent);
                break;
        }
    }

    /// <inheritdoc/>
    public void UnsubscribeFromTriggerEvent(IMacro macro, TriggerEvent triggerEvent)
    {
        ArgumentNullException.ThrowIfNull(macro);

        switch (triggerEvent)
        {
            case TriggerEvent.OnAutoRetainerCharacterPostProcess:
                if (_arApis.TryGetValue(macro.Id, out var arApi))
                {
                    arApi.OnCharacterPostprocessStep -= () => CheckCharacterPostProcess(macro);
                    arApi.OnCharacterReadyToPostProcess -= () => DoCharacterPostProcess(macro);
                    arApi.Dispose();
                    _arApis.Remove(macro.Id);
                }
                _triggerEventManager.UnregisterTrigger(macro, triggerEvent);
                break;
            case TriggerEvent.OnAddonEvent:
                if (_addonEvents.TryGetValue(macro.Id, out var cfg))
                {
                    Svc.AddonLifecycle.UnregisterListener(cfg.EventType, cfg.AddonName, OnAddonEvent);
                    _addonEvents.Remove(macro.Id);
                }
                break;
            default:
                // For all other events, we just need to unregister from the trigger event manager
                _triggerEventManager.UnregisterTrigger(macro, triggerEvent);
                break;
        }
    }

    #region Controls
    public async Task StartMacro(IMacro macro) => await StartMacro(macro, null);

    /// <summary>
    /// Registers function-level triggers for a macro.
    /// </summary>
    /// <param name="macro">The macro to register function triggers for.</param>
    private void RegisterFunctionTriggers(IMacro macro)
    {
        if (macro is TemporaryMacro) return;
        if (_functionTriggersRegistered.Contains(macro.Id)) return;
        if (macro.Type == MacroType.Lua)
        {
            // match "function OnEventName()"
            var matches = Regex.Matches(macro.Content, @"function\s+(\w+)\s*\(");
            foreach (Match match in matches)
            {
                var functionName = match.Groups[1].Value;
                _triggerEventManager.RegisterFunctionTrigger(macro, functionName);
            }
        }
        else
        {
            // match "/OnEventName"
            var matches = Regex.Matches(macro.Content, @"^/\s*(\w+)", RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                var functionName = match.Groups[1].Value;
                _triggerEventManager.RegisterFunctionTrigger(macro, functionName);
            }
        }
        _functionTriggersRegistered.Add(macro.Id);
        _cleanupManager.RegisterCleanupFunctions(macro);
    }

    /// <summary>
    /// Unregisters function-level triggers for a macro.
    /// </summary>
    /// <param name="macro">The macro to unregister function triggers for.</param>
    private void UnregisterFunctionTriggers(IMacro macro)
    {
        if (macro is TemporaryMacro) return;
        if (!_functionTriggersRegistered.Contains(macro.Id)) return;
        if (macro.Type == MacroType.Lua)
        {
            // match "function OnEventName()"
            var matches = Regex.Matches(macro.Content, @"function\s+(\w+)\s*\(");
            foreach (Match match in matches)
            {
                var functionName = match.Groups[1].Value;
                _triggerEventManager.UnregisterFunctionTrigger(macro, functionName);
            }
        }
        else
        {
            // match "/OnEventName"
            var matches = Regex.Matches(macro.Content, @"^/\s*(\w+)", RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                var functionName = match.Groups[1].Value;
                _triggerEventManager.UnregisterFunctionTrigger(macro, functionName);
            }
        }
        _functionTriggersRegistered.Remove(macro.Id);
        _cleanupManager.UnregisterCleanupFunctions(macro);
    }

    /// <inheritdoc/>
    public Task StartMacro(IMacro macro, int loopCount) => StartMacro(macro, loopCount, null);

    /// <inheritdoc/>
    public Task StartMacro(IMacro macro, TriggerEventArgs? triggerArgs = null) => StartMacro(macro, 0, triggerArgs);

    /// <inheritdoc/>
    public async Task StartMacro(IMacro macro, int loopCount, TriggerEventArgs? triggerArgs = null)
    {
        if (_macroStates.ContainsKey(macro.Id))
        {
            Svc.Log.Warning($"Macro {macro.Name} is already running.");
            return;
        }

        if (MissingRequiredPlugins(macro, out var missingPlugins))
        {
            Svc.Chat.PrintMessage($"Cannot run {macro.Name}. The following plugins need to be installed: {string.Join(", ", missingPlugins)}");
            return;
        }

        await SetPluginStates(macro, false);

        // Subscribe to state changes before creating the state
        macro.StateChanged += OnMacroStateChanged;
        var state = new MacroExecutionState(macro);

        RegisterFunctionTriggers(macro);
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

                await Svc.Framework.RunOnTick(async () =>
                {
                    try
                    {
                        Svc.Log.Verbose($"Setting macro {macro.Id} state to Running");
                        state.Macro.State = MacroState.Running;
                        await engine.StartMacro(macro, state.CancellationSource.Token, triggerArgs, loopCount);
                    }
                    catch (Exception ex)
                    {
                        Svc.Log.Error(ex, $"Error executing macro {macro.Name}");
                        state.Macro.State = MacroState.Error;
                        await SetPluginStates(macro, true);
                    }
                });
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, $"Error setting up macro {macro.Name}");
                state.Macro.State = MacroState.Error;
                await SetPluginStates(macro, true);
            }
        });

        await state.ExecutionTask;
        Svc.Log.Verbose($"Setting macro {macro.Id} state to Completed");
        state.Macro.State = MacroState.Completed;
        await SetPluginStates(macro, true);
    }

    /// <inheritdoc/>
    public async void PauseMacro(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state))
        {
            state.PauseEvent.Reset();
            state.Macro.State = MacroState.Paused;
            await SetPluginStates(state.Macro, true);

            if (C.PropagateControlsToChildren)
            {
                foreach (var child in _macroHierarchy.GetChildMacros(macroId))
                {
                    if (_macroStates.TryGetValue(child.Id, out var childState))
                    {
                        childState.PauseEvent.Reset();
                        child.State = MacroState.Paused;
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public async void ResumeMacro(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state))
        {
            state.PauseEvent.Set();
            state.Macro.State = MacroState.Running;
            await SetPluginStates(state.Macro, false);

            if (C.PropagateControlsToChildren)
            {
                foreach (var child in _macroHierarchy.GetChildMacros(macroId))
                {
                    if (_macroStates.TryGetValue(child.Id, out var childState))
                    {
                        childState.PauseEvent.Set();
                        child.State = MacroState.Running;
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public async void StopMacro(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state))
        {
            state.CancellationSource.Cancel();
            state.Macro.StateChanged -= OnMacroStateChanged;
            state.Macro.State = MacroState.Completed;

            UnregisterFunctionTriggers(state.Macro);
            await SetPluginStates(state.Macro, true);

            // Execute cleanup functions
            _cleanupManager.ExecuteCleanup(macroId, "Stopped");

            if (C.PropagateControlsToChildren)
                foreach (var child in _macroHierarchy.GetChildMacros(macroId).ToList())
                    StopMacro(child.Id);

            if (_macroStates.TryRemove(macroId, out var removedState))
                removedState.Dispose();
            _enginesByMacroId.TryRemove(macroId, out _);
        }
    }

    /// <summary>
    /// Forces cleanup of a macro's state.
    /// </summary>
    /// <param name="macroId">The ID of the macro to clean up.</param>
    public void CleanupMacro(string macroId)
    {
        if (_macroStates.TryRemove(macroId, out var state))
        {
            if (state.Macro is ConfigMacro configMacro)
                _triggerEventManager.UnregisterAllTriggers(configMacro);

            UnregisterFunctionTriggers(state.Macro);
            state.CancellationSource.Cancel();
            state.CancellationSource.Dispose();
            state.Macro.StateChanged -= OnMacroStateChanged;
        }

        _enginesByMacroId.TryRemove(macroId, out _);
    }

    /// <inheritdoc/>
    public void StopAllMacros() => _enginesByMacroId.Keys.Each(StopMacro);

    /// <inheritdoc/>
    public void CheckLoopPause(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state) && state.PauseAtLoop)
        {
            state.PauseAtLoop = false;
            state.PauseEvent.Reset();
            state.Macro.State = MacroState.Paused;
        }
    }

    /// <inheritdoc/>
    public void CheckLoopStop(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state) && state.StopAtLoop)
        {
            state.StopAtLoop = false;
            state.CancellationSource.Cancel();
            state.Macro.State = MacroState.Completed;
        }
    }

    /// <inheritdoc/>
    public void PauseAtNextLoop(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state))
        {
            state.PauseAtLoop = true;
            state.StopAtLoop = false;
        }
    }

    /// <inheritdoc/>
    public void StopAtNextLoop(string macroId)
    {
        if (_macroStates.TryGetValue(macroId, out var state))
        {
            state.PauseAtLoop = false;
            state.StopAtLoop = true;
        }
    }
    #endregion

    private bool MissingRequiredPlugins(IMacro macro, out List<string> missingPlugins)
    {
        missingPlugins = [.. macro.Metadata.PluginDependecies.Where(dep => !dep.IsNullOrEmpty() && !Svc.PluginInterface.InstalledPlugins.Any(ip => ip.InternalName == dep && ip.IsLoaded))];
        return missingPlugins.Count > 0;
    }

    private async Task SetPluginStates(IMacro macro, bool state)
    {
        foreach (var name in macro.Metadata.PluginsToDisable)
        {
            if (_disableablePlugins.TryGetValue(name, out var plugin))
            {
                if (state)
                {
                    Svc.Log.Info($"[{macro.Name}] Re-enabling plugin {name}");
                    await plugin.EnableAsync();
                }
                else
                {
                    Svc.Log.Info($"[{macro.Name}] Disabling plugin {name}");
                    await plugin.DisableAsync();
                }
            }
            else
                Svc.Log.Warning($"Plugin {name} is not registered as disableable");
        }
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

    private void OnMacroStateChanged(object? sender, MacroStateChangedEventArgs e)
    {
        // If this is a temporary macro, find its parent
        var parts = e.MacroId.Split("_");
        if (parts.Length >= 2 && C.GetMacro(parts[0]) is { } parentMacro && e.NewState == MacroState.Error)
            parentMacro.State = MacroState.Error;

        MacroStateChanged?.Invoke(sender, e);

        if (e.NewState is MacroState.Completed or MacroState.Error)
        {
            if (sender is IMacro macro)
            {
                if (macro.Metadata.TriggerEvents.Contains(TriggerEvent.OnAutoRetainerCharacterPostProcess)) // whole macro
                {
                    if (_arApis.TryGetValue(macro.Id, out var arApi))
                    {
                        Svc.Log.Info($"[{nameof(MacroScheduler)}] {macro.Name} character post process finished, calling FinishCharacterPostProcess()");
                        arApi.FinishCharacterPostProcess();
                    }
                }
                else if (macro is TemporaryMacro && e.MacroId.Contains('_') && parts.Length >= 2) // function-level trigger temp macro
                {
                    var parentId = parts[0];
                    if (C.GetMacro(parentId) is { } parent && parent.Metadata.TriggerEvents.Contains(TriggerEvent.OnAutoRetainerCharacterPostProcess))
                    {
                        if (_arApis.TryGetValue(parentId, out var arApi))
                        {
                            Svc.Log.Info($"[{nameof(MacroScheduler)}] {macro.Name} character post process finished, calling FinishCharacterPostProcess()");
                            arApi.FinishCharacterPostProcess();
                        }
                    }
                }
            }

            _cleanupManager.ExecuteCleanup(e.MacroId, e.NewState.ToString());

            // If this is a temporary macro, unregister it and clean up
            if (parts.Length >= 2 && C.GetMacro(parts[0]) is { } parentMacro2)
            {
                _macroHierarchy.UnregisterTemporaryMacro(e.MacroId);
                if (sender is IMacro tempMacro)
                    tempMacro.StateChanged -= OnMacroStateChanged;
            }

            StopMacro(e.MacroId); // handle local cancellations/state
            CleanupMacro(e.MacroId); // cleanup triggers/state
        }
    }

    #region Triggers
    private void OnTriggerEventOccurred(object? sender, TriggerEventArgs e)
    {
        if (sender is IMacro macro)
        {
            // If this is a temporary macro created from a function trigger, register it with its parent
            if (macro is TemporaryMacro && macro.Id.Contains('_'))
            {
                var parts = macro.Id.Split('_');
                Svc.Log.Verbose($"[{nameof(MacroScheduler)}] Processing temporary macro {macro.Id} with parts: {string.Join(", ", parts)}");
                if (parts.Length >= 2 && C.GetMacro(parts[0]) is { } parentMacro)
                {
                    Svc.Log.Verbose($"[{nameof(MacroScheduler)}] Found parent macro {parentMacro.Id} for temporary macro {macro.Id}");
                    _macroHierarchy.RegisterTemporaryMacro(parentMacro, macro);

                    Svc.Log.Verbose($"[{nameof(MacroScheduler)}] Subscribing to state changes for temporary macro {macro.Id}");
                    macro.StateChanged += OnMacroStateChanged;

                    _ = StartMacro(macro, e);
                }
                else
                    Svc.Log.Warning($"[{nameof(MacroScheduler)}] Could not find parent macro {parts[0]} for temporary macro {macro.Id}");
            }
            else
            {
                // don't let an infinte loop of it starting itself happen
                if (macro.State == MacroState.Running)
                {
                    Svc.Log.Verbose($"[{nameof(MacroScheduler)}] Macro {macro.Id} is already running, skipping trigger");
                    return;
                }
                _ = StartMacro(macro, e);
            }
        }
    }

    private void SubscribeToTriggerEvents()
    {
        foreach (var macro in C.Macros)
            foreach (var triggerEvent in macro.Metadata.TriggerEvents)
                SubscribeToTriggerEvent(macro, triggerEvent);

        Svc.Framework.Update += OnFrameworkUpdate;
        Svc.Condition.ConditionChange += OnConditionChange;
        Svc.ClientState.TerritoryChanged += OnTerritoryChanged;
        Svc.Chat.ChatMessage += OnChatMessage;
        Svc.ClientState.Login += OnLogin;
        Svc.ClientState.Logout += OnLogout;
    }

    private long _combatStart = 0;
    private void OnFrameworkUpdate(IFramework framework)
    {
        if (Svc.Condition[ConditionFlag.InCombat])
        {
            if (_combatStart == 0)
            {
                _combatStart = DateTime.Now.Ticks;
                var startTimestamp = _combatStart;
                var opponents = Svc.Objects.Where(o => o.TargetObjectId == Player.Object.GameObjectId).Select(o => new EntityWrapper(o));
                _ = _triggerEventManager.RaiseTriggerEvent(TriggerEvent.OnCombatStart, new { startTimestamp, opponents });
                Svc.Log.Verbose($"[{nameof(MacroScheduler)}] Combat started against {string.Join(", ", opponents.Select(o => o.Name))} at {startTimestamp}");
            }
        }
        else
        {
            if (_combatStart != 0)
            {
                var endTimestamp = DateTime.Now.Ticks;
                var duration = TimeSpan.FromTicks(endTimestamp - _combatStart).TotalSeconds;
                _combatStart = 0;
                _ = _triggerEventManager.RaiseTriggerEvent(TriggerEvent.OnCombatEnd, new { endTimestamp, duration });
                Svc.Log.Verbose($"[{nameof(MacroScheduler)}] Combat ended at {endTimestamp} in {duration:F2} seconds");
            }
        }

        _ = _triggerEventManager.RaiseTriggerEvent(TriggerEvent.OnUpdate);
    }

    private void OnAddonEvent(AddonEvent type, AddonArgs args)
    {
        var eventData = new Dictionary<string, object> { { "type", type }, { "args", args } };
        Svc.Log.Verbose($"[{nameof(MacroScheduler)}] [{nameof(OnAddonEvent)}] fired [{type}, {args}]");
        _ = _triggerEventManager.RaiseTriggerEvent(TriggerEvent.OnAddonEvent, eventData);
    }

    private void OnConditionChange(ConditionFlag flag, bool value)
    {
        var eventData = new Dictionary<string, object> { { "flag", flag }, { "value", value } };
        Svc.Log.Verbose($"[{nameof(MacroScheduler)}] [{nameof(OnConditionChange)}] fired [{flag}, {value}]");
        _ = _triggerEventManager.RaiseTriggerEvent(TriggerEvent.OnConditionChange, eventData);
    }

    private void OnTerritoryChanged(ushort territoryType)
    {
        var eventData = new Dictionary<string, object> { { "territoryType", territoryType } };
        Svc.Log.Verbose($"[{nameof(MacroScheduler)}] [{nameof(OnTerritoryChanged)}] fired [{territoryType}]");
        _ = _triggerEventManager.RaiseTriggerEvent(TriggerEvent.OnTerritoryChange, eventData);
    }

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var eventData = new Dictionary<string, object> { { "type", type }, { "timestamp", timestamp }, { "sender", sender }, { "message", message }, { "isHandled", isHandled } };
        Svc.Log.Verbose($"[{nameof(MacroScheduler)}] [{nameof(OnChatMessage)}] fired [{type}, {timestamp}, {sender}, {message}, {isHandled}]");
        _ = _triggerEventManager.RaiseTriggerEvent(TriggerEvent.OnChatMessage, eventData);
    }

    private void OnLogin()
    {
        Svc.Log.Verbose($"[{nameof(MacroScheduler)}] [{nameof(OnLogin)}] fired");
        _ = _triggerEventManager.RaiseTriggerEvent(TriggerEvent.OnLogin);
    }

    private void OnLogout(int type, int code)
    {
        var eventData = new Dictionary<string, object> { { "type", type }, { "code", code } };
        Svc.Log.Verbose($"[{nameof(MacroScheduler)}] [{nameof(OnLogout)}] fired [{type}, {code}]");
        _ = _triggerEventManager.RaiseTriggerEvent(TriggerEvent.OnLogout, eventData);
    }

    private void CheckCharacterPostProcess(IMacro macro)
    {
        if (C.ARCharacterPostProcessExcludedCharacters.Any(x => x == Svc.ClientState.LocalContentId))
            Svc.Log.Info($"Skipping post process macro {macro.Name} for current character.");
        else
            _arApis[macro.Id].RequestCharacterPostprocess();
    }

    private void DoCharacterPostProcess(IMacro macro)
    {
        if (C.ARCharacterPostProcessExcludedCharacters.Any(x => x == Svc.ClientState.LocalContentId))
        {
            Svc.Log.Info($"Skipping post process macro {macro.Name} for current character.");
            return;
        }

        Svc.Log.Info($"Executing post process macro {macro.Name} for current character.");
        var eventData = new Dictionary<string, object> { { "Id", Svc.ClientState.LocalContentId }, { "Name", Svc.ClientState.LocalPlayer?.Name.TextValue ?? string.Empty } };
        _ = _triggerEventManager.RaiseTriggerEvent(TriggerEvent.OnAutoRetainerCharacterPostProcess, eventData);
    }

    private void OnMacroControlRequested(object? sender, MacroControlEventArgs e)
    {
        Svc.Log.Verbose($"[{nameof(MacroScheduler)}] Received MacroControlRequested event for macro {e.MacroId} with control type {e.ControlType}");

        if (e.ControlType == MacroControlType.Start)
        {
            if (C.GetMacro(e.MacroId) is { } macro)
            {
                Svc.Log.Info($"[{nameof(MacroScheduler)}] Starting macro {e.MacroId}");
                _ = StartMacro(macro);
            }
            else if (sender is IMacroEngine engine && engine.GetTemporaryMacro(e.MacroId) is { } tempMacro)
            {
                Svc.Log.Info($"[{nameof(MacroScheduler)}] Starting temporary macro {e.MacroId}");
                // Find the parent macro by looking at the ID prefix
                var parentId = e.MacroId.Split("_")[0];
                if (C.GetMacro(parentId) is { } parentMacro)
                {
                    // Register the temporary macro with its parent
                    _macroHierarchy.RegisterTemporaryMacro(parentMacro, tempMacro);

                    // Subscribe to state changes for the temporary macro
                    Svc.Log.Verbose($"[{nameof(MacroScheduler)}] Subscribing to state changes for temporary macro {e.MacroId}");
                    tempMacro.StateChanged += OnMacroStateChanged;

                    // Start the temporary macro
                    _ = StartMacro(tempMacro);
                }
                else
                    Svc.Log.Warning($"[{nameof(MacroScheduler)}] Could not find parent macro {parentId} for temporary macro {e.MacroId}");
            }
            else
                Svc.Log.Warning($"[{nameof(MacroScheduler)}] Could not find macro {e.MacroId} to start");
        }
        else if (e.ControlType == MacroControlType.Stop)
            StopMacro(e.MacroId);
    }

    private void OnMacroStepCompleted(object? sender, MacroStepCompletedEventArgs e)
        => Svc.Log.Verbose($"Macro step completed for {e.MacroId}: {e.StepIndex}/{e.TotalSteps}");

    private void OnCleanupFunctionRequested(object? sender, CleanupFunctionEventArgs e)
    {
        Svc.Log.Verbose($"[{nameof(MacroScheduler)}] Executing cleanup function {e.FunctionName} for macro {e.TempMacro.Name} (reason: {e.Reason})");

        // Start the cleanup temporary macro
        _ = StartMacro(e.TempMacro);
    }
    #endregion

    /// <inheritdoc/>
    public void Dispose()
    {
        _nativeEngine.MacroError -= OnEngineError;
        _luaEngine.MacroError -= OnEngineError;
        _triggerEventManager.TriggerEventOccurred -= OnTriggerEventOccurred;

        _nativeEngine.MacroControlRequested -= OnMacroControlRequested;
        _luaEngine.MacroControlRequested -= OnMacroControlRequested;
        _nativeEngine.MacroStepCompleted -= OnMacroStepCompleted;
        _luaEngine.MacroStepCompleted -= OnMacroStepCompleted;

        _cleanupManager.CleanupFunctionRequested -= OnCleanupFunctionRequested;

        _macroStates.Values.Each(s => s.Dispose());
        _macroStates.Clear();
        _enginesByMacroId.Clear();

        Svc.Framework.Update -= OnFrameworkUpdate;
        Svc.Condition.ConditionChange -= OnConditionChange;
        Svc.ClientState.TerritoryChanged -= OnTerritoryChanged;
        Svc.Chat.ChatMessage -= OnChatMessage;
        Svc.ClientState.Login -= OnLogin;
        Svc.ClientState.Logout -= OnLogout;
        Svc.AddonLifecycle.UnregisterListener(OnAddonEvent);

        _nativeEngine.Dispose();
        _luaEngine.Dispose();
        _arApis.Values.Each(a => a.Dispose());
        _arApis.Clear();
        _addonEvents.Clear();

        _triggerEventManager.Dispose();
        _cleanupManager.Dispose();
    }

    /// <inheritdoc/>
    public bool HasCleanupFunctions(string macroId) => _cleanupManager.HasCleanupFunctions(macroId);

    /// <inheritdoc/>
    public void ExecuteCleanup(string macroId, string reason = "Manual") => _cleanupManager.ExecuteCleanup(macroId, reason);
}
