using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Controls crafting loop execution with a gate count.
/// </summary>
public class GateCommand(string text, int gateCount) : MacroCommandBase(text)
{
    private readonly int startingCrafts = gateCount;
    private int remainingCount = gateCount;

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        if (EchoModifier?.ShouldEcho == true || C.LoopEcho)
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
}
