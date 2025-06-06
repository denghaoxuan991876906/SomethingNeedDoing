using SomethingNeedDoing.Core.Github;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Contains information about a macro's Git repository source.
/// </summary>
public class GitInfo
{
    /// <summary>
    /// Gets or sets the Git repository URL.
    /// </summary>
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner of the repository.
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the repository.
    /// </summary>
    public string Repo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Git branch name.
    /// </summary>
    public string Branch { get; set; } = "main";

    /// <summary>
    /// Gets or sets the file path within the repository.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current commit hash.
    /// </summary>
    public string CommitHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to automatically update this macro.
    /// </summary>
    public bool AutoUpdate { get; set; } = true;

    /// <summary>
    /// Gets or sets the current version of the macro.
    /// </summary>
    public string CurrentVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the latest available version of the macro.
    /// </summary>
    public string LatestVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether there is an update available.
    /// </summary>
    public bool HasUpdate { get; set; }

    /// <summary>
    /// Gets or sets the last time this macro was checked for updates.
    /// </summary>
    public DateTime LastUpdateCheck { get; set; }

    /// <summary>
    /// Gets or sets the version history of this macro.
    /// </summary>
    public List<GitCommitInfo> VersionHistory { get; set; } = [];

    /// <summary>
    /// Returns a string representation of the Git information.
    /// </summary>
    public override string ToString()
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(CurrentVersion))
            parts.Add($"v{CurrentVersion}");

        if (!string.IsNullOrEmpty(CommitHash))
            parts.Add($"({CommitHash[..7]})");

        if (!string.IsNullOrEmpty(Owner) && !string.IsNullOrEmpty(Repo))
            parts.Add($"{Owner}/{Repo}");

        return string.Join(" | ", parts);
    }
}
