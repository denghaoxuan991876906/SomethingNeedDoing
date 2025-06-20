using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.Core.Interfaces;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Managers;

/// <summary>
/// Represents a function that can be triggered by an event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TriggerFunction"/> class.
/// </remarks>
/// <param name="macro">The macro containing the function.</param>
/// <param name="functionName">The name of the function.</param>
/// <param name="eventType">The trigger event this function handles.</param>
public class TriggerFunction(IMacro macro, string functionName, TriggerEvent eventType)
{
    /// <summary>
    /// Gets the macro containing this function.
    /// </summary>
    public IMacro Macro { get; } = macro;

    /// <summary>
    /// Gets the name of the function.
    /// </summary>
    public string FunctionName { get; } = functionName;

    /// <summary>
    /// Gets the trigger event this function handles.
    /// </summary>
    public TriggerEvent EventType { get; } = eventType;

    /// <summary>
    /// Gets the addon name for OnAddonEvent triggers.
    /// </summary>
    public string? AddonName { get; } = eventType == TriggerEvent.OnAddonEvent && functionName.StartsWith("OnAddonEvent_")
        ? functionName.Split('_')[1]
        : null;

    /// <summary>
    /// Gets the event type for OnAddonEvent triggers.
    /// </summary>
    public string? AddonEventType { get; } = eventType == TriggerEvent.OnAddonEvent && functionName.StartsWith("OnAddonEvent_")
        ? functionName.Split('_')[2]
        : null;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is TriggerFunction other && Macro.Id == other.Macro.Id && FunctionName == other.FunctionName;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Macro.Id, FunctionName);
}

/// <summary>
/// Manages trigger events for macros.
/// </summary>
public class TriggerEventManager : IDisposable
{
    private readonly Dictionary<TriggerEvent, List<TriggerFunction>> _eventHandlers = [];

    /// <summary>
    /// Event raised when a trigger event occurs.
    /// </summary>
    public event EventHandler<TriggerEventArgs>? TriggerEventOccurred;

    /// <summary>
    /// Registers a macro to handle a specific trigger event.
    /// </summary>
    /// <param name="macro">The macro to register.</param>
    /// <param name="eventType">The type of event to register for.</param>
    public void RegisterTrigger(IMacro macro, TriggerEvent eventType)
    {
        if (!_eventHandlers.ContainsKey(eventType))
            _eventHandlers[eventType] = [];

        var triggerFunction = new TriggerFunction(macro, string.Empty, eventType);
        if (!_eventHandlers[eventType].Contains(triggerFunction))
            _eventHandlers[eventType].Add(triggerFunction);
    }

