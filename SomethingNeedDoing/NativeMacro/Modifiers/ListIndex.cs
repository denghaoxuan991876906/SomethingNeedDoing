using SomethingNeedDoing.Core.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.NativeMacro.Modifiers;
/// <summary>
/// Modifier for specifying list indices for multi-target operations.
/// </summary>
[GenericDoc(
    "Specify list indices for multi-target operations",
    ["index"],
    ["/target \"Right Arm\" <list.1>", "/target \"Right Arm\" <list.2>"]
)]
public class ListIndexModifier(string text, int index) : MacroModifierBase(text)
{
    private static readonly Regex Regex = new(@"(?<modifier><list\.(?<index>\d+)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets the list index.
    /// </summary>
    public int Index { get; } = index;

    /// <inheritdoc/>
    public static bool TryParse(ref string text, out IMacroModifier modifier)
    {
        var match = Regex.Match(text);
        if (!match.Success)
        {
            modifier = new ListIndexModifier(string.Empty, 0);
            return false;
        }

        var group = match.Groups["modifier"];
        text = text.Remove(group.Index, group.Length);

        var index = int.Parse(match.Groups["index"].Value, CultureInfo.InvariantCulture);
        modifier = new ListIndexModifier(group.Value, index);
        return true;
    }
}
