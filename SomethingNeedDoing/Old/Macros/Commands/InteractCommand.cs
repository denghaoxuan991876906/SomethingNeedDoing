using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Misc;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class InteractCommand : MacroCommand
{
    public static string[] Commands => ["interact"];
    public static string Description => "Interacts with current target.";
    public static string[] Examples => ["/interact"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private InteractCommand(string text, WaitModifier wait, IndexModifier index) : base(text, wait, index) { }

    public static InteractCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = IndexModifier.TryParse(ref text, out var indexModifier);
        var match = Regex.Match(text);
        return match.Success ? new InteractCommand(text, waitModifier, indexModifier) : throw new MacroSyntaxError(text);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        var target = Svc.Targets.Target;

        if (target != default)
            unsafe { TargetSystem.Instance()->InteractWithObject(target.Struct(), false); }

        await PerformWait(token);
    }
}
