using FFXIVClientStructs.FFXIV.Client.Game.Fate;

namespace SomethingNeedDoing.MacroFeatures.LuaModules;
public unsafe class FateModule : LuaModuleBase
{
    public override string ModuleName => "Fates";

    private FateManager* Fm => FateManager.Instance();

    public enum FateRule : byte
    {
        None = 0,
        Normal = 1, // trash fates or boss fates
        Collect = 2, // pick up EventObjects or get them from killing mobs
        Escort = 3, // guide some npc to the finish line
        Defend = 4, // defend objectives like crates from being destroyed
        EventFate = 5, // used for seasonal event fates, like Little Ladies Day, Hatching Tide
        Chase = 6, // that one special fate in The Peaks
        ConcertedWorks = 7, // rebuilding the firmament fates
        Fete = 8, // firmament fates
    }

    [LuaFunction] public FateWrapper? CurrentFate => Fm->CurrentFate != null ? new(Fm->CurrentFate->FateId) : null;
    [LuaFunction] public FateWrapper? GetFateById(ushort fateID) => new(fateID);

    [LuaFunction]
    public FateWrapper? GetNearestFate() => Fm->Fates.Where(f => f.Value is not null)
        .OrderBy(f => Player.DistanceTo(f.Value->Location))
        .Select(f => new FateWrapper(f.Value->FateId))
        .FirstOrDefault();

    [LuaFunction]
    public unsafe List<FateWrapper> GetActiveFates() => [.. Fm->Fates.Where(f => f.Value is not null)
        .OrderBy(f => Player.DistanceTo(f.Value->Location))
        .Select(f => new FateWrapper(f.Value->FateId))];

    public class FateWrapper(ushort Id)
    {
        private FateContext* Fate => FateManager.Instance()->GetFateById(Id);

        [LuaWrapper] public bool Exists => Fate != null;
        [LuaWrapper] public bool InFate => FateManager.Instance()->CurrentFate->FateId == Id;
        [LuaWrapper] public FateState State => Fate->State;
        [LuaWrapper] public int StartTimeEpoch => Fate->StartTimeEpoch;
        [LuaWrapper] public float Duration => Fate->Duration;
        [LuaWrapper] public string Name => Fate->Name.ToString();
        [LuaWrapper] public float HandInCount => Fate->HandInCount;
        [LuaWrapper] public Vector3 Location => Fate->Location;
        [LuaWrapper] public float Progress => Fate->Progress;
        [LuaWrapper] public bool IsBonus => Fate->IsBonus;
        [LuaWrapper] public float Radius => Fate->Radius;
        [LuaWrapper] public FateRule Rule => (FateRule)Fate->Rule;
        [LuaWrapper] public int Level => Fate->Level;
        [LuaWrapper] public int MaxLevel => Fate->MaxLevel;
        [LuaWrapper] public ushort FATEChain => Fate->FATEChain;
        [LuaWrapper] public uint EventItem => Fate->EventItem;
        [LuaWrapper] public float DistanceToPlayer => Player.DistanceTo(Location);
    }
}
