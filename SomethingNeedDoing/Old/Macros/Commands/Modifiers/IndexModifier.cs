using System.Globalization;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Old.Macros.Commands.Modifiers;

internal class IndexModifier : MacroModifier
{
    public static string Modifier => "<index>";
    public static string Description => "For supported commands, specify the object index. For example, when there are multiple targets with the same name.";
    public static string[] Examples => ["/target abc <index.5>"];

    private static readonly Regex Regex = new(@"(?<modifier><index\.(?<objectId>\d+(?:\.\d+)?)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private IndexModifier(int objectId) => ObjectId = objectId;

    public int ObjectId { get; }

    public static bool TryParse(ref string text, out IndexModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (!success)
        {
            command = new IndexModifier(0);
            return false;
        }

        var group = match.Groups["modifier"];
        text = text.Remove(group.Index, group.Length);

        var indexGroup = match.Groups["objectId"];
        var indexValue = indexGroup.Value;
        var index = int.Parse(indexValue, CultureInfo.InvariantCulture);

        command = new IndexModifier(index);
        return true;
    }
}
