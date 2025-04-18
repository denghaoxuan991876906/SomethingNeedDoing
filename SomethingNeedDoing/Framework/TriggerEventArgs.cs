using System;
using System.Collections.Generic;

namespace SomethingNeedDoing.Framework;

/// <summary>
/// Arguments passed to macros when triggered by events.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TriggerEventArgs"/> class.
/// </remarks>
/// <param name="eventType">The type of trigger event.</param>
public class TriggerEventArgs(TriggerEvent eventType) : EventArgs
{
    /// <summary>
    /// Gets the type of trigger event.
    /// </summary>
    public TriggerEvent EventType { get; set; } = eventType;

    /// <summary>
    /// Gets the event-specific data.
    /// </summary>
    public Dictionary<string, object> EventData { get; } = [];

    /// <summary>
    /// Gets or sets the value associated with the event.
    /// </summary>
    public bool? Value { get; set; }

    /// <summary>
    /// Gets or sets the territory type associated with the event.
    /// </summary>
    public ushort? TerritoryType { get; set; }

    /// <summary>
    /// Gets or sets the message associated with the event.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
