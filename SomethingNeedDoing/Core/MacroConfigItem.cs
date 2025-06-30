namespace SomethingNeedDoing.Core;

/// <summary>
/// Represents a configuration item for a macro.
/// </summary>
public class MacroConfigItem
{
    /// <summary>
    /// Gets or sets the current value of the config item.
    /// </summary>
    public object Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default value of the config item.
    /// </summary>
    public object DefaultValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the config item.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the config item (e.g., "string", "int", "bool", "float").
    /// </summary>
    public string Type { get; set; } = "string";

    /// <summary>
    /// Gets or sets the minimum value for numeric types.
    /// </summary>
    public object? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum value for numeric types.
    /// </summary>
    public object? MaxValue { get; set; }

    /// <summary>
    /// Gets or sets whether this config item is required.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the validation pattern for string types (regex).
    /// </summary>
    public string? ValidationPattern { get; set; }

    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public string? ValidationMessage { get; set; }
}
