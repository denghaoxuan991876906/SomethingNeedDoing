using SomethingNeedDoing.MacroFeatures.Native.Modifiers;
using System.Text.RegularExpressions;
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
public abstract class MacroCommandBase(string text, WaitModifier? waitDuration) : IMacroCommand
{
    /// <summary>
    /// Gets the original text of the command.
    /// </summary>
    public string CommandText { get; } = text;

    /// <summary>
    /// Gets the wait duration in milliseconds.
    /// </summary>
    protected int WaitDuration { get; } = waitDuration?.WaitDuration ?? 0;

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

    protected string ExtractAndUnquote(Match match, string groupName)
    {
        var group = match.Groups[groupName];
        var groupValue = group.Value;

        if (groupValue.StartsWith('"') && groupValue.EndsWith('"'))
            groupValue = groupValue.Trim('"');

        return groupValue;
    }

    /// <summary>
    /// Parses a command from text.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>The parsed command.</returns>
    /// <exception cref="MacroSyntaxError">Thrown when the text cannot be parsed as a valid command.</exception>
    public abstract IMacroCommand Parse(string text);
}
