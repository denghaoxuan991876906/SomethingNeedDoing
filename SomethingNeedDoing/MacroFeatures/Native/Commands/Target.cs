using Dalamud.Game.ClientState.Objects.Types;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Targets game objects based on name or other criteria.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TargetCommand"/> class.
/// </remarks>
public class TargetCommand(string text, string targetName, IndexModifier? targetIndex, ListIndexModifier? listIndex, PartyIndexModifier? partyIndex, WaitModifier? waitDuration) : MacroCommandBase(text, waitDuration)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.RunOnFramework(() =>
        {
            var target = partyIndex != default
                ? (Svc.Party[partyIndex.Index - 1]?.GameObject)
                : Svc.Objects.OrderBy(o => Player.DistanceTo(o))
                    .Where(obj => obj.Name.TextValue.Equals(targetName, StringComparison.InvariantCultureIgnoreCase) && obj.IsTargetable && (targetIndex?.Index <= 0 || obj.ObjectIndex == targetIndex?.Index))
                    .Skip(listIndex?.Index ?? 0)
                    .FirstOrDefault();

            if (target == null && C.StopMacroIfTargetNotFound)
                throw new MacroException("Could not find target");

            if (target != null)
                Svc.Targets.Target = target;
        });

        await PerformWait(token);
    }

    private static readonly Regex Regex = new($@"^/target\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public override TargetCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = IndexModifier.TryParse(ref text, out var indexModifier);
        _ = ListIndexModifier.TryParse(ref text, out var listIndexModifier);
        _ = PartyIndexModifier.TryParse(ref text, out var partyIndexModifier);
        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");
        return new TargetCommand(text, nameValue, indexModifier as IndexModifier, listIndexModifier as ListIndexModifier, partyIndexModifier as PartyIndexModifier, waitModifier as WaitModifier);
    }
}
