using System.Threading;
using System.Threading.Tasks;
using ECommons.Logging;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Interacts with the current target.
/// </summary>
public class InteractCommand(string text) : MacroCommandBase(text)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.RunOnFramework(() =>
        {
            if (Game.Interact(Svc.Targets.Target))
                PluginLog.Log($"Interacting with [{Svc.Targets.Target?.Address:X}] {Svc.Targets.Target?.Name}");
            else
                PluginLog.Log($"Failed to interact with target.");
        });

        await PerformWait(token);
    }
}
