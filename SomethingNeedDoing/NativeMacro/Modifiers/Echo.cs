using SomethingNeedDoing.Core.Interfaces;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.NativeMacro.Modifiers;
/// <summary>
/// Modifier for controlling loop count echo behavior.
/// </summary>
public class EchoModifier(string text, bool shouldEcho) : MacroModifierBase(text)
{
    private static readonly Regex Regex = new(@"(?<modifier><echo>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets whether to echo loop counts.
    /// </summary>
    public bool ShouldEcho { get; } = shouldEcho;

    /// <inheritdoc/>
    public static bool TryParse(ref string text, out IMacroModifier modifier)
    {
        var match = Regex.Match(text);
        if (!match.Success)
        {
            modifier = new EchoModifier(string.Empty, false);
            return false;
        }

        var group = match.Groups["modifier"];
        text = text.Remove(group.Index, group.Length);

        modifier = new EchoModifier(group.Value, true);
        return true;
    }
}
