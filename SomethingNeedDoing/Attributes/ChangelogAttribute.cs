namespace SomethingNeedDoing.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ChangelogAttribute(string version, params string[] changes) : Attribute
{
    public string Version { get; } = version;
    public string[] Changes { get; } = changes;
}
