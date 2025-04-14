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

internal class RequireSpiritbondCommand : MacroCommand
{
    public static string[] Commands => ["requirespiritbond"];
    public static string Description => "Pause when an item is ready to have materia extracted. Optional argument to keep crafting if the next highest spiritbond is greater-than-or-equal to the argument value.";
    public static string[] Examples => ["/requirespiritbond", "/requirespiritbond 99.5"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}(\s+(?<within>\d+(?:\.\d+)?))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly float within;

    private RequireSpiritbondCommand(string text, float within, WaitModifier wait) : base(text, wait) => this.within = within;

    public static RequireSpiritbondCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var withinGroup = match.Groups["within"];
        var withinValue = withinGroup.Success ? withinGroup.Value : "100";
        var within = float.Parse(withinValue, CultureInfo.InvariantCulture);

        return new RequireSpiritbondCommand(text, within, waitModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        if (CraftingState.Instance.CanExtractMateria(within))
            throw new MacroPause("You can extract materia now", UIColor.Green);

        await PerformWait(token);
    }
}
