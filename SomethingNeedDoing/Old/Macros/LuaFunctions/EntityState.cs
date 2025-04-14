using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System.Reflection;

namespace SomethingNeedDoing.Old.Macros.LuaFunctions;

internal class EntityState
{
    internal static EntityState Instance { get; } = new();

    public List<string> ListAllFunctions()
    {
        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var list = new List<string>();
        foreach (var method in methods.Where(x => x.Name != nameof(ListAllFunctions) && x.DeclaringType != typeof(object)))
        {
            var parameterList = method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue ?? "null") : "")}");
            list.Add($"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameterList)})");
        }
        return list;
    }

    public float GetDistanceToPoint(float x, float y, float z) => Vector3.Distance(Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero, new Vector3(x, y, z));

    #region Target
    public string GetTargetName() => Svc.Targets.Target?.Name.TextValue ?? "";
    public unsafe uint GetTargetWorldId() => (Svc.Targets.Target as IPlayerCharacter)?.CurrentWorld.Value.RowId ?? 0;
    public unsafe string GetTargetWorldName() => (Svc.Targets.Target as IPlayerCharacter)?.CurrentWorld.Value.Name.ExtractText() ?? "";
    public float GetTargetRawXPos() => Svc.Targets.Target?.Position.X ?? 0;
    public float GetTargetRawYPos() => Svc.Targets.Target?.Position.Y ?? 0;
    public float GetTargetRawZPos() => Svc.Targets.Target?.Position.Z ?? 0;
    public unsafe bool IsTargetCasting() => ((Character*)Svc.Targets.Target?.Address!)->IsCasting;
    public unsafe uint GetTargetActionID() => ((Character*)Svc.Targets.Target?.Address!)->GetCastInfo()->ActionId;
    public float GetTargetHP() => (Svc.Targets.Target as Dalamud.Game.ClientState.Objects.Types.ICharacter)?.CurrentHp ?? 0;
    public float GetTargetMaxHP() => (Svc.Targets.Target as Dalamud.Game.ClientState.Objects.Types.ICharacter)?.MaxHp ?? 0;
    public float GetTargetHPP() => GetTargetHP() / GetTargetMaxHP() * 100;
    public float GetTargetRotation() => (float)(Svc.Targets.Target?.Rotation * (180 / Math.PI) ?? 0);
    public byte? GetTargetObjectKind() => (byte?)Svc.Targets.Target?.ObjectKind;
    public byte? GetTargetSubKind() => Svc.Targets.Target?.SubKind;
    public unsafe void TargetClosestEnemy(float distance = 0) => Svc.Targets.Target = Svc.Objects.OrderBy(DistanceToObject).FirstOrDefault(o => o.IsTargetable && o.IsHostile() && !o.IsDead && (distance == 0 || DistanceToObject(o) <= distance));
    public unsafe void TargetClosestFateEnemy(float distance = 0) => Svc.Targets.Target = Svc.Objects.OrderBy(DistanceToObject).FirstOrDefault(o => o.IsTargetable && o.IsHostile() && !o.IsDead && (distance == 0 || DistanceToObject(o) <= distance) && o.Struct()->FateId > 0);
    public void ClearTarget() => Svc.Targets.Target = null;
    public float GetDistanceToTarget() => Vector3.Distance(Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero, Svc.Targets.Target?.Position ?? Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero);

    public unsafe bool TargetHasStatus(uint statusID) => ((Character*)Svc.Targets.Target?.Address!)->GetStatusManager()->HasStatus(statusID);
    public unsafe uint GetTargetFateID() => Svc.Targets.Target != null ? Svc.Targets.Target.Struct()->FateId : 0u;
    public unsafe bool IsTargetMounted()
    {
        var target = Svc.Targets.Target;
        if (target == null)
            return false;

        if (target.ObjectKind != ObjectKind.Player)
            return false;

        var targetGameObject = target.Struct();
        if (targetGameObject->ObjectIndex + 1 > Svc.Objects.Length)
            return false;

        var mountObject = Svc.Objects[targetGameObject->ObjectIndex + 1];
        if (mountObject == null || mountObject.ObjectKind != ObjectKind.MountType)
            return false;
        return true;
    }
    public unsafe bool IsTargetInCombat() => ((Character*)Svc.Targets.Target?.Address!)->InCombat;
    public byte GetTargetHuntRank() => (byte)(Svc.Targets.Target != null ? FindRow<NotoriousMonster>(x => x.BNpcBase.Value!.RowId == Svc.Targets.Target.DataId)!.Value.Rank : 0);
    public float GetTargetHitboxRadius() => Svc.Targets.Target?.HitboxRadius ?? 0;
    public bool HasTarget() => Svc.Targets.Target != null;
    #endregion

    #region Focus Target
    public string GetFocusTargetName() => Svc.Targets.FocusTarget?.Name.TextValue ?? "";
    public float GetFocusTargetRawXPos() => Svc.Targets.FocusTarget?.Position.X ?? 0;
    public float GetFocusTargetRawYPos() => Svc.Targets.FocusTarget?.Position.Y ?? 0;
    public float GetFocusTargetRawZPos() => Svc.Targets.FocusTarget?.Position.Z ?? 0;
    public unsafe bool IsFocusTargetCasting() => ((Character*)Svc.Targets.FocusTarget?.Address!)->IsCasting;
    public unsafe uint GetFocusTargetActionID() => ((Character*)Svc.Targets.FocusTarget?.Address!)->GetCastInfo()->ActionId;
    public float GetFocusTargetHP() => (Svc.Targets.FocusTarget as Dalamud.Game.ClientState.Objects.Types.ICharacter)?.CurrentHp ?? 0;
    public float GetFocusTargetMaxHP() => (Svc.Targets.FocusTarget as Dalamud.Game.ClientState.Objects.Types.ICharacter)?.MaxHp ?? 0;
    public float GetFocusTargetHPP() => GetFocusTargetHP() / GetFocusTargetMaxHP() * 100;
    public float GetFocusTargetRotation() => (float)(Svc.Targets.FocusTarget?.Rotation * (180 / Math.PI) ?? 0);
    public void ClearFocusTarget() => Svc.Targets.FocusTarget = null;
    public float GetDistanceToFocusTarget() => Vector3.Distance(Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero, Svc.Targets.FocusTarget?.Position ?? Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero);
    public unsafe bool FocusTargetHasStatus(uint statusID) => ((Character*)Svc.Targets.FocusTarget?.Address!)->GetStatusManager()->HasStatus(statusID);
    public unsafe uint GetFocusTargetFateID() => Svc.Targets.FocusTarget != null ? Svc.Targets.FocusTarget.Struct()->FateId : 0u;
    #endregion

    #region Any Object
    public float GetObjectRawXPos(string name) => GetGameObjectFromName(name)?.Position.X ?? 0;
    public float GetObjectRawYPos(string name) => GetGameObjectFromName(name)?.Position.Y ?? 0;
    public float GetObjectRawZPos(string name) => GetGameObjectFromName(name)?.Position.Z ?? 0;
    public float GetDistanceToObject(string name) => Vector3.Distance(Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero, Svc.Objects.OrderBy(DistanceToObject).FirstOrDefault(x => x.Name.TextValue.Equals(name, StringComparison.InvariantCultureIgnoreCase))?.Position ?? Vector3.Zero);
    public unsafe bool IsObjectCasting(string name) => ((Character*)GetGameObjectFromName(name)?.Address!)->IsCasting;
    public unsafe uint GetObjectActionID(string name) => ((Character*)GetGameObjectFromName(name)?.Address!)->GetCastInfo()->ActionId;
    public float GetObjectHP(string name) => (GetGameObjectFromName(name) as Dalamud.Game.ClientState.Objects.Types.ICharacter)?.CurrentHp ?? 0;
    public float GetObjectMaxHP(string name) => (GetGameObjectFromName(name) as Dalamud.Game.ClientState.Objects.Types.ICharacter)?.MaxHp ?? 0;
    public float GetObjectHPP(string name) => GetObjectHP(name) / GetObjectMaxHP(name) * 100;
    public float GetObjectRotation(string name) => (float)(GetGameObjectFromName(name)?.Rotation * (180 / Math.PI) ?? 0);
    public unsafe bool ObjectHasStatus(string name, uint statusID) => ((Character*)GetGameObjectFromName(name)?.Address!)->GetStatusManager()->HasStatus(statusID);
    public unsafe uint GetObjectFateID(string name) => GetGameObjectFromName(name) != null ? GetGameObjectFromName(name).Struct()->FateId : 0u;
    public bool DoesObjectExist(string name) => GetGameObjectFromName(name) != null;
    public unsafe bool IsObjectMounted(string name)
    {
        var target = GetGameObjectFromName(name);
        if (target == null)
            return false;

        if (target.ObjectKind != ObjectKind.Player)
            return false;

        var targetGameObject = target.Struct();
        if (targetGameObject->ObjectIndex + 1 > Svc.Objects.Length)
            return false;

        var mountObject = Svc.Objects[targetGameObject->ObjectIndex + 1];
        if (mountObject == null || mountObject.ObjectKind != ObjectKind.MountType)
            return false;
        return true;
    }
    public uint GetObjectDataID(string name) => GetGameObjectFromName(name)?.DataId ?? 0;
    public unsafe bool IsObjectInCombat(string name) => ((Character*)GetGameObjectFromName(name)?.Address!)->InCombat;
    public byte GetObjectHuntRank(string name) => Svc.Data.GetExcelSheet<NotoriousMonster>()?.FirstOrDefault(x => x.BNpcBase.Value!.RowId == GetObjectDataID(name)).Rank ?? 0;
    public float GetObjectHitboxRadius(string name) => GetGameObjectFromName(name)?.HitboxRadius ?? 0;
    #endregion

    #region Party Members
    public string GetPartyMemberName(int index) => Svc.Party[index]?.Name.TextValue ?? "";
    public unsafe uint GetPartyMemberWorldId(int index) => Svc.Party[index]?.World.RowId ?? 0;
    public unsafe string GetPartyMemberWorldName(int index) => Svc.Party[index]?.World.Value.Name.ExtractText() ?? "";
    public float GetPartyMemberRawXPos(int index) => Svc.Party[index]?.Position.X ?? 0;
    public float GetPartyMemberRawYPos(int index) => Svc.Party[index]?.Position.Y ?? 0;
    public float GetPartyMemberRawZPos(int index) => Svc.Party[index]?.Position.Z ?? 0;
    public float GetDistanceToPartyMember(int index) => Vector3.Distance(Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero, Svc.Party[index]?.Position ?? Vector3.Zero);
    public unsafe bool IsPartyMemberCasting(int index) => ((Character*)Svc.Party[index]?.Address!)->IsCasting;
    public unsafe uint GetPartyMemberActionID(int index) => ((Character*)Svc.Party[index]?.Address!)->GetCastInfo()->ActionId;
    public float GetPartyMemberHP(int index) => Svc.Party[index]?.CurrentHP ?? 0;
    public float GetPartyMemberMaxHP(int index) => Svc.Party[index]?.MaxHP ?? 0;
    public float GetPartyMemberHPP(int index) => GetPartyMemberHP(index) / GetPartyMemberMaxHP(index) * 100;
    public float GetPartyMemberRotation(int index) => (float)(Svc.Party[index]?.GameObject?.Rotation * (180 / Math.PI) ?? 0);
    public unsafe bool PartyMemberHasStatus(int index, uint statusID) => Svc.Party[index]?.Statuses.Any(s => s.StatusId == statusID) ?? false;
    public unsafe bool IsPartyMemberMounted(int index)
    {
        var target = Svc.Party[index]?.GameObject;
        if (target == null)
            return false;

        if (target.ObjectKind != ObjectKind.Player)
            return false;

        var targetGameObject = target.Struct();
        if (targetGameObject->ObjectIndex + 1 > Svc.Objects.Length)
            return false;

        var mountObject = Svc.Objects[targetGameObject->ObjectIndex + 1];
        if (mountObject == null || mountObject.ObjectKind != ObjectKind.MountType)
            return false;
        return true;
    }
    public unsafe bool IsPartyMemberInCombat(int index) => ((Character*)Svc.Party[index]?.Address!)->InCombat;

    public uint GetPartyLeadIndex() => Svc.Party.PartyLeaderIndex;
    #endregion

    #region Chocobo
    public unsafe float GetBuddyTimeRemaining() => UIState.Instance()->Buddy.CompanionInfo.TimeLeft;
    #endregion

    private float DistanceToObject(Dalamud.Game.ClientState.Objects.Types.IGameObject o) => Vector3.Distance(o.Position, Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero);
    private Dalamud.Game.ClientState.Objects.Types.IGameObject? GetGameObjectFromName(string name) => Svc.Objects.OrderBy(DistanceToObject).FirstOrDefault(x => x.Name.TextValue.Equals(name, StringComparison.InvariantCultureIgnoreCase));
}
