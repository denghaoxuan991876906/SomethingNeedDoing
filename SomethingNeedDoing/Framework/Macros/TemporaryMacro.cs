namespace SomethingNeedDoing.Framework;
/// <summary>
/// A temporary macro that is not persisted in configuration.
/// </summary>
public class TemporaryMacro(string content) : IMacro
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; } = "Temporary Macro";
    public MacroType Type { get; } = MacroType.Native;
    public string Content { get; } = content;
    public MacroState State => MacroState.Ready;
    public IReadOnlyList<IMacroCommand> Commands { get; } = [];
}
