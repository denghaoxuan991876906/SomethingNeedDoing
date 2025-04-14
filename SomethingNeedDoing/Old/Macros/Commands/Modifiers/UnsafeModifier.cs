using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Old.Macros.Commands.Modifiers;

internal class UnsafeModifier : MacroModifier
{
    public static string Modifier => "<unsafe>";
    public static string Description => "Prevent the /action command from waiting for a positive server response and attempting to execute the command anyways.";
    public static string[] Examples => ["/ac \"Tricks of the Trade\" <unsafe>"];

    private static readonly Regex Regex = new(@"(?<modifier><unsafe>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private UnsafeModifier(bool isUnsafe) => IsUnsafe = isUnsafe;

    public bool IsUnsafe { get; }

    public static bool TryParse(ref string text, out UnsafeModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (success)
        {
            var group = match.Groups["modifier"];
            text = text.Remove(group.Index, group.Length);
        }

        command = new UnsafeModifier(success);

        return success;
    }
}
