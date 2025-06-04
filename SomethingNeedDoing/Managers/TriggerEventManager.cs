using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.Core.Interfaces;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Scheduler;

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

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not TriggerFunction other)
            return false;
        return Macro.Id == other.Macro.Id && FunctionName == other.FunctionName;
    }

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
        // Try to parse the function name as a trigger event
        if (Enum.TryParse<TriggerEvent>(functionName, true, out var eventType))
        {
            if (!_eventHandlers.ContainsKey(eventType))
                _eventHandlers[eventType] = [];

            var triggerFunction = new TriggerFunction(macro, functionName, eventType);
            if (!_eventHandlers[eventType].Contains(triggerFunction))
            {
                Svc.Log.Debug($"Registering trigger event {eventType} for macro {macro.Name} function {functionName}");
                _eventHandlers[eventType].Add(triggerFunction);
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
        // Try to parse the function name as a trigger event
        if (Enum.TryParse<TriggerEvent>(functionName, true, out var eventType))
        {
            if (_eventHandlers.TryGetValue(eventType, out var value))
                value.RemoveAll(tf => tf.Macro.Id == macro.Id && tf.FunctionName == functionName);
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
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RaiseTriggerEvent(TriggerEvent eventType, object? data = null)
    {
        var args = new TriggerEventArgs(eventType, data);
        if (!_eventHandlers.TryGetValue(eventType, out var handlers))
            return;

        foreach (var triggerFunction in handlers)
        {
            try
            {
                TriggerEventOccurred?.Invoke(this, args);
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
