namespace SomethingNeedDoing.Core.Exceptions;
/// <summary>
/// Exception thrown when macro syntax is invalid.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MacroSyntaxError"/> class.
/// </remarks>
public class MacroSyntaxError(string command, string? guidance = null) : MacroException($"Syntax error in command: {command}{(guidance != null ? $"\nGuidance: {guidance}" : string.Empty)}")
{
    /// <summary>
    /// Gets the command that failed parsing.
    /// </summary>
    public string Command { get; } = command;

    /// <summary>
    /// Gets additional guidance for fixing the error.
    /// </summary>
    public string? Guidance { get; } = guidance;
}
