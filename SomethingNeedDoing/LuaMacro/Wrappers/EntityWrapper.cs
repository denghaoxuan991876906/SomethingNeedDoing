using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel.Sheets;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.LuaMacro.Wrappers;
public unsafe class EntityWrapper : IWrapper
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
    private T GetCharacterValue<T>(Func<T> getter) => IsPlayer ? getter() : default!;

    [LuaDocs] public ObjectKind Type => _obj->ObjectKind;
    [LuaDocs] public string Name => _obj->NameString;
    [LuaDocs] public Vector3 Position => _obj->Position;
    [LuaDocs] public float DistanceTo => Player.DistanceTo(Position);

    [LuaDocs] public ulong ContentId => GetCharacterValue(() => Character->ContentId);
    [LuaDocs] public ulong AccountId => GetCharacterValue(() => Character->AccountId);
    [LuaDocs] public ushort CurrentWorld => GetCharacterValue(() => Character->CurrentWorld);
    [LuaDocs] public ushort HomeWorld => GetCharacterValue(() => Character->HomeWorld);

    [LuaDocs] public uint CurrentHp => GetCharacterValue(() => Character->Health);
    [LuaDocs] public uint MaxHp => GetCharacterValue(() => Character->MaxHealth);
    [LuaDocs] public float HealthPercent => CurrentHp / MaxHp * 100;
    [LuaDocs] public uint CurrentMp => GetCharacterValue(() => Character->Mana);
    [LuaDocs] public uint MaxMp => GetCharacterValue(() => Character->MaxMana);

    [LuaDocs] public EntityWrapper? Target => Dalamud?.TargetObject is { } target ? new(target) : null;
    [LuaDocs] public bool IsCasting => GetCharacterValue(() => Character->IsCasting);
    [LuaDocs] public bool IsCastInterruptible => GetCharacterValue(() => Character->GetCastInfo()->Interruptible) > 0;
    [LuaDocs] public bool IsInCombat => GetCharacterValue(() => Character->InCombat);
    [LuaDocs] public byte HuntRank => FindRow<NotoriousMonster>(x => x.BNpcBase.Value!.RowId == _obj->EntityId)?.Rank ?? 0;

    [LuaDocs] public void SetAsTarget() => Svc.Targets.Target = Dalamud;
    [LuaDocs] public void SetAsFocusTarget() => Svc.Targets.FocusTarget = Dalamud;
    [LuaDocs] public void ClearTarget() => Svc.Targets.Target = null;
    [LuaDocs] public void Interact() => Game.Interact(Dalamud);
}
