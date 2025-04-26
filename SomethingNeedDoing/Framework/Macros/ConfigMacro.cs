namespace SomethingNeedDoing.Framework;
/// <summary>
/// Represents a macro that is stored in the config.
/// </summary>
public class ConfigMacro : MacroBase
{
    /// <inheritdoc/>
    public override string Id { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public override string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override MacroType Type { get; set; }

    /// <inheritdoc/>
    public override string Content { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override MacroMetadata Metadata { get; set; } = new();

    /// <inheritdoc/>
    public override IReadOnlyList<IMacroCommand> Commands { get; set; } = [];

    /// <summary>
    /// Gets or sets the folder path of the macro.
    /// </summary>
    public string FolderPath { get; set; } = string.Empty;
}
