namespace SomethingNeedDoing.Framework;

/// <summary>
/// Arguments passed to macros when triggered by events.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TriggerEventArgs"/> class.
/// </remarks>
/// <param name="eventType">The type of event that triggered the macro.</param>
/// <param name="data">The data associated with the event.</param>
public class TriggerEventArgs(TriggerEvent eventType, object? data = null)
{
    /// <summary>
    /// Gets the type of event that triggered the macro.
    /// </summary>
    public TriggerEvent EventType { get; } = eventType;

    /// <summary>
    /// Gets the data associated with the event.
    /// </summary>
    public object? Data { get; } = data;

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.Now;

    /// <summary>
    /// Gets the data associated with the event as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the data to.</typeparam>
    /// <returns>The data cast to the specified type, or null if the cast fails.</returns>
    public T? GetData<T>() => Data is T value ? value : default;
}
