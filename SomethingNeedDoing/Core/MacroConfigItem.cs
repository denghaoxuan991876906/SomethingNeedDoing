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

    public bool IsValueDefault()
    {
        if (Value == null && DefaultValue == null)
            return true;
        if (Value == null || DefaultValue == null)
            return false;
        return Type.ToLower() switch
        {
            "int" => Convert.ToInt32(Value) == Convert.ToInt32(DefaultValue),
            "float" or "double" => Math.Abs(Convert.ToSingle(Value) - Convert.ToSingle(DefaultValue)) < 0.0001f,
            "bool" or "boolean" => Convert.ToBoolean(Value) == Convert.ToBoolean(DefaultValue),
            _ => string.Equals(Value.ToString() ?? "", DefaultValue.ToString() ?? "", StringComparison.Ordinal),
        };
    }
}
