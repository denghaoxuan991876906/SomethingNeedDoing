using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Controls crafting loop execution with a gate count.
/// </summary>
public class GateCommand(string text, int gateCount, WaitModifier? waitMod = null, EchoModifier? echoMod = null) : MacroCommandBase(text, waitMod)
{
    private readonly int startingCrafts = gateCount;
    private int remainingCount = gateCount;

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        if (echoMod?.ShouldEcho == true || C.LoopEcho)
        {
            if (remainingCount == 0)
                Svc.Chat.Print("No crafts remaining");
            else
            {
                var noun = remainingCount == 1 ? "craft" : "crafts";
                Svc.Chat.Print($"{remainingCount} {noun} remaining");
            }
        }

        remainingCount--;
        await PerformWait(token);

        if (remainingCount < 0)
        {
            remainingCount = startingCrafts;
            throw new MacroGateCompleteException();
        }
    }

    /// <summary>
    /// Parses a gate command from text.
    /// </summary>
    public override GateCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);
        _ = EchoModifier.TryParse(ref text, out var echoMod);

        var match = Regex.Match(text, @"^/(?:craft|gate)(?:\s+(?<count>\d+))?\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var countGroup = match.Groups["count"];
        var count = countGroup.Success ? int.Parse(countGroup.Value) : int.MaxValue;

        return new(text, count, waitMod as WaitModifier, echoMod as EchoModifier);
    }
}
