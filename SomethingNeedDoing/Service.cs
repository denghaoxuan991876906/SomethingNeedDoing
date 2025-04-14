using AutoRetainerAPI;
using SomethingNeedDoing.Old.IPC;

namespace SomethingNeedDoing;
public class Service
{
    public static AutoRetainerApi AutoRetainerApi { get; set; } = null!;
    public static MacroScheduler MacroScheduler { get; set; } = null!;
    public static Tippy Tippy { get; set; } = null!;
}
