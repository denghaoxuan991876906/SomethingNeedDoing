namespace SomethingNeedDoing.Attributes;
/// <summary>
/// Marks a method as exposed to Lua and provides documentation.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
public class LuaFunctionAttribute(string? name = null, string? description = null, string[]? parameterDescriptions = null, string[]? examples = null) : Attribute
{
    /// <summary>
    /// Gets the Lua name of the function. If null, uses the method name.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Gets the description of the function.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Gets descriptions for each parameter.
    /// </summary>
    public string[]? ParameterDescriptions { get; } = parameterDescriptions;

    /// <summary>
    /// Gets examples of using the function.
    /// </summary>
    public string[]? Examples { get; } = examples;
}
