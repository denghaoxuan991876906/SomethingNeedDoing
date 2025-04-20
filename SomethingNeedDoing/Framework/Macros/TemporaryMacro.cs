using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;

/// <summary>
/// Represents a temporary macro that is not persisted in configuration.
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

    /// <inheritdoc/>
    public override IReadOnlyList<IMacroCommand> Commands { get; set; } = [];

    /// <inheritdoc/>
    protected override async Task ExecuteMacro(TriggerEventArgs? args)
    {
        // TODO: Implement macro execution logic
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override async Task StopMacro()
    {
        // TODO: Implement macro stop logic
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override async Task PauseMacro()
    {
        // TODO: Implement macro pause logic
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override async Task ResumeMacro()
    {
        // TODO: Implement macro resume logic
        await Task.CompletedTask;
    }
}
