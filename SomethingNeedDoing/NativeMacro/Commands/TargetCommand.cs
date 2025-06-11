using Dalamud.Game.ClientState.Objects.Types;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Targets game objects based on name or other criteria.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TargetCommand"/> class.
/// </remarks>
[GenericDoc(
    "Target a game object by name",
    ["targetName"],
    ["/target \"Target Name\"", "/target \"Target Name\" <errorif.targetnotfound>", "/target \"Target Name\" <1>"]
)]
public class TargetCommand(string text, string targetName) : MacroCommandBase(text)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.RunOnFramework(() =>
        {
            IGameObject? target;
            if (PartyIndexModifier is { Index: var index })
                target = Svc.Party[index - 1]?.GameObject;
            else
                target = Svc.Objects.OrderBy(o => Player.DistanceTo(o))
                        .Where(obj => obj.Name.TextValue.Equals(targetName, StringComparison.InvariantCultureIgnoreCase) && obj.IsTargetable && (IndexModifier?.Index <= 0 || obj.ObjectIndex == IndexModifier?.Index))
                        .Skip(ListIndexModifier?.Index ?? 0)
                        .FirstOrDefault();

            if (target == null && ErrorIfModifier?.Condition == Modifiers.ErrorCondition.TargetNotFound)
                throw new MacroException("Could not find target");

            if (target != null)
                Svc.Targets.Target = target;
        });

        await PerformWait(token);
    }
}
