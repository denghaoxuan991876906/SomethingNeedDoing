using ECommons.GameFunctions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Misc;
using SomethingNeedDoing.Old.Macros.Exceptions;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class TargetEnemyCommand : MacroCommand
{
    public static string[] Commands => ["targetenemy"];
    public static string Description => "Targets the nearest enemy.";
    public static string[] Examples => ["/targetenemy"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly int targetIndex;

    private TargetEnemyCommand(string text, WaitModifier wait, IndexModifier index) : base(text, wait, index)
    {
        targetIndex = index.ObjectId;
    }

    public static TargetEnemyCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = IndexModifier.TryParse(ref text, out var indexModifier);
        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);
        return new TargetEnemyCommand(text, waitModifier, indexModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        var target = Svc.Objects.OrderBy(DistanceToObject).FirstOrDefault(o => o.IsTargetable && o.IsHostile() && !o.IsDead);
        Svc.Log.Info("executing");

        if (target == default && C.StopMacroIfTargetNotFound)
            throw new MacroCommandError("Could not find target");

        if (target != default)
            Svc.Targets.Target = target;

        await PerformWait(token);
    }

    private float DistanceToObject(Dalamud.Game.ClientState.Objects.Types.IGameObject o) => Vector3.Distance(o.Position, Svc.ClientState.LocalPlayer!.Position);
}
