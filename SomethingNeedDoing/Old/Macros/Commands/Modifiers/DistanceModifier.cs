using System.Globalization;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Old.Macros.Commands.Modifiers;

/// <summary>
/// The &lt;index&gt; modifier.
/// </summary>
internal class DistanceModifier : MacroModifier
{
    public static string Modifier => "<distance>";
    public static string Description => "For supported commands, specifiy the max distance to check against.";
    public static string[] Examples => ["/target Alexander <distance.10>"];

    private static readonly Regex Regex = new(@"(?<modifier><distance\.(?<distance>\d+(?:\.\d+)?)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private DistanceModifier(int distance) => Distance = distance;

    public int Distance { get; }

    public static bool TryParse(ref string text, out DistanceModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (!success)
        {
            command = new DistanceModifier(0);
            return false;
        }

        var group = match.Groups["modifier"];
        text = text.Remove(group.Index, group.Length);

        var indexGroup = match.Groups["objectId"];
        var indexValue = indexGroup.Value;
        var index = int.Parse(indexValue, CultureInfo.InvariantCulture);

        command = new DistanceModifier(index);
        return true;
    }
}
