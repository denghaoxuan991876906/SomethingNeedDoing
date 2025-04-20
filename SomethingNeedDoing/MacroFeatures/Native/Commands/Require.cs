using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Requires specific crafting conditions to be met.
/// </summary>
public class RequireCommand(string text, string[] conditions, WaitModifier? waitMod = null, MaxWaitModifier? maxWaitMod = null) : RequireCommandBase(text, waitMod)
{
    private readonly int timeout = maxWaitMod?.MaxWaitMilliseconds ?? DefaultTimeout;

    /// <inheritdoc/>
    protected override async Task<bool> CheckCondition(MacroContext context)
    {
        var result = false;
        await context.RunOnFramework(() =>
        {
            result = conditions.Any(c => Game.Crafting.GetCondition().ToString() == c);
        });
        return result;
    }

    /// <inheritdoc/>
    protected override string GetErrorMessage() => "Required condition not found";

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.WaitForCondition(() => CheckCondition(context).Result, timeout, DefaultCheckInterval);
        await PerformWait(token);
    }

    /// <summary>
    /// Parses a require command from text.
    /// </summary>
    public static RequireCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);
        _ = MaxWaitModifier.TryParse(ref text, out var maxWaitMod);

        var match = Regex.Match(text, @"^/require\s+(?<conditions>.*?)\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var conditions = match.Groups["conditions"].Value
            .Trim('"')
            .Split(',')
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrEmpty(c))
            .ToArray();

        return new(text, conditions, waitMod as WaitModifier, maxWaitMod as MaxWaitModifier);
    }

    //protected override bool CheckCondition(MacroContext context)
    //{
    //    // Parse the condition string and check the corresponding state
    //    var parts = condition.Split(' ');
    //    if (parts.Length < 2) return false;

    //    var type = parts[0].ToLower();
    //    var value = parts[1];

    //    return type switch
    //    {
    //        "gp" => CheckGP(value),
    //        "mp" => CheckMP(value),
    //        "cp" => CheckCP(value),
    //        "incombat" => CheckInCombat(value),
    //        "ininstance" => CheckInInstance(value),
    //        "hasbuff" => CheckHasBuff(value),
    //        "hasdebuff" => CheckHasDebuff(value),
    //        "hasstatus" => CheckHasStatus(value),
    //        "hasitem" => CheckHasItem(value),
    //        "hasaction" => CheckHasAction(value),
    //        "hasability" => CheckHasAbility(value),
    //        "hasrecipe" => CheckHasRecipe(value),
    //        "hasmateria" => CheckHasMateria(value),
    //        _ => false
    //    };
    //}
}
