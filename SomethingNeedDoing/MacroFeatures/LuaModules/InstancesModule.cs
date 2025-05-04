using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace SomethingNeedDoing.MacroFeatures.LuaModules;
public unsafe class InstancesModule : LuaModuleBase
{
    public override string ModuleName => "Instances";

    [LuaFunction] public DutyFinderWrapper DutyFinder => new();
    public unsafe class DutyFinderWrapper
    {
        public void OpenRouletteDuty(byte contentRouletteID) => AgentContentsFinder.Instance()->OpenRouletteDuty(contentRouletteID);
        public void OpenRegularDuty(uint contentsFinderCondition) => AgentContentsFinder.Instance()->OpenRegularDuty(contentsFinderCondition);

        public bool IsUnrestrictedParty { get => ContentsFinder.Instance()->IsUnrestrictedParty; set => ContentsFinder.Instance()->IsUnrestrictedParty = value; }
        public bool IsLevelSync { get => ContentsFinder.Instance()->IsLevelSync; set => ContentsFinder.Instance()->IsLevelSync = value; }
        public bool IsMinIL { get => ContentsFinder.Instance()->IsMinimalIL; set => ContentsFinder.Instance()->IsMinimalIL = value; }
        public bool IsSilenceEcho { get => ContentsFinder.Instance()->IsSilenceEcho; set => ContentsFinder.Instance()->IsSilenceEcho = value; }
        public bool IsExplorerMode { get => ContentsFinder.Instance()->IsExplorerMode; set => ContentsFinder.Instance()->IsExplorerMode = value; }
        public bool IsLimitedLevelingRoulette { get => ContentsFinder.Instance()->IsLimitedLevelingRoulette; set => ContentsFinder.Instance()->IsLimitedLevelingRoulette = value; }
    }
}
