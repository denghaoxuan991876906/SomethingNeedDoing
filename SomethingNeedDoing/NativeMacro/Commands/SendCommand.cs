using Dalamud.Game.ClientState.Keys;
using ECommons.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Sends keyboard input to the game.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SendCommand"/> class.
/// </remarks>
[GenericDoc(
    "Send keyboard press to the game",
    ["keys", "modifiers"],
    ["/send W", "/send CONTROL+MENU+SHIFT+NUMPAD0"]
)]
public class SendCommand(string text, VirtualKey[] keys, VirtualKey[] modifiers) : MacroCommandBase(text)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        foreach (var key in keys)
            WindowsKeypress.SendKeypress(key, modifiers);

        await PerformWait(token);
    }
}
