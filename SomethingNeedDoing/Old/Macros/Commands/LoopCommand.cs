using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Misc;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class LoopCommand : MacroCommand
{
    public static string[] Commands => ["loop"];
    public static string Description => "Loop the current macro forever, or a certain amount of times.";
    public static string[] Examples => ["/loop", "/loop 5"];

    private const int MaxLoops = int.MaxValue;
    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}(?:\s+(?<count>\d+))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly EchoModifier echoMod;
    private readonly int startingLoops;
    private int loopsRemaining;

    private LoopCommand(string text, int loopCount, WaitModifier wait, EchoModifier echo) : base(text, wait)
    {
        loopsRemaining = loopCount >= 0 ? loopCount : MaxLoops;
        startingLoops = loopsRemaining;

        if (C.LoopTotal && loopsRemaining != 0 && loopsRemaining != MaxLoops)
            loopsRemaining -= 1;

        echoMod = echo;
    }

    public static LoopCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = EchoModifier.TryParse(ref text, out var echoModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var countGroup = match.Groups["count"];
        var countValue = countGroup.Success
            ? int.Parse(countGroup.Value, CultureInfo.InvariantCulture)
            : int.MaxValue;

        return new LoopCommand(text, countValue, waitModifier, echoModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        if (loopsRemaining == MaxLoops)
        {
            if (echoMod.PerformEcho || C.LoopEcho)
                Service.ChatManager.PrintMessage("Looping");
        }
        else
        {
            if (echoMod.PerformEcho || C.LoopEcho)
            {
                if (loopsRemaining == 0)
                    Service.ChatManager.PrintMessage("No loops remaining");
                else
                {
                    var noun = loopsRemaining == 1 ? "loop" : "loops";
                    Service.ChatManager.PrintMessage($"{loopsRemaining} {noun} remaining");
                }
            }

            loopsRemaining--;

            if (loopsRemaining < 0)
            {
                loopsRemaining = startingLoops;
                return;
            }
        }

        macro.Loop();
        Service.MacroManager.LoopCheckForPause();
        Service.MacroManager.LoopCheckForStop();
        await Task.Delay(10, token);
        await PerformWait(token);
    }
}
