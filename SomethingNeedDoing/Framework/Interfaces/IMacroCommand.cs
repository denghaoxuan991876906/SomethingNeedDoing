using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Represents a command that can be executed as part of a macro.
/// </summary>
public interface IMacroCommand
{
    /// <summary>
    /// Gets whether this command must run on the framework thread.
    /// </summary>
    public bool RequiresFrameworkThread { get; }

    /// <summary>
    /// Gets the original text of the command.
    /// </summary>
    public string CommandText { get; }

    /// <summary>
    /// Executes the command with the given context and cancellation token.
    /// </summary>
    /// <param name="context">The context in which the command is executing.</param>
    /// <param name="token">A token to cancel execution.</param>
    public Task Execute(MacroContext context, CancellationToken token);

    /// <summary>
    /// Parses a command from text.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>The parsed command.</returns>
    /// <exception cref="MacroSyntaxError">Thrown when the text cannot be parsed as a valid command.</exception>
    public IMacroCommand Parse(string text);
}
