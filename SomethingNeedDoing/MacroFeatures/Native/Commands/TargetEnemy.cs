using ECommons.GameFunctions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Targets the nearest enemy.
/// </summary>
public class TargetEnemyCommand(string text, WaitModifier? waitMod = null, IndexModifier? indexMod = null) : MacroCommandBase(text, waitMod?.WaitDuration ?? 0)
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

            if (obj == null && C.StopMacroIfTargetNotFound)
                throw new MacroException("Could not find target");

            if (obj != null)
                Svc.Targets.Target = obj;
        });

        await PerformWait(token);
    }

    /// <summary>
    /// Parses a target enemy command from text.
    /// </summary>
    public static TargetEnemyCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);
        _ = IndexModifier.TryParse(ref text, out var indexMod);

        var match = Regex.Match(text, @"^/targetenemy", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        return new(text, waitMod as WaitModifier, indexMod as IndexModifier);
    }
}
