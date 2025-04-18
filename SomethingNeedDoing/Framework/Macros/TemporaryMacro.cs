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
    public IReadOnlyList<IMacroCommand> Commands { get; } = [];

    /// <summary>
    /// Gets or sets the current state of the macro.
    /// </summary>
    public MacroState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                var oldState = _state;
                _state = value;
                StateChanged?.Invoke(this, new MacroStateChangedEventArgs(Id, value, oldState));
            }
        }
    }

    /// <summary>
    /// Event raised when the macro's state changes.
    /// </summary>
    public event EventHandler<MacroStateChangedEventArgs>? StateChanged;

    private MacroState _state = MacroState.Ready;
}
