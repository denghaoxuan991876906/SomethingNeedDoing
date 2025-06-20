using FFXIVClientStructs.FFXIV.Client.Game.UI;
using NLua;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.LuaMacro.Wrappers;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.PlayerState;

namespace SomethingNeedDoing.LuaMacro.Modules;
public unsafe class PlayerModule : LuaModuleBase
{
    public override string ModuleName => "Player";
    public override void Register(Lua lua)
    {
        lua.DoString("WeeklyBingoTaskStatus = luanet.import_type('FFXIVClientStructs.FFXIV.Client.Game.UI.PlayerState+WeeklyBingoTaskStatus')");
        base.Register(lua);
    }

    private PlayerState* Ps => Instance();

    [LuaFunction] public byte GrandCompany => Ps->GrandCompany;
    [LuaFunction] public byte GCRankMaelstrom { get => Ps->GCRankMaelstrom; set => Ps->GCRankMaelstrom = value; }
    [LuaFunction] public byte GCRankImmortalFlames { get => Ps->GCRankImmortalFlames; set => Ps->GCRankImmortalFlames = value; }
    [LuaFunction] public byte GCRankTwinAdders { get => Ps->GCRankTwinAdders; set => Ps->GCRankTwinAdders = value; }

    [LuaFunction] public uint FishingBait => Ps->FishingBait;

    [LuaFunction] public EntityWrapper Entity => new(Player.Object);
    [LuaFunction] public FreeCompanyWrapper FreeCompany => new();

    [LuaFunction] public JobWrapper Job => new(Player.JobId);
    [LuaFunction] public JobWrapper GetJob(uint classJobId) => new(classJobId);

    [LuaFunction] public bool IsMoving => Player.IsMoving;
    [LuaFunction] public bool IsInDuty => Player.IsInDuty;
    [LuaFunction] public bool IsOnIsland => Player.IsOnIsland;
    [LuaFunction] public bool CanMount => Player.CanMount;
    [LuaFunction] public bool CanFly => Player.CanFly;
    [LuaFunction] public bool Revivable => Player.Revivable;
    [LuaFunction] public bool Available => Player.Available;

    [LuaFunction]
    [Changelog("12.8")]
    public bool IsBusy => Player.IsBusy;

    [LuaFunction][Changelog("12.12")] public BingoWrapper Bingo => new(this);
    public class BingoWrapper(PlayerModule parentModule) : IWrapper
    {
        private PlayerState* Ps => Instance();

        [LuaDocs] public bool HasWeeklyBingoJournal => Ps->HasWeeklyBingoJournal;
        [LuaDocs] public bool IsWeeklyBingoExpired => Ps->IsWeeklyBingoExpired();
        [LuaDocs] public uint WeeklyBingoNumSecondChancePoints => Ps->WeeklyBingoNumSecondChancePoints;
        [LuaDocs] public int WeeklyBingoNumPlacedStickers => Ps->WeeklyBingoNumPlacedStickers;
        [LuaDocs] public object? GetWeeklyBingoOrderDataRow(int wonderousTailsIndex) => parentModule.GetModule<ExcelModule>()?.GetRow("WeeklyBingoOrderData", Ps->WeeklyBingoOrderData[wonderousTailsIndex]);
        [LuaDocs] public WeeklyBingoTaskStatus GetWeeklyBingoTaskStatus(int wonderousTailsIndex) => Ps->GetWeeklyBingoTaskStatus(wonderousTailsIndex);
    }
}
