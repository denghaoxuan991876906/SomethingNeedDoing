namespace SomethingNeedDoing.Attributes;
/// <summary>
/// Marks a property or method of a wrapper class as exposed to Lua and provides documentation. Does not register as a standalone property/method to the engine. see <see cref="LuaFunctionAttribute"/>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class LuaDocsAttribute(string? name = null, string? description = null, string[]? parameterDescriptions = null, string[]? examples = null) : Attribute
{
    /// <summary>
    /// Gets the Lua name of the wrapper member. If null, uses the property/method name.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Gets the description of the wrapper member.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Gets descriptions for each parameter.
    /// </summary>
    public string[]? ParameterDescriptions { get; } = parameterDescriptions;

    /// <summary>
    /// Gets examples of using the wrapper member.
    /// </summary>
    public string[]? Examples { get; } = examples;
}
