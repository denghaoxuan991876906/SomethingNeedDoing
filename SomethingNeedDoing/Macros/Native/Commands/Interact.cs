using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Macros.Native.Modifiers;

namespace SomethingNeedDoing.Macros.Native.Commands;
/// <summary>
/// Interacts with the current target.
/// </summary>
public class InteractCommand(string text, WaitModifier? waitMod = null, IndexModifier? indexMod = null) : MacroCommandBase(text, waitMod?.WaitDuration ?? 0)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.RunOnFramework(() =>
        {
            if (Svc.Targets.Target is { } target)
            {
                unsafe
                {
                    TargetSystem.Instance()->InteractWithObject(target.Struct(), false);
                }
            }
        });

        await PerformWait(token);
    }

    /// <summary>
    /// Parses an interact command from text.
    /// </summary>
    public static InteractCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);
        _ = IndexModifier.TryParse(ref text, out var indexMod);

        var match = Regex.Match(text, @"^/interact", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        return new(text, waitMod as WaitModifier, indexMod as IndexModifier);
    }
}
