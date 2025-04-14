namespace SomethingNeedDoing.Framework;
/// <summary>
/// Base interface for all macro command modifiers.
/// </summary>
public interface IMacroModifier
{
    /// <summary>
    /// Gets the original text of the modifier.
    /// </summary>
    string ModifierText { get; }
}

/// <summary>
/// Base class for all macro modifiers.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MacroModifierBase"/> class.
/// </remarks>
public abstract class MacroModifierBase(string text) : IMacroModifier
{
    /// <inheritdoc/>
    public string ModifierText { get; } = text;
}
