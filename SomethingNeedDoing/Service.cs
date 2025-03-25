using AutoRetainerAPI;
using SomethingNeedDoing.IPC;
using SomethingNeedDoing.Macros;
using SomethingNeedDoing.Managers;

namespace SomethingNeedDoing;

internal class Service
{
    internal static AutoRetainerApi AutoRetainerApi { get; set; } = null!;
    internal static ChatManager ChatManager { get; set; } = null!;
    internal static GameEventManager GameEventManager { get; set; } = null!;
    internal static MacroManager MacroManager { get; set; } = null!;
    internal static Tippy Tippy { get; set; } = null!;
}
