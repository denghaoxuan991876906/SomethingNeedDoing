using SomethingNeedDoing.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Loops the current macro a specified number of times.
/// </summary>
[GenericDoc(
    "Loop the current macro a specified number of times",
    ["loopCount"],
    ["/loop 10", "/loop 10 <echo>", "/loop"]
)]
public class LoopCommand : MacroCommandBase
{
    private const int MaxLoops = int.MaxValue;
    private readonly int _loopCount;
    private readonly IMacroScheduler? _scheduler;
    private int loopsRemaining;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopCommand"/> class.
    /// </summary>
    /// <param name="text">The command text.</param>
    /// <param name="loopCount">The number of loops.</param>
    /// <param name="scheduler">The macro scheduler.</param>
    public LoopCommand(string text, int loopCount, IMacroScheduler scheduler) : base(text)
    {
        _loopCount = loopCount;
        _scheduler = scheduler;
        loopsRemaining = loopCount >= 0 ? loopCount : MaxLoops;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopCommand"/> class.
    /// </summary>
    /// <param name="text">The command text.</param>
    /// <param name="loopCount">The number of loops.</param>
    public LoopCommand(string text, int loopCount) : base(text)
    {
        _loopCount = loopCount;
        _scheduler = null;
        loopsRemaining = loopCount >= 0 ? loopCount : MaxLoops;
    }

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

        if (_scheduler != null)
        {
            _scheduler.CheckLoopPause(context.Macro.Id);
            _scheduler.CheckLoopStop(context.Macro.Id);
        }
        // Note: Loop pause/stop functionality would need to be handled through events
        // when scheduler is not available

        if (loopsRemaining != MaxLoops)
            loopsRemaining--;

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
