using SomethingNeedDoing.Core.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.NativeMacro.Modifiers;
/// <summary>
/// Modifier for specifying maximum wait times for operations.
/// </summary>
[GenericDoc(
    "For certain commands, the maximum time to wait for a certain state to be achieved. By default, this is 5 seconds.",
    ["wait"],
    ["/waitaddon RecipeNote <maxwait.10>"]
)]
public class MaxWaitModifier(string text, int maxWaitMs) : MacroModifierBase(text)
{
    private static readonly Regex Regex = new(@"(?<modifier><maxwait\.(?<wait>\d+(?:\.\d+)?)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets the maximum wait time in milliseconds.
    /// </summary>
    public int MaxWaitMilliseconds { get; } = maxWaitMs;

    /// <inheritdoc/>
    public static bool TryParse(ref string text, out IMacroModifier modifier)
    {
        var match = Regex.Match(text);
        if (!match.Success)
        {
            modifier = new MaxWaitModifier(string.Empty, 5000); // Default 5 seconds
            return false;
        }

        var group = match.Groups["modifier"];
        text = text.Remove(group.Index, group.Length);

        var waitSeconds = float.Parse(match.Groups["wait"].Value, CultureInfo.InvariantCulture);
        var waitMs = (int)(waitSeconds * 1000);
        modifier = new MaxWaitModifier(group.Value, waitMs);
        return true;
    }
}
