using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Macros.Native.Modifiers;

namespace SomethingNeedDoing.Macros.Native.Commands;
/// <summary>
/// Loops the current macro a specified number of times.
/// </summary>
public class LoopCommand(string text, int loopCount, WaitModifier? waitMod = null, EchoModifier? echoMod = null) : MacroCommandBase(text, waitMod?.WaitDuration ?? 0)
{
    private const int MaxLoops = int.MaxValue;
    private readonly int startingLoops = loopCount;
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
            if (echoMod?.ShouldEcho == true || C.LoopEcho)
                Svc.Chat.PrintMessage("Looping");
        }
        else
        {
            if (echoMod?.ShouldEcho == true || C.LoopEcho)
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
                loopsRemaining = startingLoops;
                return;
            }
        }

        context.Loop();
        context.CheckLoopPause();
        context.CheckLoopStop();

        await Task.Delay(10, token);
        await PerformWait(token);
    }

    /// <summary>
    /// Parses a loop command from text.
    /// </summary>
    public static LoopCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);
        _ = EchoModifier.TryParse(ref text, out var echoMod);

        var match = Regex.Match(text, @"^/loop(?:\s+(?<count>\d+))?\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var countGroup = match.Groups["count"];
        var count = countGroup.Success ? int.Parse(countGroup.Value, CultureInfo.InvariantCulture) : int.MaxValue;

        return new(text, count, waitMod as WaitModifier, echoMod as EchoModifier);
    }
}
