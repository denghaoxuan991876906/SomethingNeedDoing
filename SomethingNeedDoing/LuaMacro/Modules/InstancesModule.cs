using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using SomethingNeedDoing.LuaMacro.Wrappers;
using static FFXIVClientStructs.FFXIV.Client.UI.Info.InfoProxyCommonList.CharacterData;

namespace SomethingNeedDoing.LuaMacro.Modules;
public unsafe class InstancesModule : LuaModuleBase
{
    public override string ModuleName => "Instances";

    [LuaFunction] public DutyFinderWrapper DutyFinder => new();
    public unsafe class DutyFinderWrapper
    {
        [LuaDocs] public void OpenRouletteDuty(byte contentRouletteID) => AgentContentsFinder.Instance()->OpenRouletteDuty(contentRouletteID);
        [LuaDocs] public void OpenRegularDuty(uint contentsFinderCondition) => AgentContentsFinder.Instance()->OpenRegularDuty(contentsFinderCondition);

        [LuaDocs] public bool IsUnrestrictedParty { get => ContentsFinder.Instance()->IsUnrestrictedParty; set => ContentsFinder.Instance()->IsUnrestrictedParty = value; }
        [LuaDocs] public bool IsLevelSync { get => ContentsFinder.Instance()->IsLevelSync; set => ContentsFinder.Instance()->IsLevelSync = value; }
        [LuaDocs] public bool IsMinIL { get => ContentsFinder.Instance()->IsMinimalIL; set => ContentsFinder.Instance()->IsMinimalIL = value; }
        [LuaDocs] public bool IsSilenceEcho { get => ContentsFinder.Instance()->IsSilenceEcho; set => ContentsFinder.Instance()->IsSilenceEcho = value; }
        [LuaDocs] public bool IsExplorerMode { get => ContentsFinder.Instance()->IsExplorerMode; set => ContentsFinder.Instance()->IsExplorerMode = value; }
        [LuaDocs] public bool IsLimitedLevelingRoulette { get => ContentsFinder.Instance()->IsLimitedLevelingRoulette; set => ContentsFinder.Instance()->IsLimitedLevelingRoulette = value; }
    }

    [LuaFunction] public FriendsListWrapper FriendsList => new();
    public class FriendsListWrapper
    {
        public List<FriendWrapper> Friends
        {
            get
            {
                var friends = new List<FriendWrapper>();
                for (var i = 0; i < AgentFriendlist.Instance()->InfoProxy->CharDataSpan.Length; i++)
                    friends.Add(new(AgentFriendlist.Instance()->InfoProxy->CharDataSpan[i]));
                return friends;
            }
        }

        public FriendWrapper? GetFriendByName(string name) => Friends.FirstOrDefault(f => f.Name == name);
    }

    public class FriendWrapper(InfoProxyCommonList.CharacterData data)
    {
        public string Name => data.NameString;
        public ulong ContentId => data.ContentId;
        public OnlineStatus State => data.State;
        public bool IsOtherServer => data.IsOtherServer;
        public ushort CurrentWorld => data.CurrentWorld;
        public ushort HomeWorld => data.HomeWorld;
        public ushort Location => data.Location;
        public GrandCompany GrandCompany => data.GrandCompany;
        public Language ClientLanguage => data.ClientLanguage;
        public byte Sex => data.Sex;
        public JobWrapper Job => new(data.Job);
    }
}
