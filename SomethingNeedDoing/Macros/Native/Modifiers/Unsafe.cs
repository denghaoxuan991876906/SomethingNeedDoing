using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Macros.Native.Modifiers;

/// <summary>
/// Modifier for specifying unsafe action execution.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UnsafeModifier"/> class.
/// </remarks>
public class UnsafeModifier(string text, bool isUnsafe) : MacroModifierBase(text)
{
    private static readonly Regex Regex = new(@"(?<modifier><unsafe>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets whether unsafe mode is enabled.
    /// </summary>
    public bool IsUnsafe { get; } = isUnsafe;

    /// <inheritdoc/>
    public static bool TryParse(ref string text, out IMacroModifier modifier)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (success)
        {
            var group = match.Groups["modifier"];
            text = text.Remove(group.Index, group.Length);
        }

        modifier = new UnsafeModifier(match.Value, success);
        return success;
    }
}
