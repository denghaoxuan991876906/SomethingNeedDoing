using Lumina.Excel.Sheets;

namespace SomethingNeedDoing.Core.Exceptions;
/// <summary>
/// Exception thrown when a macro needs to pause but not treat it as an error.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MacroPauseException"/> class.
/// </remarks>
public class MacroPauseException(string message, UIColor color) : MacroException(message)
{
    /// <summary>
    /// Gets the color to use when displaying the pause message.
    /// </summary>
    public UIColor Color { get; } = color;
}
