using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;

/// <summary>
/// Requires items to need repair before continuing.
/// </summary>
[GenericDoc(
    "Require items to need repair before continuing",
    ["durability_threshold"],
    ["/requirerepair 10", "/requirerepair"]
)]
public class RequireRepairCommand(string text, int durabilityThreshold = 0) : RequireCommandBase(text)
{
    /// <inheritdoc/>
    protected override Task<bool> CheckCondition(MacroContext context) => Task.FromResult(Game.NeedsRepair(durabilityThreshold));

    /// <inheritdoc/>
    protected override string GetErrorMessage() => $"No items need repair (durability threshold: {durabilityThreshold}%)";

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.WaitForCondition(() => CheckCondition(context).Result, MaxWaitModifier?.MaxWaitMilliseconds ?? DefaultTimeout, DefaultCheckInterval);
        await PerformWait(token);
    }
}
