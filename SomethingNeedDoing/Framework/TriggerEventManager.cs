using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;

/// <summary>
/// Manages trigger events for macros.
/// </summary>
public class TriggerEventManager : IDisposable
{
    private readonly Dictionary<TriggerEvent, List<IMacro>> _eventHandlers = [];

    /// <summary>
    /// Registers a macro to handle a specific trigger event.
    /// </summary>
    /// <param name="macro">The macro to register.</param>
    /// <param name="eventType">The type of event to register for.</param>
    public void RegisterTrigger(IMacro macro, TriggerEvent eventType)
    {
        if (!_eventHandlers.ContainsKey(eventType))
            _eventHandlers[eventType] = [];

        if (!_eventHandlers[eventType].Contains(macro))
            _eventHandlers[eventType].Add(macro);
    }

    /// <summary>
    /// Unregisters a macro from handling a specific trigger event.
    /// </summary>
    /// <param name="macro">The macro to unregister.</param>
    /// <param name="eventType">The type of event to unregister from.</param>
    public void UnregisterTrigger(IMacro macro, TriggerEvent eventType)
    {
        if (_eventHandlers.ContainsKey(eventType))
            _eventHandlers[eventType].Remove(macro);
    }

    /// <summary>
    /// Unregisters a macro from all trigger events.
    /// </summary>
    /// <param name="macro">The macro to unregister.</param>
    public void UnregisterAllTriggers(IMacro macro)
    {
        foreach (var handlers in _eventHandlers.Values)
            handlers.Remove(macro);
    }

    /// <summary>
    /// Raises a trigger event to all registered macros.
    /// </summary>
    /// <param name="eventType">The type of event to raise.</param>
    /// <param name="data">Optional data associated with the event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RaiseTriggerEvent(TriggerEvent eventType, object? data = null)
    {
        var args = new TriggerEventArgs(eventType, data);
        if (!_eventHandlers.TryGetValue(eventType, out var handlers))
            return;

        foreach (var macro in handlers)
        {
            try
            {
                await macro.HandleTriggerEvent(args);
            }
            catch (Exception ex)
            {
                // Log the error but continue processing other macros
                // TODO: Add proper logging
                Svc.Log.Error($"Error handling trigger event {eventType} for macro {macro.Name}: {ex}");
            }
        }
    }

    /// <summary>
    /// Disposes of the trigger event manager.
    /// </summary>
    public void Dispose()
    {
        _eventHandlers.Clear();
    }
}
