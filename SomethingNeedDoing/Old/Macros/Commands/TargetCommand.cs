using Dalamud.Game.ClientState.Objects.Types;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Macros.Exceptions;
using SomethingNeedDoing.Old.Misc;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class TargetCommand : MacroCommand
{
    public static string[] Commands => ["target"];
    public static string Description => "Target anyone and anything that can be selected.";
    public static string[] Examples => ["/target Eirikur", "/target Moyce"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string targetName;
    private readonly int targetIndex;
    private readonly int listIndex;
    private readonly int partyIndex;

    private TargetCommand(string text, string targetName, WaitModifier wait, IndexModifier index, ListIndexModifier listIndex, PartyIndexModifier partyIndex) : base(text, wait, index)
    {
        targetIndex = index.ObjectId;
        this.targetName = targetName.ToLowerInvariant();
        this.listIndex = listIndex.ListIndex;
        this.partyIndex = partyIndex.PartyIndex;
    }

    public static TargetCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = IndexModifier.TryParse(ref text, out var indexModifier);
        _ = ListIndexModifier.TryParse(ref text, out var listIndexModifier);
        _ = PartyIndexModifier.TryParse(ref text, out var partyIndexModifier);
        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");
        return new TargetCommand(text, nameValue, waitModifier, indexModifier, listIndexModifier, partyIndexModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        IGameObject? target;

        if (partyIndex != default)
            target = Svc.Party[partyIndex - 1]?.GameObject;
        else
            Svc.Log.Info($"looking for non party member target");
        target = Svc.Objects
            .OrderBy(o => Vector3.Distance(o.Position, Svc.ClientState.LocalPlayer!.Position))
            .Where(obj => obj.Name.TextValue.Equals(targetName, StringComparison.InvariantCultureIgnoreCase) && obj.IsTargetable && (targetIndex <= 0 || obj.ObjectIndex == targetIndex))
            .Skip(listIndex)
            .FirstOrDefault();

        if (target == default && C.StopMacroIfTargetNotFound)
            throw new MacroCommandError("Could not find target");
        if (target != default)
            Svc.Targets.Target = target;

        await PerformWait(token);
    }
}
