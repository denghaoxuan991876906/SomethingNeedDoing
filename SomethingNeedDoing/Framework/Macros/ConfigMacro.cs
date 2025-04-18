namespace SomethingNeedDoing.Framework;
/// <summary>
/// Represents a macro stored in the configuration.
/// </summary>
public class ConfigMacro : IMacro
{
    /// <summary>
    /// Gets or sets the unique identifier of the macro.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the display name of the macro.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the macro.
    /// </summary>
    public MacroType Type { get; set; }

    /// <summary>
    /// Gets or sets the content of the macro.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder path of the macro.
    /// </summary>
    public string FolderPath { get; set; } = "/";

    /// <summary>
    /// Gets or sets the metadata for this macro.
    /// </summary>
    public MacroMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets the current state of the macro.
    /// </summary>
    public MacroState State { get; } = MacroState.Ready;

    /// <summary>
    /// Gets the commands that make up this macro.
    /// </summary>
    public IReadOnlyList<IMacroCommand> Commands { get; } = [];

    /// <summary>
    /// Updates the last modified timestamp.
    /// </summary>
    public void UpdateLastModified() => Metadata.LastModified = DateTime.Now;

}
