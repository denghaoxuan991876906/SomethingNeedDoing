using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;

/// <summary>
/// Requires items to be spiritbonded before continuing.
/// </summary>
[GenericDoc(
    "Require items to be spiritbonded before continuing",
    ["within_percentage"],
    ["/requirespiritbond", "/requirespiritbond 95"]
)]
public class RequireSpiritbondCommand(string text, float within = 100f) : RequireCommandBase(text)
{
    /// <inheritdoc/>
    protected override Task<bool> CheckCondition(MacroContext context) => Task.FromResult(!Game.HasSpiritbondedItems(within));

    /// <inheritdoc/>
    protected override string GetErrorMessage() => $"No items are spiritbonded (within: {within}%)";

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.WaitForCondition(() => CheckCondition(context).Result, MaxWaitModifier?.MaxWaitMilliseconds ?? DefaultTimeout, DefaultCheckInterval);
        await PerformWait(token);
    }
}
