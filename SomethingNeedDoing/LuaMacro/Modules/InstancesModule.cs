using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.LuaMacro.Wrappers;
using static FFXIVClientStructs.FFXIV.Client.UI.Info.InfoProxyCommonList.CharacterData;

namespace SomethingNeedDoing.LuaMacro.Modules;
public unsafe class InstancesModule : LuaModuleBase
{
    public override string ModuleName => "Instances";

    [LuaFunction] public DutyFinderWrapper DutyFinder => new();
    public unsafe class DutyFinderWrapper : IWrapper
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
    public class FriendsListWrapper : IWrapper
    {
        [LuaDocs]
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

        [LuaDocs] public FriendWrapper? GetFriendByName(string name) => Friends.FirstOrDefault(f => f.Name == name);
    }

    public class FriendWrapper(InfoProxyCommonList.CharacterData data) : IWrapper
    {
        [LuaDocs] public string Name => data.NameString;
        [LuaDocs] public ulong ContentId => data.ContentId;
        [LuaDocs] public OnlineStatus State => data.State;
        [LuaDocs] public bool IsOtherServer => data.IsOtherServer;
        [LuaDocs] public ushort CurrentWorld => data.CurrentWorld;
        [LuaDocs] public ushort HomeWorld => data.HomeWorld;
        [LuaDocs] public ushort Location => data.Location;
        [LuaDocs] public GrandCompany GrandCompany => data.GrandCompany;
        [LuaDocs] public Language ClientLanguage => data.ClientLanguage;
        [LuaDocs] public byte Sex => data.Sex;
        [LuaDocs] public JobWrapper Job => new(data.Job);
    }

    [LuaFunction]
    [Changelog("12.8")]
    public MapWrapper Map => new();
    public class MapWrapper : IWrapper
    {
        [LuaDocs]
        [Changelog("12.8")]
        public bool IsFlagMarkerSet => AgentMap.Instance()->IsFlagMarkerSet;

        [LuaDocs]
        [Changelog("12.8")]
        public FlagWrapper Flag => new(AgentMap.Instance()->FlagMapMarker);
    }

    public class FlagWrapper(FlagMapMarker data) : IWrapper
    {
        [LuaDocs]
        [Changelog("12.8")]
        public uint TerritoryId => data.TerritoryId;

        [LuaDocs]
        [Changelog("12.8")]
        public uint MapId => data.MapId;

        [LuaDocs]
        [Changelog("12.8")]
        public float XFloat => data.XFloat;

        [LuaDocs]
        [Changelog("12.8")]
        public float YFloat => data.YFloat;

        [LuaDocs]
        [Changelog("12.8")]
        public Vector2 Vector2 => new(XFloat, YFloat);

        [LuaDocs]
        [Changelog("12.8")]
        public Vector3 Vector3 => new(XFloat, 0, YFloat); // TODO use navmesh PointOnFloor
    }

    public class MapMarkerDataWrapper(MapMarkerData data) : IWrapper
    {
        [LuaDocs]
        [Changelog("12.8")]
        public uint LevelId => data.LevelId;

        [LuaDocs]
        [Changelog("12.8")]
        public uint ObjectiveId => data.ObjectiveId;

        [LuaDocs]
        [Changelog("12.8")]
        public string TooltipString => data.TooltipString->ToString();

        [LuaDocs]
        [Changelog("12.8")]
        public uint IconId => data.IconId;

        [LuaDocs]
        [Changelog("12.8")]
        public Vector3 Position => data.Position;

        [LuaDocs]
        [Changelog("12.8")]
        public float Radius => data.Radius;

        [LuaDocs]
        [Changelog("12.8")]
        public uint MapId => data.MapId;

        [LuaDocs]
        [Changelog("12.8")]
        public uint PlaceNameZoneId => data.PlaceNameZoneId;

        [LuaDocs]
        [Changelog("12.8")]
        public uint PlaceNameId => data.PlaceNameId;

        [LuaDocs]
        [Changelog("12.8")]
        public int EndTimestamp => data.EndTimestamp;

        [LuaDocs]
        [Changelog("12.8")]
        public ushort RecommendedLevel => data.RecommendedLevel;

        [LuaDocs]
        [Changelog("12.8")]
        public ushort TerritoryTypeId => data.TerritoryTypeId;

        [LuaDocs]
        [Changelog("12.8")]
        public ushort DataId => data.DataId;

        [LuaDocs]
        [Changelog("12.8")]
        public byte MarkerType => data.MarkerType;

        [LuaDocs]
        [Changelog("12.8")]
        public sbyte EventState => data.EventState;

        [LuaDocs]
        [Changelog("12.8")]
        public byte Flags => data.Flags;
    }
}
