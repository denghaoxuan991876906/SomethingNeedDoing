using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Old.Macros.Commands.Modifiers;

internal class EchoModifier : MacroModifier
{
    public static string Modifier => "<echo>";
    public static string Description => "Echo the amount of loops remaining after executing a /loop command.";
    public static string[] Examples => ["/loop 5 <echo>"];

    private static readonly Regex Regex = new(@"(?<modifier><echo>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private EchoModifier(bool echo) => PerformEcho = echo;

    public bool PerformEcho { get; }

    public static bool TryParse(ref string text, out EchoModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (success)
        {
            var group = match.Groups["modifier"];
            text = text.Remove(group.Index, group.Length);
        }

        command = new EchoModifier(success);

        return success;
    }
}
