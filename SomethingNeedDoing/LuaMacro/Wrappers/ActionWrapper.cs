using FFXIVClientStructs.FFXIV.Client.Game;

namespace SomethingNeedDoing.LuaMacro.Wrappers;
public unsafe class ActionWrapper(uint actionId)
{
    private ActionManager* Am => ActionManager.Instance();

    [LuaDocs] public uint AdjustedActionId => Am->GetAdjustedActionId(actionId);
    [LuaDocs] public float RecastTimeElapsed => Am->GetRecastTimeElapsed(ActionType.Action, AdjustedActionId);
    [LuaDocs] public float RealRecastTimeElapsed => Am->GetRecastTimeElapsed(ActionType.Action, actionId);
    [LuaDocs] public float RecastTime => Am->GetRecastTime(ActionType.Action, AdjustedActionId);
    [LuaDocs] public float RealRecastTime => Am->GetRecastTime(ActionType.Action, actionId);
    [LuaDocs] public float SpellCooldown => Math.Abs(RecastTime - RecastTimeElapsed);
    [LuaDocs] public float RealSpellCooldown => Math.Abs(RealRecastTime - RealRecastTimeElapsed);
}
