using SomethingNeedDoing.Framework.Interfaces;

namespace SomethingNeedDoing.Framework;

/// <summary>
/// Represents a temporary macro that is not persisted in configuration.
/// This class is used for one-off macros that don't need to be saved to the config file.
/// It shares the same execution logic as ConfigMacro but differs in persistence.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TemporaryMacro"/> class.
/// </remarks>
/// <param name="content">The macro content.</param>
public class TemporaryMacro(string content) : MacroBase
{
    /// <inheritdoc/>
    public override string Id { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public override string Name { get; set; } = "Temporary Macro";

    /// <inheritdoc/>
    public override MacroType Type { get; set; } = MacroType.Native;

    /// <inheritdoc/>
    public override string Content { get; set; } = content;

    /// <inheritdoc/>
    public override MacroMetadata Metadata { get; set; } = new();
}
