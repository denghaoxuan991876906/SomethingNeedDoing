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
        [LuaWrapper] public uint CurrentRoute => EventFramework.Instance()->GetInstanceContentOceanFishing()->CurrentRoute;
        [LuaWrapper] public byte TimeOfDaty => Svc.Data.GetExcelSheet<IKDRoute>()?.GetRow(CurrentRoute).Time[GetCurrentOceanFishingZone].Value.Unknown0 ?? 0;
        [LuaWrapper] public OceanFishingStatus Status => EventFramework.Instance()->GetInstanceContentOceanFishing()->Status;
        [LuaWrapper] public int GetCurrentOceanFishingZone => (int)EventFramework.Instance()->GetInstanceContentOceanFishing()->CurrentZone;
        [LuaWrapper] public float TimeLeft => EventFramework.Instance()->GetInstanceContentDirector()->ContentDirector.ContentTimeLeft - TimeOffset;
        [LuaWrapper] public uint TimeOffset => EventFramework.Instance()->GetInstanceContentOceanFishing()->TimeOffset;
        [LuaWrapper] public uint WeatherId => EventFramework.Instance()->GetInstanceContentOceanFishing()->WeatherId;
        [LuaWrapper] public bool SpectralCurrentActive => EventFramework.Instance()->GetInstanceContentOceanFishing()->SpectralCurrentActive;
        [LuaWrapper] public uint Mission1Type => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission1Type;
        [LuaWrapper] public uint Mission2Type => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission2Type;
        [LuaWrapper] public uint Mission3Type => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission3Type;
        [LuaWrapper] public byte Mission1Goal => GetRow<IKDPlayerMissionCondition>(Mission1Type)!.Value.Unknown1;
        [LuaWrapper] public byte Mission2Goal => GetRow<IKDPlayerMissionCondition>(Mission2Type)!.Value.Unknown1;
        [LuaWrapper] public byte Mission3Goal => GetRow<IKDPlayerMissionCondition>(Mission3Type)!.Value.Unknown1;
        [LuaWrapper] public uint Mission1Progress => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission1Progress;
        [LuaWrapper] public uint Mission2Progress => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission2Progress;
        [LuaWrapper] public uint Mission3Progress => EventFramework.Instance()->GetInstanceContentOceanFishing()->Mission3Progress;
        [LuaWrapper] public uint Points => AgentModule.Instance()->GetAgentIKDFishingLog()->Points;
        [LuaWrapper] public uint Score => AgentModule.Instance()->GetAgentIKDResult()->Data->Score;
        [LuaWrapper] public uint TotalScore => AgentModule.Instance()->GetAgentIKDResult()->Data->TotalScore;
    }
}
