using System.Globalization;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Old.Macros.Commands.Modifiers;

internal class MaxWaitModifier : MacroModifier
{
    public static string Modifier => "<maxwait>";
    public static string Description => "For certain commands, the maximum time to wait for a certain state to be achieved. By default, this is 5 seconds.";
    public static string[] Examples => ["/waitaddon RecipeNote <maxwait.10>"];

    private static readonly Regex Regex = new(@"(?<modifier><maxwait\.(?<wait>\d+(?:\.\d+)?)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private MaxWaitModifier(int wait) => Wait = wait;

    /// <summary>
    /// Gets the milliseconds to wait.
    /// </summary>
    public int Wait { get; }

    public static bool TryParse(ref string text, out MaxWaitModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (success)
        {
            var group = match.Groups["modifier"];
            text = text.Remove(group.Index, group.Length);

            var waitGroup = match.Groups["wait"];
            var waitValue = waitGroup.Value;
            var wait = (int)(float.Parse(waitValue, CultureInfo.InvariantCulture) * 1000);

            command = new MaxWaitModifier(wait);
        }
        else
            command = new MaxWaitModifier(0);

        return success;
    }
}
