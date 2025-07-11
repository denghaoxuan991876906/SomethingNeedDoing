using ECommons.EzIpcManager;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.External;
public class Wrath : IPC
{
    public override string Name => "WrathCombo";
    public override string Repo => Repos.Punish;
    private static string InternalName => Svc.PluginInterface.InternalName;

    [EzIPC("RegisterForLease")]
    private readonly Func<string, string, Guid?> _registerForLease = null!;

    [LuaFunction(
        description: "Registers for lease",
        parameterDescriptions: ["scriptName"])]
    public Guid? RegisterForLease(string scriptName) => _registerForLease(InternalName, scriptName);

    [EzIPC]
    [LuaFunction(
        description: "Gets the auto rotation state")]
    public readonly Func<bool> GetAutoRotationState = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets the auto rotation state",
        parameterDescriptions: ["leaseId", "enabled"])]
    public readonly Action<Guid, bool> SetAutoRotationState = null!;

    [EzIPC]
    [LuaFunction(
        description: "Checks if the current job auto rotation is ready")]
    public readonly Func<bool> IsCurrentJobAutoRotationReady = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets the current job auto rotation ready",
        parameterDescriptions: ["leaseId"])]
    public readonly Action<Guid> SetCurrentJobAutoRotationReady = null!;

    [EzIPC]
    [LuaFunction(
        description: "Releases control",
        parameterDescriptions: ["leaseId"])]
    public readonly Action<Guid> ReleaseControl = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets the auto rotation config state")]
    public readonly Func<AutoRotationConfigOption> GetAutoRotationConfigState = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets the auto rotation config state",
        parameterDescriptions: ["leaseId", "configOption"])]
    public readonly Action<Guid, AutoRotationConfigOption> SetAutoRotationConfigState = null!;

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
}
