using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Executes a callback on a game addon.
/// </summary>
public class CallbackCommand(string text, string addonName, bool updateState, object[] values) : MacroCommandBase(text)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.RunOnFramework(() =>
        {
            unsafe
            {
                if (!TryGetAddonByName<AtkUnitBase>(addonName, out var addon))
                {
                    if (C.StopMacroIfAddonNotFound)
                        throw new MacroException($"Addon {addonName} not found");
                    return;
                }

                if (IsAddonReady(addon))
                {
                    Svc.Log.Debug($"Sending callback to {addonName} with args [{string.Join(", ", values)}]");
                    Callback.Fire(addon, updateState, values);
                }
            }
        });

        await PerformWait(token);
    }
}
