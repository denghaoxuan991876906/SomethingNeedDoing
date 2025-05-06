using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.Sheets;

namespace SomethingNeedDoing.MacroFeatures.LuaModules;
public unsafe class PlayerModule : LuaModuleBase
{
    public override string ModuleName => "Player";

    private PlayerState* Ps => PlayerState.Instance();

    [LuaFunction] public byte GrandCompany => Ps->GrandCompany;
    [LuaFunction] public byte GCRankMaelstrom { get => Ps->GCRankMaelstrom; set => Ps->GCRankMaelstrom = value; }
    [LuaFunction] public byte GCRankImmortalFlames { get => Ps->GCRankImmortalFlames; set => Ps->GCRankImmortalFlames = value; }
    [LuaFunction] public byte GCRankTwinAdders { get => Ps->GCRankTwinAdders; set => Ps->GCRankTwinAdders = value; }

    [LuaFunction] public FreeCompanyWrapper FreeCompany => new();
    public unsafe class FreeCompanyWrapper
    {
        private InfoProxyFreeCompany* FreeCompanyProxy => (InfoProxyFreeCompany*)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->UIModule->GetInfoModule()->GetInfoProxyById(InfoProxyId.FreeCompany);

        [LuaWrapper] public FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany GrandCompany => FreeCompanyProxy->GrandCompany;
        [LuaWrapper] public byte Rank => FreeCompanyProxy->Rank;
        [LuaWrapper] public int OnlineMemebers => FreeCompanyProxy->OnlineMembers;
        [LuaWrapper] public int TotalMembers => FreeCompanyProxy->TotalMembers;
        [LuaWrapper] public string Name => FreeCompanyProxy->Name.ToString();
        [LuaWrapper] public ulong Id => FreeCompanyProxy->Id;
    }

    [LuaFunction] public JobWrapper Job => new(Player.JobId);
    [LuaFunction] public JobWrapper GetJob(uint classJobId) => new(classJobId);
    public class JobWrapper(uint classJobId)
    {
        [LuaWrapper] public uint Id => classJobId;
        [LuaWrapper] public string Name => GetRow<ClassJob>(Id)?.Name.ToString() ?? string.Empty;
        [LuaWrapper] public string Abbreviation => GetRow<ClassJob>(Id)?.Abbreviation.ToString() ?? string.Empty;
        [LuaWrapper] public bool IsCrafter => Id is >= 8 and <= 15;
        [LuaWrapper] public bool IsGatherer => Id is >= 16 and <= 18;
        [LuaWrapper] public bool IsMeleeDPS => Id is 2 or 4 or 20 or 22 or 29 or 30 or 34 or 39;
        [LuaWrapper] public bool IsRangedDPS => Id is 5 or 23 or 31 or 38;
        [LuaWrapper] public bool IsMagicDPS => Id is 7 or 25 or 26 or 27 or 35;
        [LuaWrapper] public bool IsHealer => Id is 6 or 24 or 28 or 33 or 40;
        [LuaWrapper] public bool IsTank => Id is 3 or 19 or 21 or 32 or 37;
        [LuaWrapper] public bool IsDPS => IsMeleeDPS || IsRangedDPS || IsMagicDPS;
        [LuaWrapper] public bool IsDiscipleOfWar => IsMeleeDPS || IsRangedDPS || IsTank;
        [LuaWrapper] public bool IsDiscipleOfMagic => IsMagicDPS || IsHealer;
        [LuaWrapper] public bool IsBlu => Id is 36;
        [LuaWrapper] public bool IsLimited => IsBlu;
        [LuaWrapper(description: "Current unsynced level")] public int Level => Player.GetUnsyncedLevel((Job)Id);
    }
}
