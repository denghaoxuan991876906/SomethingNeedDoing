using System.Globalization;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Old.Macros.Commands.Modifiers;

internal class ListIndexModifier : MacroModifier
{
    public static string Modifier => "<list>";
    public static string Description => "For supported commands, specify the index to check. For example, when there are multiple targets with the same name.";
    public static string[] Examples => ["/target abc <list.5>"];

    private static readonly Regex Regex = new(@"(?<modifier><list\.(?<listIndex>\d+(?:\.\d+)?)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private ListIndexModifier(int listIndex) => ListIndex = listIndex;

    public int ListIndex { get; }

    public static bool TryParse(ref string text, out ListIndexModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (!success)
        {
            command = new ListIndexModifier(0);
            return false;
        }

        var group = match.Groups["modifier"];
        text = text.Remove(group.Index, group.Length);

        var indexGroup = match.Groups["listIndex"];
        var indexValue = indexGroup.Value;
        var index = int.Parse(indexValue, CultureInfo.InvariantCulture);

        command = new ListIndexModifier(index);
        return true;
    }
}
