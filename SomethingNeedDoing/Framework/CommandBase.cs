using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Base class for all macro commands providing common functionality.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MacroCommandBase"/> class.
/// </remarks>
/// <param name="text">The original command text.</param>
/// <param name="waitDuration">The wait duration in milliseconds.</param>
public abstract class MacroCommandBase(string text, int waitDuration = 0) : IMacroCommand
{
    /// <summary>
    /// Gets the original text of the command.
    /// </summary>
    public string CommandText { get; } = text;

    /// <summary>
    /// Gets the wait duration in milliseconds.
    /// </summary>
    protected int WaitDuration { get; } = waitDuration;

    /// <summary>
    /// Gets whether this command must run on the framework thread.
    /// </summary>
    public abstract bool RequiresFrameworkThread { get; }

    /// <inheritdoc/>
    public abstract Task Execute(MacroContext context, CancellationToken token);

    /// <summary>
    /// Performs the wait specified by the wait modifier.
    /// </summary>
    protected async Task PerformWait(CancellationToken token)
    {
        if (WaitDuration > 0)
            await Task.Delay(WaitDuration, token);
    }
}
