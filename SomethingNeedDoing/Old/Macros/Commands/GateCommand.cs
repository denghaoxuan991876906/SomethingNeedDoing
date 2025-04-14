using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Macros.Exceptions;
using SomethingNeedDoing.Old.Misc;

namespace SomethingNeedDoing.Old.Macros.Commands;

/// <summary>
/// The /craft command.
/// </summary>
internal class GateCommand : MacroCommand
{
    public static string[] Commands => ["craft", "gate"];
    public static string Description => "Similar to loop but used at the start of a macro with an infinite /loop at the end. Allows a certain amount of executions before stopping the macro.";
    public static string[] Examples => ["/craft 10"];

    private static readonly Regex Regex = new($@"^/({string.Join("|", Commands)})(?:\s+(?<count>\d+))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly EchoModifier echoMod;
    private readonly int startingCrafts;
    private int craftsRemaining;

    /// <summary>
    /// Initializes a new instance of the <see cref="GateCommand"/> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="craftCount">Craft count.</param>
    /// <param name="wait">Wait value.</param>
    /// <param name="echo">Echo value.</param>
    private GateCommand(string text, int craftCount, WaitModifier wait, EchoModifier echo) : base(text, wait)
    {
        startingCrafts = craftsRemaining = craftCount;
        echoMod = echo;
    }

    /// <summary>
    /// Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static GateCommand Parse(string text)
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

        return new GateCommand(text, countValue, waitModifier, echoModifier);
    }

    /// <inheritdoc/>
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        if (echoMod.PerformEcho || C.LoopEcho)
        {
            if (echoMod.PerformEcho || C.LoopEcho)
            {
                if (craftsRemaining == 0)
                    Old.Service.ChatManager.PrintMessage("No crafts remaining");
                else
                {
                    var noun = craftsRemaining == 1 ? "craft" : "crafts";
                    Old.Service.ChatManager.PrintMessage($"{craftsRemaining} {noun} remaining");
                }
            }
        }
        craftsRemaining--;

        await PerformWait(token);

        if (craftsRemaining < 0)
        {
            craftsRemaining = startingCrafts;
            throw new GateComplete();
        }
    }
}
