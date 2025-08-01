namespace SomethingNeedDoing.Core.Events;

/// <summary>
/// Event arguments for when a macro's content changes.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MacroContentChangedEventArgs"/> class.
/// </remarks>
/// <param name="macroId">The ID of the macro whose content changed.</param>
/// <param name="newContent">The new content of the macro.</param>
/// <param name="oldContent">The previous content of the macro.</param>
public class MacroContentChangedEventArgs(string macroId, string newContent, string oldContent) : EventArgs
{
    /// <summary>
    /// Gets the ID of the macro whose content changed.
    /// </summary>
    public string MacroId { get; } = macroId;

    /// <summary>
    /// Gets the new content of the macro.
    /// </summary>
    public string NewContent { get; } = newContent;

    /// <summary>
    /// Gets the previous content of the macro.
    /// </summary>
    public string OldContent { get; } = oldContent;
}
