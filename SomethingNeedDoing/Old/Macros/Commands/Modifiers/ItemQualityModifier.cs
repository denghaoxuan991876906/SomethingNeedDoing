using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Old.Macros.Commands.Modifiers;

internal class ItemQualityModifier : MacroModifier
{
    public static string Modifier => "<hq>";
    public static string Description => "For supported commands, specifiy whether or not to use the HQ version of an item.";
    public static string[] Examples => ["/item Calamari Ripieni <hq> <wait.3>"];

    private static readonly Regex Regex = new(@"(?<modifier><hq>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private ItemQualityModifier(bool isHQ) => IsHq = isHQ;

    public bool IsHq { get; }

    public static bool TryParse(ref string text, out ItemQualityModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (success)
        {
            var group = match.Groups["modifier"];
            text = text.Remove(group.Index, group.Length);
        }

        command = new ItemQualityModifier(success);

        return success;
    }
}
