using SomethingNeedDoing.Core.Interfaces;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.NativeMacro.Modifiers;
/// <summary>
/// Modifier for specifying high-quality items.
/// </summary>
public class ItemQualityModifier(string text, bool isHq) : MacroModifierBase(text)
{
    private static readonly Regex Regex = new(@"(?<modifier><hq>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets whether the item should be high quality.
    /// </summary>
    public bool IsHighQuality { get; } = isHq;

    /// <inheritdoc/>
    public static bool TryParse(ref string text, out IMacroModifier modifier)
    {
        var match = Regex.Match(text);
        if (!match.Success)
        {
            modifier = new ItemQualityModifier(string.Empty, false);
            return false;
        }

        var group = match.Groups["modifier"];
        text = text.Remove(group.Index, group.Length);

        modifier = new ItemQualityModifier(group.Value, true);
        return true;
    }
}
