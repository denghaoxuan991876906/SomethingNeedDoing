using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Requires specific crafting conditions to be met.
/// </summary>
public class RequireCommand(string text, string[] conditions) : RequireCommandBase(text)
{
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
        await context.WaitForCondition(() => CheckCondition(context).Result, MaxWaitModifier?.MaxWaitMilliseconds ?? DefaultTimeout, DefaultCheckInterval);
        await PerformWait(token);
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
