using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.LuaFunctions;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Misc;
using SomethingNeedDoing.Old.Macros.Exceptions;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class RequireStatsCommand : MacroCommand
{
    public static string[] Commands => ["requirestats"];
    public static string Description => "Require a certain amount of stats effect to be present before continuing. Syntax is Craftsmanship, Control, then CP.";
    public static string[] Examples => ["/requirestats 2700 2600 500"];

    private const int StatusCheckMaxWait = 1000;
    private const int StatusCheckInterval = 250;

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<craftsmanship>\d+)\s+(?<control>\d+)\s+(?<cp>\d+)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly uint requiredCraftsmanship;
    private readonly uint requiredControl;
    private readonly uint requiredCp;
    private readonly int maxWait;

    private RequireStatsCommand(string text, uint craftsmanship, uint control, uint cp, WaitModifier wait, MaxWaitModifier maxWait) : base(text, wait)
    {
        requiredCraftsmanship = craftsmanship;
        requiredControl = control;
        requiredCp = cp;

        this.maxWait = maxWait.Wait == 0 ? StatusCheckMaxWait : maxWait.Wait;
    }

    public static RequireStatsCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = MaxWaitModifier.TryParse(ref text, out var maxWaitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var craftsmanshipValue = match.Groups["craftsmanship"].Value;
        var craftsmanship = uint.Parse(craftsmanshipValue, CultureInfo.InvariantCulture);

        var controlValue = match.Groups["control"].Value;
        var control = uint.Parse(controlValue, CultureInfo.InvariantCulture);

        var cpValue = match.Groups["cp"].Value;
        var cp = uint.Parse(cpValue, CultureInfo.InvariantCulture);

        return new RequireStatsCommand(text, craftsmanship, control, cp, waitModifier, maxWaitModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        bool AreStatsGood() => CraftingState.Instance.HasStats(requiredCraftsmanship, requiredControl, requiredCp);

        var hasStats = await LinearWait(StatusCheckInterval, maxWait, AreStatsGood, token);

        if (!hasStats)
            throw new MacroCommandError("Required stats were not found");

        await PerformWait(token);
    }
}
