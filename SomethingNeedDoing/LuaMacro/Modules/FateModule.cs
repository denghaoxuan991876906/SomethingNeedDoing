using FFXIVClientStructs.FFXIV.Client.Game.Fate;

namespace SomethingNeedDoing.LuaMacro.Modules;
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

        [LuaDocs] public bool Exists => Fate != null;
        [LuaDocs] public bool InFate => FateManager.Instance()->CurrentFate->FateId == Id;
        [LuaDocs] public FateState State => Fate->State;
        [LuaDocs] public int StartTimeEpoch => Fate->StartTimeEpoch;
        [LuaDocs] public float Duration => Fate->Duration;
        [LuaDocs] public string Name => Fate->Name.ToString();
        [LuaDocs] public float HandInCount => Fate->HandInCount;
        [LuaDocs] public Vector3 Location => Fate->Location;
        [LuaDocs] public float Progress => Fate->Progress;
        [LuaDocs] public bool IsBonus => Fate->IsBonus;
        [LuaDocs] public float Radius => Fate->Radius;
        [LuaDocs] public FateRule Rule => (FateRule)Fate->Rule;
        [LuaDocs] public int Level => Fate->Level;
        [LuaDocs] public int MaxLevel => Fate->MaxLevel;
        [LuaDocs] public ushort FATEChain => Fate->FATEChain;
        [LuaDocs] public uint EventItem => Fate->EventItem;
        [LuaDocs] public float DistanceToPlayer => Player.DistanceTo(Location);
    }
}
