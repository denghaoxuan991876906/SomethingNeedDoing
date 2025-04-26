namespace SomethingNeedDoing.Framework;
/// <summary>
/// Represents information about a Git commit.
/// </summary>
public class GitCommitInfo
{
    /// <summary>
    /// Gets or sets the commit hash.
    /// </summary>
    public string CommitHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the commit message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the commit date.
    /// </summary>
    public DateTime CommitDate { get; set; }

    /// <summary>
    /// Gets or sets the commit author.
    /// </summary>
    public string Author { get; set; } = string.Empty;
}
