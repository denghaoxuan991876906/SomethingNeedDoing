using ECommons.ChatMethods;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.LuaFunctions;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Macros.Exceptions;
using SomethingNeedDoing.Old.Misc;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class RequireQualityCommand : MacroCommand
{
    public static string[] Commands => ["requirequality"];
    public static string Description => "Require a certain amount of quality be present before continuing.";
    public static string[] Examples => ["/requirequality 3000"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<quality>\d+)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly uint requiredQuality;

    private RequireQualityCommand(string text, uint quality, WaitModifier wait) : base(text, wait) => requiredQuality = quality;

    public static RequireQualityCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var qualityValue = match.Groups["quality"].Value;
        var quality = uint.Parse(qualityValue, CultureInfo.InvariantCulture);

        return new RequireQualityCommand(text, quality, waitModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        var current = CraftingState.Instance.GetQuality();

        if (current < requiredQuality)
            throw new MacroPause("Required quality was not found", UIColor.Red);

        await PerformWait(token);
    }
}
