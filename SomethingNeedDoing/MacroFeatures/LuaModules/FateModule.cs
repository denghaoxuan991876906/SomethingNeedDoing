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

    [LuaFunction] public FateWrapper? CurrentFate => new(Fm->CurrentFate->FateId);
    [LuaFunction] public FateWrapper? GetFateById(ushort fateID) => new(fateID);

    [LuaFunction]
    public FateWrapper? GetNearestFate() => Fm->Fates.Where(f => f.Value is not null)
        .OrderBy(f => Player.DistanceTo(f.Value->Location))
        .Select(f => new FateWrapper(f.Value->FateId))
        .FirstOrDefault();

    [LuaFunction]
    public unsafe List<FateWrapper> GetActiveFates() => Fm->Fates.Where(f => f.Value is not null)
        .OrderBy(f => Player.DistanceTo(f.Value->Location))
        .Select(f => new FateWrapper(f.Value->FateId))
        .ToList();

    public class FateWrapper(ushort Id)
    {
        private FateContext* Fate => FateManager.Instance()->GetFateById(Id);

        public static implicit operator FateWrapper?(ushort id)
        {
            var wrapper = new FateWrapper(id);
            return wrapper.Exists ? wrapper : null;
        }

        public bool Exists => Fate != null;
        public bool InFate => FateManager.Instance()->CurrentFate->FateId == Id;
        public FateState State => Fate->State;
        public int StartTimeEpoch => Fate->StartTimeEpoch;
        public float Duration => Fate->Duration;
        public string Name => Fate->Name.ToString();
        public float HandInCount => Fate->HandInCount;
        public Vector3 Location => Fate->Location;
        public float Progress => Fate->Progress;
        public bool IsBonus => Fate->IsBonus;
        public float Radius => Fate->Radius;
        public FateRule Rule => (FateRule)Fate->Rule;
        public int Level => Fate->Level;
        public int MaxLevel => Fate->MaxLevel;
        public ushort FATEChain => Fate->FATEChain;
        public uint EventItem => Fate->EventItem;
        public float DistanceToPlayer => Player.DistanceTo(Location);
    }
}
