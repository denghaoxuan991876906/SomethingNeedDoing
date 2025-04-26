namespace SomethingNeedDoing.Framework;

/// <summary>
/// Event arguments for Git macro update events.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GitMacroUpdateEventArgs"/> class.
/// </remarks>
/// <param name="macro">The macro that was updated.</param>
/// <param name="error">The error that occurred during the update, if any.</param>
public class GitMacroUpdateEventArgs(GitMacro macro, Exception? error = null) : EventArgs
{
    /// <summary>
    /// Gets the macro that was updated.
    /// </summary>
    public GitMacro Macro { get; } = macro;

    /// <summary>
    /// Gets the error that occurred during the update, if any.
    /// </summary>
    public Exception? Error { get; } = error;
}
