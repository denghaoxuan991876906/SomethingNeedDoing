using Dalamud.Game.ClientState.Keys;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ECommons.Automation;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Macros.Exceptions;
using SomethingNeedDoing.Old.Misc;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class HoldCommand : MacroCommand
{
    public static string[] Commands => ["hold"];
    public static string Description => "Holds an arbitrary keystroke with optional modifiers. Keys are pressed in the same order as the command. Requires release to release the held keys.";
    public static string[] Examples => ["/hold MULTIPLY", "/hold NUMPAD0", "/hold CONTROL+MENU+SHIFT+NUMPAD0"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly VirtualKey[] vkCodes;

    private HoldCommand(string text, VirtualKey[] vkCodes, WaitModifier wait) : base(text, wait) => this.vkCodes = vkCodes;

    public static HoldCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");
        var vkCodes = nameValue.Split("+")
            .Select(name =>
            {
                return !Enum.TryParse<VirtualKey>(name, true, out var vkCode) ? throw new MacroCommandError("Invalid virtual key") : vkCode;
            })
            .ToArray();

        return new HoldCommand(text, vkCodes, waitModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        if (vkCodes.Length == 1)
            WindowsKeypress.SendKeyHold(vkCodes[0], null);
        else
        {
            var key = vkCodes.Last();
            var mods = vkCodes.SkipLast(1);
            WindowsKeypress.SendKeyHold(key, mods);
        }

        await PerformWait(token);
    }
}
