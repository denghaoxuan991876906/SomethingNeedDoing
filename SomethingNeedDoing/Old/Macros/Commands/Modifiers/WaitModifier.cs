using System.Globalization;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Old.Macros.Commands.Modifiers;

internal class WaitModifier : MacroModifier
{
    public static string Modifier => "<wait>";
    public static string Description => "Wait a certain amount of time, or a random time within a range.";
    public static string[] Examples => ["/ac Groundwork <wait.3>       # Wait 3 seconds", "/ac Groundwork <wait.3.5>     # Wait 3.5 seconds", "/ac Groundwork <wait.1-5>     # Wait between 1 and 5 seconds", "/ac Groundwork <wait.1.5-5.5> # Wait between 1.5 and 5.5 seconds"];

    private static readonly Regex Regex = new(@"(?<modifier><wait\.(?<wait>\d+(?:\.\d+)?)(?:-(?<until>\d+(?:\.\d+)?))?>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private WaitModifier(int wait, int until)
    {
        Wait = wait;
        Until = until;
    }

    /// <summary>
    /// Gets the milliseconds to wait.
    /// </summary>
    public int Wait { get; }

    /// <summary>
    /// Gets the milliseconds to wait until.
    /// </summary>
    public int Until { get; }

    public static bool TryParse(ref string text, out WaitModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (!success)
        {
            command = new WaitModifier(0, 0);
            return false;
        }

        var group = match.Groups["modifier"];
        text = text.Remove(group.Index, group.Length);

        var waitGroup = match.Groups["wait"];
        var waitValue = waitGroup.Value;
        var wait = (int)(float.Parse(waitValue, CultureInfo.InvariantCulture) * 1000);

        var untilGroup = match.Groups["until"];
        var untilValue = untilGroup.Success ? untilGroup.Value : "0";
        var until = (int)(float.Parse(untilValue, CultureInfo.InvariantCulture) * 1000);

        if (wait > until && until > 0)
            throw new ArgumentException("Until value cannot be lower than the wait value");

        command = new WaitModifier(wait, until);
        return true;
    }
}
