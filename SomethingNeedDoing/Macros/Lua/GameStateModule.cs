using FFXIVClientStructs.FFXIV.Client.UI;

namespace SomethingNeedDoing.Macros.Lua;
public class GameStateModule : LuaModuleBase
{
    public override string ModuleName => "Game";

    [LuaFunction]
    public bool IsCrafting() => Svc.Condition[ConditionFlag.Crafting];

    [LuaFunction]
    public uint GetCurrentProgress()
    {
        unsafe
        {
            var addon = (AddonSynthesis*)Svc.GameGui.GetAddonByName("Synthesis", 1);
            if (addon == null) return 0;
            return uint.Parse(addon->CurrentProgress->NodeText.ToString());
        }
    }

    [LuaFunction]
    public void LogInfo(object message) => Svc.Log.Info(message.ToString() ?? string.Empty);
}
