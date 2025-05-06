namespace SomethingNeedDoing.NativeMacro;

/// <summary>
/// Represents information about a command extracted from command text.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CommandParseInfo"/> class.
/// </remarks>
/// <param name="commandName">The name of the command.</param>
/// <param name="parameters">The parameters of the command.</param>
/// <param name="remainingText">The remaining text after parsing the command.</param>
public class CommandParseInfo(string commandName, string parameters, string remainingText)
{
    /// <summary>
    /// Gets or sets the name of the command.
    /// </summary>
    public string CommandName { get; set; } = commandName;

    /// <summary>
    /// Gets or sets the parameters of the command.
    /// </summary>
    public string Parameters { get; set; } = parameters;

    /// <summary>
    /// Gets or sets the remaining text after parsing the command.
    /// </summary>
    public string RemainingText { get; set; } = remainingText;
}
