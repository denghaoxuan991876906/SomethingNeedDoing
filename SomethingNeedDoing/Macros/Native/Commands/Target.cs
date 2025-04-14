using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Macros.Native.Commands;
/// <summary>
/// Targets game objects based on name or other criteria.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TargetCommand"/> class.
/// </remarks>
public class TargetCommand(string text, string targetName, int targetIndex, float maxDistance, int waitDuration) : MacroCommandBase(text, waitDuration)
{
    private readonly string targetName = targetName.ToLowerInvariant();
    private readonly int targetIndex = targetIndex;
    private readonly float maxDistance = maxDistance;

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.RunOnFramework(() =>
        {
            var target = Svc.Objects
                .Where(o => o.IsTargetable)
                .Where(o => o.Name.TextValue.Equals(targetName, StringComparison.InvariantCultureIgnoreCase))
                .Where(o => maxDistance <= 0 || Vector3.Distance(o.Position, Svc.ClientState.LocalPlayer!.Position) <= maxDistance)
                .FirstOrDefault(o => targetIndex <= 0 || o.ObjectIndex == targetIndex);

            if (target == null && C.StopMacroIfTargetNotFound)
                throw new MacroException("Could not find target");

            if (target != null)
                Svc.Targets.Target = target;
        });

        await PerformWait(token);
    }
}
