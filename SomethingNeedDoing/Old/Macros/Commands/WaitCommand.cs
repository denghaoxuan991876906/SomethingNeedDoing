using SomethingNeedDoing.Old.Misc;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class WaitCommand : MacroCommand
{
    public static string[] Commands => ["wait"];
    public static string Description => "The same as the wait modifier, but as a command.";
    public static string[] Examples => ["/wait 1-5"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<wait>\d+(?:\.\d+)?)(?:-(?<until>\d+(?:\.\d+)?))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private WaitCommand(string text, int wait, int waitUntil) : base(text, wait, waitUntil) { }

    public static WaitCommand Parse(string text)
    {
        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var waitGroup = match.Groups["wait"];
        var waitValue = waitGroup.Value;
        var wait = (int)(float.Parse(waitValue, CultureInfo.InvariantCulture) * 1000);

        var untilGroup = match.Groups["until"];
        var untilValue = untilGroup.Success ? untilGroup.Value : "0";
        var until = (int)(float.Parse(untilValue, CultureInfo.InvariantCulture) * 1000);

        return wait > until && until > 0
            ? throw new ArgumentException("Wait value cannot be lower than the until value")
            : new WaitCommand(text, wait, until);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        await PerformWait(token);
    }
}
