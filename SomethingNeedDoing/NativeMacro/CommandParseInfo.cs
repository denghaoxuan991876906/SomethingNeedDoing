namespace SomethingNeedDoing.NativeMacro;

/// <summary>
/// Represents information about a command extracted from command text.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CommandParseInfo"/> class.
/// </remarks>
/// <param name="commandName">The name of the command.</param>
/// <param name="Parameters">The parameters of the command.</param>
/// <param name="RemainingText">The remaining text after parsing the command.</param>
public record class CommandParseInfo(string CommandName, string Parameters, string RemainingText)
{
    /// <summary>
    /// Gets or sets the name of the command.
    /// </summary>
    public string CommandName { get; set; } = CommandName;

    /// <summary>
    /// Gets or sets the parameters of the command.
    /// </summary>
    public string Parameters { get; set; } = Parameters;

    /// <summary>
    /// Gets or sets the remaining text after parsing the command.
    /// </summary>
    public string RemainingText { get; set; } = RemainingText;
}
