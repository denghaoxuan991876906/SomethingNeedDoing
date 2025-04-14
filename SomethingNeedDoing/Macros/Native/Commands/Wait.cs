using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Macros.Native.Commands;
/// <summary>
/// Waits for a specified duration.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WaitCommand"/> class.
/// </remarks>
public class WaitCommand(string text, int waitDuration) : MacroCommandBase(text, waitDuration)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await Task.Delay(WaitDuration, token);
    }
}
