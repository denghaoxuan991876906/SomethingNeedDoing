using SomethingNeedDoing.Core.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.NativeMacro.Modifiers;
/// <summary>
/// Modifier for specifying object indices for targeting and interaction.
/// </summary>
[GenericDoc(
    "Specify object indices for targeting and interaction",
    ["index"],
    ["/target \"Zodiark\" <index.1>", "/interact \"Y'shtola\" <index.2>"]
)]
public class IndexModifier(string text, int index) : MacroModifierBase(text)
{
    private static readonly Regex Regex = new(@"(?<modifier><index\.(?<index>\d+)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets the target object index.
    /// </summary>
    public int Index { get; } = index;

    /// <inheritdoc/>
    public static bool TryParse(ref string text, out IMacroModifier modifier)
    {
        var match = Regex.Match(text);
        if (!match.Success)
        {
            modifier = null!;
            return false;
        }

        var group = match.Groups["modifier"];
        text = text.Remove(group.Index, group.Length);

        var index = int.Parse(match.Groups["index"].Value, CultureInfo.InvariantCulture);
        modifier = new IndexModifier(group.Value, index);
        return true;
    }
}
