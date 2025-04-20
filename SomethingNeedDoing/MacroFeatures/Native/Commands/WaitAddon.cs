using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Waits for a specific addon to be visible.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WaitAddonCommand"/> class.
/// </remarks>
public class WaitAddonCommand(string text, string addonName, int maxWait, int waitDuration) : MacroCommandBase(text, waitDuration)
{
    private readonly string addonName = addonName;
    private readonly int maxWait = maxWait > 0 ? maxWait : 5000;
    private const int CHECK_INTERVAL = 250;

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
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
            maxWait,
            CHECK_INTERVAL
        );

        await PerformWait(token);
    }
}
