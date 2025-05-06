using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel.Sheets;

namespace SomethingNeedDoing.MacroFeatures.LuaModules;
public unsafe class EntityModule : LuaModuleBase
{
    public override string ModuleName => "Entity";

    [LuaFunction] public EntityWrapper? Target => Svc.Targets.Target is { } target ? new(target) : null;
    [LuaFunction] public EntityWrapper? FocusTarget => Svc.Targets.FocusTarget is { } target ? new(target) : null;
    [LuaFunction] public EntityWrapper? GetPartyMemeber(int index) => Svc.Party.GetPartyMemberAddress(index) is { } member ? new(member) : null;
    [LuaFunction] public EntityWrapper? GetAllianceMember(int index) => Svc.Party.GetAllianceMemberAddress(index) is { } member ? new(member) : null;
    [LuaFunction] public EntityWrapper? GetEntityByName(string name) => Svc.Objects.FirstOrDefault(o => o.Name.TextValue.Equals(name, StringComparison.InvariantCultureIgnoreCase)) is { } obj ? new(obj) : null;

    public class EntityWrapper
    {
        public EntityWrapper(GameObject* obj) => _obj = obj;
        public EntityWrapper(nint obj) => _obj = (GameObject*)obj;
        public EntityWrapper(IGameObject obj) => _obj = (GameObject*)obj.Address;
        public EntityWrapper(IPartyMember obj) => _obj = (GameObject*)obj.Address;

        private readonly GameObject* _obj;
        private IGameObject? Dalamud => Svc.Objects.CreateObjectReference((nint)_obj);
        private Character* Character => Type == ObjectKind.Pc ? (Character*)_obj : null;
        private BattleChara* BattleChara => Type == ObjectKind.BattleNpc ? (BattleChara*)_obj : null;
        private bool IsPlayer => Type == ObjectKind.Pc && Character != null;

        [LuaWrapper] public ObjectKind Type => _obj->ObjectKind;
        [LuaWrapper] public string Name => _obj->NameString;
        [LuaWrapper] public Vector3 Position => _obj->Position;
        [LuaWrapper] public float DistanceTo => Player.DistanceTo(Position);

        private T GetCharacterValue<T>(Func<T> getter) => IsPlayer ? getter() : default!;
        [LuaWrapper] public ulong ContentId => GetCharacterValue(() => Character->ContentId);
        [LuaWrapper] public ulong AccountId => GetCharacterValue(() => Character->AccountId);
        [LuaWrapper] public ushort CurrentWorld => GetCharacterValue(() => Character->CurrentWorld);
        [LuaWrapper] public ushort HomeWorld => GetCharacterValue(() => Character->HomeWorld);

        [LuaWrapper] public uint CurrentHp => GetCharacterValue(() => Character->Health);
        [LuaWrapper] public uint MaxHp => GetCharacterValue(() => Character->MaxHealth);
        [LuaWrapper] public float HealthPercent => CurrentHp / MaxHp * 100;
        [LuaWrapper] public uint CurrentMp => GetCharacterValue(() => Character->Mana);
        [LuaWrapper] public uint MaxMp => GetCharacterValue(() => Character->MaxMana);

        [LuaWrapper] public EntityWrapper? Target => Dalamud?.TargetObject is { } target ? new(target) : null;
        [LuaWrapper] public bool IsCasting => GetCharacterValue(() => Character->IsCasting);
        [LuaWrapper] public bool IsCastInterruptible => GetCharacterValue(() => Character->GetCastInfo()->Interruptible) > 0;
        [LuaWrapper] public bool IsInCombat => GetCharacterValue(() => Character->InCombat);
        [LuaWrapper] public byte HuntRank => FindRow<NotoriousMonster>(x => x.BNpcBase.Value!.RowId == _obj->EntityId)?.Rank ?? 0;

        [LuaWrapper] public void SetAsTarget() => Svc.Targets.Target = Dalamud;
        [LuaWrapper] public void SetAsFocusTarget() => Svc.Targets.FocusTarget = Dalamud;
        [LuaWrapper] public void ClearTarget() => Svc.Targets.Target = null;
    }
}