    /// <summary>
    /// Registers a function within a macro to handle a trigger event based on its name.
    /// </summary>
    /// <param name="macro">The macro containing the function.</param>
    /// <param name="functionName">The name of the function to register.</param>
    public void RegisterFunctionTrigger(IMacro macro, string functionName)
    {
        // Check for OnAddonEvent function name pattern
        if (functionName.StartsWith("OnAddonEvent_"))
        {
            var parts = functionName.Split('_');
            if (parts.Length == 3) // OnAddonEvent_AddonName_EventType
            {
                if (!_eventHandlers.ContainsKey(TriggerEvent.OnAddonEvent))
                    _eventHandlers[TriggerEvent.OnAddonEvent] = [];

                var triggerFunction = new TriggerFunction(macro, functionName, TriggerEvent.OnAddonEvent);
                if (_eventHandlers[TriggerEvent.OnAddonEvent].Contains(triggerFunction))
                {
                    Svc.Log.Debug($"[{nameof(TriggerEventManager)}] Function trigger already registered for macro {macro.Name} function {functionName} (Addon: {parts[1]}, Event: {parts[2]})");
                    return;
                }
                Svc.Log.Debug($"[{nameof(TriggerEventManager)}] Registering OnAddonEvent trigger for macro {macro.Name} function {functionName} (Addon: {parts[1]}, Event: {parts[2]})");
                _eventHandlers[TriggerEvent.OnAddonEvent].Add(triggerFunction);
                return;
            }
        }

        // Check if the function name starts with any trigger event name
        foreach (var eventType in Enum.GetValues<TriggerEvent>())
        {
            if (eventType == TriggerEvent.None) continue;

            if (functionName.StartsWith(eventType.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (!_eventHandlers.ContainsKey(eventType))
                    _eventHandlers[eventType] = [];

                var triggerFunction = new TriggerFunction(macro, functionName, eventType);
                if (_eventHandlers[eventType].Contains(triggerFunction))
                {
                    Svc.Log.Debug($"[{nameof(TriggerEventManager)}] Function trigger already registered for macro {macro.Name} function {functionName} event {eventType}");
                    return;
                }
                Svc.Log.Debug($"[{nameof(TriggerEventManager)}] Registering trigger event {eventType} for macro {macro.Name} function {functionName}");
                _eventHandlers[eventType].Add(triggerFunction);
                return;
            }
        }
    }

    /// <summary>
    /// Unregisters a macro from handling a specific trigger event.
    /// </summary>
    /// <param name="macro">The macro to unregister.</param>
    /// <param name="eventType">The type of event to unregister from.</param>
    public void UnregisterTrigger(IMacro macro, TriggerEvent eventType)
    {
        if (_eventHandlers.TryGetValue(eventType, out var value))
            value.RemoveAll(tf => tf.Macro.Id == macro.Id && string.IsNullOrEmpty(tf.FunctionName));
    }

    /// <summary>
    /// Unregisters a function within a macro from handling its trigger event.
    /// </summary>
    /// <param name="macro">The macro containing the function.</param>
    /// <param name="functionName">The name of the function to unregister.</param>
    public void UnregisterFunctionTrigger(IMacro macro, string functionName)
    {
        // Check for OnAddonEvent function name pattern
        if (functionName.StartsWith("OnAddonEvent_"))
        {
            var parts = functionName.Split('_');
            if (parts.Length == 3) // OnAddonEvent_AddonName_EventType
            {
                if (_eventHandlers.TryGetValue(TriggerEvent.OnAddonEvent, out var value))
                {
                    var removed = value.RemoveAll(tf => tf.Macro.Id == macro.Id && tf.FunctionName == functionName);
                    if (removed > 0)
                        Svc.Log.Debug($"[{nameof(TriggerEventManager)}] Unregistering OnAddonEvent trigger for macro {macro.Name} function {functionName}");
                }
                return;
            }
        }

        // Check if the function name starts with any trigger event name
        foreach (var eventType in Enum.GetValues<TriggerEvent>())
        {
            if (eventType == TriggerEvent.None) continue;

            if (functionName.StartsWith(eventType.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (_eventHandlers.TryGetValue(eventType, out var value))
                {
                    var removed = value.RemoveAll(tf => tf.Macro.Id == macro.Id && tf.FunctionName == functionName);
                    if (removed > 0)
                        Svc.Log.Debug($"[{nameof(TriggerEventManager)}] Unregistering trigger event {eventType} for macro {macro.Name} function {functionName}");
                }
                return;
            }
        }
    }

    /// <summary>
    /// Unregisters a macro from all trigger events.
    /// </summary>
    /// <param name="macro">The macro to unregister.</param>
    public void UnregisterAllTriggers(IMacro macro)
    {
        foreach (var handlers in _eventHandlers.Values)
            handlers.RemoveAll(tf => tf.Macro.Id == macro.Id);
    }

    /// <summary>
    /// Raises a trigger event to all registered macros and functions.
    /// </summary>
    /// <param name="eventType">The type of event to raise.</param>
    /// <param name="data">Optional data associated with the event.</param>
    public async Task RaiseTriggerEvent(TriggerEvent eventType, object? data = null)
    {
        if (!_eventHandlers.TryGetValue(eventType, out var handlers))
            return;

        Svc.Log.Verbose($"[{nameof(TriggerEventManager)}] Handlers for {eventType}: {string.Join(", ", handlers.Select(h => $"{h.Macro.Name}:{h.FunctionName}"))}");

        var args = new TriggerEventArgs(eventType, data);
        foreach (var triggerFunction in handlers.ToList())
        {
            try
            {
                // For OnAddonEvent, check if the addon name and event type match
                if (eventType == TriggerEvent.OnAddonEvent && data is { } eventData)
                {
                    var addonName = eventData.GetType().GetProperty("AddonName")?.GetValue(eventData) as string;
                    var addonEventType = eventData.GetType().GetProperty("EventType")?.GetValue(eventData) as string;

                    if (addonName != triggerFunction.AddonName || addonEventType != triggerFunction.AddonEventType)
                        continue;
                }

                if (string.IsNullOrEmpty(triggerFunction.FunctionName))
                {
                    // Macro-level trigger: raise the event for the entire macro
                    Svc.Log.Verbose($"Raising trigger event {eventType} for macro {triggerFunction.Macro.Name}");
                    TriggerEventOccurred?.Invoke(triggerFunction.Macro, args);
                }
                else
                {
                    string functionContent;
                    if (triggerFunction.Macro.Type == MacroType.Lua)
                    {
                        Svc.Log.Verbose($"Looking for function {triggerFunction.FunctionName} in macro {triggerFunction.Macro.Name}");

                        // get only the function body between 'function ...' and the matching 'end', nothing after
                        var match = Regex.Match(triggerFunction.Macro.Content, $@"function\s+{triggerFunction.FunctionName}\s*\([^)]*\)\s*\n(.*?)\n\s*end", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        if (!match.Success)
                        {
                            Svc.Log.Error($"Could not find function {triggerFunction.FunctionName} in macro {triggerFunction.Macro.Name}");
                            continue;
                        }
                        var functionBody = match.Groups[1].Value.Trim();
                        functionContent = functionBody;
                    }
                    else
                        functionContent = triggerFunction.Macro.Content; // natives just use the whole macro

                    // Create a temporary macro with the parent macro's ID
                    var tempMacroId = $"{triggerFunction.Macro.Id}_{triggerFunction.FunctionName}_{Guid.NewGuid()}";
                    var tempMacro = new TemporaryMacro(functionContent, tempMacroId)
                    {
                        Name = $"{triggerFunction.Macro.Name} - {triggerFunction.FunctionName}",
                        Type = triggerFunction.Macro.Type
                    };
                    Svc.Log.Verbose($"Created temporary macro {tempMacro.Id} for function {triggerFunction.FunctionName} in macro {triggerFunction.Macro.Name}");
                    TriggerEventOccurred?.Invoke(tempMacro, args);
                }
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"Error handling trigger event {eventType} for macro {triggerFunction.Macro.Name} function {triggerFunction.FunctionName}: {ex}");
            }
        }
    }

    /// <summary>
    /// Disposes of the trigger event manager.
    /// </summary>
    public void Dispose() => _eventHandlers.Clear();
}
