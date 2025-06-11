using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Targets the nearest enemy.
/// </summary>
[GenericDoc(
    "Target the nearest enemy",
    [],
    ["/targetenemy", "/targetenemy <errorif.targetnotfound>"]
)]
public class TargetEnemyCommand(string text) : MacroCommandBase(text)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.RunOnFramework(() =>
        {
            var obj = Svc.Objects
                .OrderBy(o => Vector3.Distance(o.Position, Player.Position))
                .FirstOrDefault(o => o.IsTargetable && o.IsHostile() && !o.IsDead);

            if (obj == null && ErrorIfModifier?.Condition == Modifiers.ErrorCondition.TargetNotFound)
                throw new MacroException("Could not find target");

            if (obj != null)
                Svc.Targets.Target = obj;
        });

        await PerformWait(token);
    }
}
