using Dalamud.Game.ClientState.Keys;
using ECommons.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Holds keyboard keys down until released.
/// </summary>
public class HoldCommand(string text, VirtualKey[] keys, VirtualKey[] modifiers) : MacroCommandBase(text)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        if (modifiers.Length == 0)
        {
            foreach (var key in keys)
                WindowsKeypress.SendKeyHold(key, null);
        }
        else
        {
            foreach (var key in keys)
                WindowsKeypress.SendKeyHold(key, modifiers);
        }

        await PerformWait(token);
    }
}
