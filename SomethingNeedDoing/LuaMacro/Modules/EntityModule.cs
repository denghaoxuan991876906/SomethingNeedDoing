using Dalamud.Game.ClientState.Objects.SubKinds;
using SomethingNeedDoing.LuaMacro.Wrappers;

namespace SomethingNeedDoing.LuaMacro.Modules;
public unsafe class EntityModule : LuaModuleBase
{
    public override string ModuleName => "Entity";

    [LuaFunction] public EntityWrapper? Target => Svc.Targets.Target is { } target ? new(target) : null;
    [LuaFunction] public EntityWrapper? FocusTarget => Svc.Targets.FocusTarget is { } target ? new(target) : null;
    [LuaFunction] public EntityWrapper? NearestDeadCharacter => Svc.Objects.OfType<IPlayerCharacter>().OrderBy(Player.DistanceTo).FirstOrDefault(o => o.IsDead) is { } obj ? new(obj) : null;
    [LuaFunction] public EntityWrapper? GetPartyMemeber(int index) => Svc.Party.GetPartyMemberAddress(index) is { } member ? new(member) : null;
    [LuaFunction] public EntityWrapper? GetAllianceMember(int index) => Svc.Party.GetAllianceMemberAddress(index) is { } member ? new(member) : null;
    [LuaFunction] public EntityWrapper? GetEntityByName(string name) => Svc.Objects.FirstOrDefault(o => o.Name.TextValue.Equals(name, StringComparison.InvariantCultureIgnoreCase)) is { } obj ? new(obj) : null;
}
