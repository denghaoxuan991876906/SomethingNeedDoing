using FFXIVClientStructs.FFXIV.Client.Game.UI;
using SomethingNeedDoing.LuaMacro.Wrappers;

namespace SomethingNeedDoing.LuaMacro.Modules;
public unsafe class PlayerModule : LuaModuleBase
{
    public override string ModuleName => "Player";

    private PlayerState* Ps => PlayerState.Instance();

    [LuaFunction] public byte GrandCompany => Ps->GrandCompany;
    [LuaFunction] public byte GCRankMaelstrom { get => Ps->GCRankMaelstrom; set => Ps->GCRankMaelstrom = value; }
    [LuaFunction] public byte GCRankImmortalFlames { get => Ps->GCRankImmortalFlames; set => Ps->GCRankImmortalFlames = value; }
    [LuaFunction] public byte GCRankTwinAdders { get => Ps->GCRankTwinAdders; set => Ps->GCRankTwinAdders = value; }

    [LuaFunction] public uint FishingBait => Ps->FishingBait;

    [LuaFunction] public FreeCompanyWrapper FreeCompany => new();

    [LuaFunction] public JobWrapper Job => new(Player.JobId);
    [LuaFunction] public JobWrapper GetJob(uint classJobId) => new(classJobId);

    [LuaFunction] public bool IsMoving => Player.IsMoving;
    [LuaFunction] public bool IsInDuty => Player.IsInDuty;
    [LuaFunction] public bool IsOnIsland => Player.IsOnIsland;
    [LuaFunction] public bool CanMount => Player.CanMount;
    [LuaFunction] public bool CanFly => Player.CanFly;
    [LuaFunction] public bool Revivable => Player.Revivable;
}
