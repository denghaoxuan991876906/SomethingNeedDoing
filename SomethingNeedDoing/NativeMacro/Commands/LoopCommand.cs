using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Loops the current macro a specified number of times.
/// </summary>
/// <param name="text">The command text.</param>
/// <param name="loopCount">The number of loops.</param>
[GenericDoc(
    "Loop the current macro a specified number of times",
    ["loopCount"],
    ["/loop 10", "/loop 10 <echo>", "/loop"]
)]
public class LoopCommand(string text, int loopCount) : MacroCommandBase(text)
{
    private const int MaxLoops = int.MaxValue;
    private int loopsRemaining = loopCount >= 0 ? loopCount : MaxLoops;

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        if (C.LoopTotal && loopsRemaining != 0 && loopsRemaining != MaxLoops)
            loopsRemaining -= 1;

        if (loopsRemaining == MaxLoops)
        {
            if (EchoModifier?.ShouldEcho == true || C.LoopEcho)
                Svc.Chat.PrintMessage("Looping");
        }
        else
        {
            if (EchoModifier?.ShouldEcho == true || C.LoopEcho)
            {
                if (loopsRemaining == 0)
                    Svc.Chat.PrintMessage("No loops remaining");
                else
                {
                    var noun = loopsRemaining == 1 ? "loop" : "loops";
                    Svc.Chat.PrintMessage($"{loopsRemaining} {noun} remaining");
                }
            }

            if (loopsRemaining <= 0)
                return;
        }

        context.Loop();
        context.OnLoopPauseRequested(this);
        context.OnLoopStopRequested(this);

        if (loopsRemaining != MaxLoops)
            loopsRemaining--;

        await Task.Delay(10, token);
        await PerformWait(token);
    }
}
