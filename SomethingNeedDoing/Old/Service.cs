using AutoRetainerAPI;
using SomethingNeedDoing.Managers;
using SomethingNeedDoing.Old.IPC;
using SomethingNeedDoing.Old.Managers;

namespace SomethingNeedDoing.Old;

internal class Service
{
    internal static AutoRetainerApi AutoRetainerApi { get; set; } = null!;
    internal static ChatManager ChatManager { get; set; } = null!;
    internal static GameEventManager GameEventManager { get; set; } = null!;
    internal static MacroManager MacroManager { get; set; } = null!;
    internal static Tippy Tippy { get; set; } = null!;
}
