using Dalamud.Game.ClientState.Keys;
using ECommons.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Releases held keyboard keys.
/// </summary>
public class ReleaseCommand(string text, VirtualKey[] keys, VirtualKey[] modifiers) : MacroCommandBase(text)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        if (modifiers.Length == 0)
        {
            foreach (var key in keys)
                WindowsKeypress.SendKeyRelease(key, null);
        }
        else
        {
            foreach (var key in keys)
                WindowsKeypress.SendKeyRelease(key, modifiers);
        }

        await PerformWait(token);
    }
}
