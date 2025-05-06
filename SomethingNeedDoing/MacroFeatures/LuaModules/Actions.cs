using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace SomethingNeedDoing.MacroFeatures.LuaModules;
/// <summary>
/// Module for executing actions or info about actions
/// </summary>
public unsafe class Actions : LuaModuleBase
{
    public override string ModuleName => "Actions";

    [LuaFunction] public void ExecuteAction(uint actionID) => ActionManager.Instance()->UseAction(ActionType.Action, actionID);
    [LuaFunction] public void ExecuteGeneralAction(uint actionID) => ActionManager.Instance()->UseAction(ActionType.GeneralAction, actionID);
    [LuaFunction] public void Teleport(uint aetheryteId) => Telepo.Instance()->Teleport(aetheryteId, 0);

    [LuaFunction] public ActionWrapper GetActionInfo(uint actionId) => new(actionId);
    public class ActionWrapper(uint actionId)
    {
        private ActionManager* Am => ActionManager.Instance();

        [LuaWrapper] public uint AdjustedActionId => Am->GetAdjustedActionId(actionId);
        [LuaWrapper] public float RecastTimeElapsed => Am->GetRecastTimeElapsed(ActionType.Action, AdjustedActionId);
        [LuaWrapper] public float RealRecastTimeElapsed => Am->GetRecastTimeElapsed(ActionType.Action, actionId);
        [LuaWrapper] public float RecastTime => Am->GetRecastTime(ActionType.Action, AdjustedActionId);
        [LuaWrapper] public float RealRecastTime => Am->GetRecastTime(ActionType.Action, actionId);
        [LuaWrapper] public float SpellCooldown => Math.Abs(RecastTime - RecastTimeElapsed);
        [LuaWrapper] public float RealSpellCooldown => Math.Abs(RealRecastTime - RealRecastTimeElapsed);
    }
}
