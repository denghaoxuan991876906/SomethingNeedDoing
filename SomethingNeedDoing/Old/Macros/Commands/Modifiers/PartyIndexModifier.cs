using System.Globalization;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Old.Macros.Commands.Modifiers;

internal class PartyIndexModifier : MacroModifier
{
    public static string Modifier => "<1-9>";
    public static string Description => "For supported commands, specify the index of party members to check against.";
    public static string[] Examples => ["/target <1>"];

    private static readonly Regex Regex = new(@"(?<modifier><(?<index>\d+)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private PartyIndexModifier(int index) => PartyIndex = index;

    public int PartyIndex { get; }

    public static bool TryParse(ref string text, out PartyIndexModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (!success)
        {
            command = new PartyIndexModifier(0);
            return false;
        }

        var group = match.Groups["modifier"];
        text = text.Remove(group.Index, group.Length);

        var indexGroup = match.Groups["index"];
        var indexValue = indexGroup.Value;
        var index = int.Parse(indexValue, CultureInfo.InvariantCulture);

        command = new PartyIndexModifier(index);
        return true;
    }
}
