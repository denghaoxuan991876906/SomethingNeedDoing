using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace SomethingNeedDoing.LuaMacro.Wrappers;
public unsafe class LimitBreakWrapper
{
    [LuaDocs] public ushort CurrentUnits => UIState.Instance()->LimitBreakController.CurrentUnits;
    [LuaDocs] public uint BarUnits => UIState.Instance()->LimitBreakController.BarUnits;
    [LuaDocs] public byte BarCount => UIState.Instance()->LimitBreakController.BarCount;
}
