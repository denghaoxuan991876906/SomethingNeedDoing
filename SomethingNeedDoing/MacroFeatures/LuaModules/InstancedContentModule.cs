using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentOceanFishing;

namespace SomethingNeedDoing.MacroFeatures.LuaModules;
/// <summary>
/// Module for deep dungeons and forays
/// </summary>
public unsafe class InstancedContentModule : LuaModuleBase
{
    public override string ModuleName => "InstancedContent";

    [LuaFunction] public float ContentTimeLeft => EventFramework.Instance()->GetInstanceContentDirector()->ContentDirector.ContentTimeLeft;

    [LuaFunction] public OceanFishingWrapper OceanFishing => new();
    public class OceanFishingWrapper
    {
        public uint CurrentRoute => EventFramework.Instance()->GetInstanceContentOceanFishing()->CurrentRoute;
        public byte GetCurrentOceanFishingTimeOfDay() => Svc.Data.GetExcelSheet<IKDRoute>()?.GetRow(CurrentRoute).Time[GetCurrentOceanFishingZone].Value.Unknown0 ?? 0;
        public OceanFishingStatus GetCurrentOceanFishingStatus() => EventFramework.Instance()->GetInstanceContentOceanFishing()->Status;
        public int GetCurrentOceanFishingZone => (int)EventFramework.Instance()->GetInstanceContentOceanFishing()->CurrentZone;
        public float TimeLeft => EventFramework.Instance()->GetInstanceContentDirector()->ContentDirector.ContentTimeLeft - TimeOffset;
        public uint TimeOffset => EventFramework.Instance()->GetInstanceContentOceanFishing()->TimeOffset;
        public uint WeatherId => EventFramework.Instance()->GetInstanceContentOceanFishing()->WeatherId;
        public bool SpectralCurrentActive => EventFramework.Instance()->GetInstanceContentOceanFishing()->SpectralCurrentActive;
        public uint Mission1Type => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission1Type;
        public uint Mission2Type => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission2Type;
        public uint Mission3Type => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission3Type;
        public byte Mission1Goal => GetRow<IKDPlayerMissionCondition>(Mission1Type)!.Value.Unknown1;
        public byte Mission2Goal => GetRow<IKDPlayerMissionCondition>(Mission2Type)!.Value.Unknown1;
        public byte Mission3Goal => GetRow<IKDPlayerMissionCondition>(Mission3Type)!.Value.Unknown1;
        public uint Mission1Progress => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission1Progress;
        public uint Mission2Progress => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission2Progress;
        public uint Mission3Progress => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission3Progress;
        public uint Points => AgentModule.Instance()->GetAgentIKDFishingLog()->Points;
        public uint Score => AgentModule.Instance()->GetAgentIKDResult()->Data->Score;
        public uint TotalScore => AgentModule.Instance()->GetAgentIKDResult()->Data->TotalScore;
    }
}
