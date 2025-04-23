using AutoRetainerAPI;
using SomethingNeedDoing.MacroFeatures.IPC;

namespace SomethingNeedDoing;
public class Service
{
    public static AutoRetainerApi AutoRetainerApi { get; set; } = null!;
    public static MacroScheduler MacroScheduler { get; set; } = null!;
    public static Tippy Tippy { get; set; } = null!;
}
