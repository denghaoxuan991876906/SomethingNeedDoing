using Dalamud.Game.ClientState.Keys;
using ECommons.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Holds keyboard keys down until released.
/// </summary>
[GenericDoc(
    "Hold keyboard keys down until released",
    ["keys", "modifiers"],
    ["/hold W", "/hold CONTROL+MENU+SHIFT+NUMPAD0"]
)]
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
