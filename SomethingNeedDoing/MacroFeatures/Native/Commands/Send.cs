using Dalamud.Game.ClientState.Keys;
using ECommons.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Sends keyboard input to the game.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SendCommand"/> class.
/// </remarks>
public class SendCommand(string text, VirtualKey[] keys, VirtualKey[] modifiers, int waitDuration) : MacroCommandBase(text, waitDuration)
{
    private readonly VirtualKey[] keys = keys;
    private readonly VirtualKey[] modifiers = modifiers;

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
