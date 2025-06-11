namespace SomethingNeedDoing.Attributes;

/// <summary>
/// Attribute for documenting macro commands and modifiers.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GenericDocAttribute"/> class.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class GenericDocAttribute(string description, string[] parameters, string[] examples) : Attribute
{
    /// <summary>
    /// Gets the description
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// Gets the parameters
    /// </summary>
    public string[] Parameters { get; } = parameters;

    /// <summary>
    /// Gets example usages
    /// </summary>
    public string[] Examples { get; } = examples;
}
