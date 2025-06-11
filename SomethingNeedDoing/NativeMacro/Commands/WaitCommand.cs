using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Waits for a specified duration.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WaitCommand"/> class.
/// </remarks>
[GenericDoc(
    "Wait for a specified duration",
    ["duration"],
    ["/wait 1", "/wait 1-2"]
)]
public class WaitCommand(string text) : MacroCommandBase(text)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token) => await Task.Delay(WaitDuration, token);
}
