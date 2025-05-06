using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace SomethingNeedDoing.LuaMacro.Modules;
/// <summary>
/// Module for executing actions or info about actions
/// </summary>
public unsafe class ActionsModule : LuaModuleBase
{
    public override string ModuleName => "Actions";

    [LuaFunction] public void ExecuteAction(uint actionID, ActionType actionType = ActionType.Action) => ActionManager.Instance()->UseAction(actionType, actionID);
    [LuaFunction] public void ExecuteGeneralAction(uint actionID) => ActionManager.Instance()->UseAction(ActionType.GeneralAction, actionID);
    [LuaFunction] public void Teleport(uint aetheryteId) => Telepo.Instance()->Teleport(aetheryteId, 0);

    [LuaFunction] public ActionWrapper GetActionInfo(uint actionId) => new(actionId);
    public class ActionWrapper(uint actionId)
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
}
