using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Loops the current macro a specified number of times.
/// </summary>
public class LoopCommand(string text, int loopCount, IMacroScheduler scheduler) : MacroCommandBase(text)
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

            loopsRemaining--;

            if (loopsRemaining < 0)
            {
                loopsRemaining = loopCount;
                return;
            }
        }

        context.Loop();
        scheduler.CheckLoopPause(context.Macro.Id);
        scheduler.CheckLoopStop(context.Macro.Id);

        await Task.Delay(10, token);
        await PerformWait(token);
    }

    /// <summary>
    /// Parses a loop command from text.
    /// </summary>
    //public override LoopCommand Parse(string text)
    //{
    //    _ = WaitModifier.TryParse(ref text, out var waitMod);
    //    _ = EchoModifier.TryParse(ref text, out var echoMod);

    //    var match = Regex.Match(text, @"^/loop(?:\s+(?<count>\d+))?\s*$", RegexOptions.Compiled);
    //    if (!match.Success)
    //        throw new MacroSyntaxError(text);

    //    var countGroup = match.Groups["count"];
    //    var count = countGroup.Success ? int.Parse(countGroup.Value, CultureInfo.InvariantCulture) : int.MaxValue;

    //    return new(text, count, _scheduler, waitMod as WaitModifier, echoMod as EchoModifier);
    //}
}
