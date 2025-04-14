using ECommons.ChatMethods;

namespace SomethingNeedDoing.Old.Macros.Exceptions;

/// <summary>
/// Error thrown when a macro needs to pause, but not treat it like an error.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MacroPause"/> class.
/// </remarks>
/// <param name="command">The reason for stopping.</param>
/// <param name="color">SeString color.</param>
internal partial class MacroPause(string command, UIColor color) : InvalidOperationException($"Macro paused: {command}")
{
    public UIColor Color { get; } = color;
}
