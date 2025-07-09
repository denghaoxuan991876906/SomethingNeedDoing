using FFXIVClientStructs.FFXIV.Component.GUI;
using SomethingNeedDoing.NativeMacro.Modifiers;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Waits for a specific addon to be visible.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WaitAddonCommand"/> class.
/// </remarks>
[GenericDoc(
    "Wait for a specific addon to be visible",
    ["addonName"],
    ["/waitaddon \"Synthesis\"", "/waitaddon \"Synthesis\" <maxwait.5>"]
)]
public class WaitAddonCommand(string text, string addonName) : MacroCommandBase(text)
{
    private const int CHECK_INTERVAL = 250;

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        try
        {
            await context.WaitForCondition(
                () =>
                {
                    var result = false;
                    context.RunOnFramework(() =>
                    {
                        unsafe
                        {
                            if (TryGetAddonByName<AtkUnitBase>(addonName, out var addon))
                                result = addon->IsVisible && addon->UldManager.LoadedState == AtkLoadState.Loaded;
                        }
                    }).Wait();
                    return result;
                },
                MaxWaitModifier?.MaxWaitMilliseconds > 0 ? MaxWaitModifier.MaxWaitMilliseconds : 5000,
                CHECK_INTERVAL
            );
        }
        catch (MacroTimeoutException)
        {
            if (C.StopOnError || ErrorIfModifier?.Condition == ErrorCondition.ActionTimeout)
                throw new MacroTimeoutException($"Addon '{addonName}' did not appear within the timeout period");
            Svc.Log.Warning($"Addon '{addonName}' did not appear within the timeout period, continuing...");
        }

        await PerformWait(token);
    }
}
