namespace SomethingNeedDoing.Framework;

/// <summary>
/// Represents information about a modifier extracted from command text.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ModifierInfo"/> class.
/// </remarks>
/// <param name="name">The name of the modifier.</param>
/// <param name="parameter">The parameter of the modifier, if any.</param>
/// <param name="originalText">The original text of the modifier.</param>
public class ModifierInfo(string name, string parameter, string originalText)
{
    /// <summary>
    /// Gets or sets the name of the modifier.
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// Gets or sets the parameter of the modifier, if any.
    /// </summary>
    public string Parameter { get; set; } = parameter;

    /// <summary>
    /// Gets or sets the original text of the modifier.
    /// </summary>
    public string OriginalText { get; set; } = originalText;
}
