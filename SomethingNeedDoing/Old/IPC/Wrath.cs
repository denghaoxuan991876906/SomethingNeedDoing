using ECommons.EzIpcManager;

namespace SomethingNeedDoing.Old.IPC;
#nullable disable
#pragma warning disable CS8632
public class Wrath
{
    public Wrath() => EzIPC.Init(this, "WrathCombo");

    public enum AutoRotationConfigOption
    {
        InCombatOnly = 0, //bool
        DPSRotationMode = 1,
        HealerRotationMode = 2,
        FATEPriority = 3, //bool
        QuestPriority = 4, //bool
        SingleTargetHPP = 5, //int
        AoETargetHPP = 6, //int
        SingleTargetRegenHPP = 7, //int
        ManageKardia = 8, //bool
        AutoRez = 9, //bool
        AutoRezDPSJobs = 10, //bool
        AutoCleanse = 11, //bool
        IncludeNPCs = 12, //bool
    }

    [EzIPC] public readonly Func<string, string, string, Guid?> RegisterForLeaseWithCallback;

    [EzIPC] public readonly Func<bool> GetAutoRotationState;

    [EzIPC] public readonly Action<Guid, bool> SetAutoRotationState;

    [EzIPC] public readonly Func<bool> IsCurrentJobAutoRotationReady;

    [EzIPC] public readonly Action<Guid> SetCurrentJobAutoRotationReady;

    [EzIPC] public readonly Action<Guid> ReleaseControl;

    [EzIPC] public readonly Func<AutoRotationConfigOption, object> GetAutoRotationConfigState;

    [EzIPC] public readonly Action<Guid, AutoRotationConfigOption, object> SetAutoRotationConfigState;
}
