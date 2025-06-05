using System.Text.Json.Serialization;

namespace SomethingNeedDoing.Core.Github;
/// <summary>
/// Represents information about a Git commit.
/// </summary>
public class GitCommitInfo
{
    /// <summary>
    /// Gets or sets the commit hash.
    /// </summary>
    [JsonPropertyName("sha")]
    public string CommitHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the commit message.
    /// </summary>
    [JsonPropertyName("commit")]
    public CommitDetails Commit { get; set; } = new();

    /// <summary>
    /// Gets or sets the commit author.
    /// </summary>
    [JsonPropertyName("author")]
    public AuthorDetails Author { get; set; } = new();
}

/// <summary>
/// Details about a commit.
/// </summary>
public class CommitDetails
{
    /// <summary>
    /// Gets or sets the commit message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the commit date.
    /// </summary>
    [JsonPropertyName("author")]
    public CommitAuthor Author { get; set; } = new();
}

/// <summary>
/// Author information for a commit.
/// </summary>
public class CommitAuthor
{
    /// <summary>
    /// Gets or sets the commit date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
}

/// <summary>
/// Author details.
/// </summary>
public class AuthorDetails
{
    /// <summary>
    /// Gets or sets the author's login name.
    /// </summary>
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;
}
