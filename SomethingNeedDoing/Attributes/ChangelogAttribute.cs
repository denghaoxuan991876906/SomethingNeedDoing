namespace SomethingNeedDoing.Attributes;

public enum ChangelogType
{
    Added,
    Fixed,
    Changed,
    Removed
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class ChangelogAttribute : Attribute
{
    public string Version { get; }
    public ChangelogType ChangeType { get; }
    public string? Description { get; }

    public ChangelogAttribute(string version)
    {
        Version = version;
        ChangeType = ChangelogType.Added;
    }

    public ChangelogAttribute(string version, ChangelogType changeType, string? description = null)
    {
        Version = version;
        ChangeType = changeType;
        Description = description;
    }
}
