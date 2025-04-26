using ECommons.EzIpcManager;
using GCInfo = (uint ShopDataID, uint ExchangeDataID, System.Numerics.Vector3 Position);

namespace SomethingNeedDoing.External;

public class AutoRetainer : IPC
{
    public override string Name => "AutoRetainer";
    public override string Repo => Repos.Punish;

    [EzIPC]
    [LuaFunction(description: "Gets whether multi-mode is enabled")]
    public readonly Func<bool> GetMultiModeEnabled = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets whether multi-mode is enabled",
        parameterDescriptions: ["enabled"])]
    public readonly Action<bool> SetMultiModeEnabled = null!;

    [EzIPC("PluginState.%m")]
    [LuaFunction(description: "Checks if the plugin is busy")]
    public readonly Func<bool> IsBusy = null!;

    [EzIPC("PluginState.%m")]
    [LuaFunction(description: "Gets the number of free inventory slots")]
    public readonly Func<int> GetInventoryFreeSlotCount = null!;

    [EzIPC("PluginState.%m")]
    [LuaFunction(description: "Gets all enabled retainers")]
    public readonly Func<Dictionary<ulong, HashSet<string>>> GetEnabledRetainers = null!;

    [EzIPC("PluginState.%m")]
    [LuaFunction(description: "Checks if any retainers are available for the current character")]
    public readonly Func<bool> AreAnyRetainersAvailableForCurrentChara = null!;

    [EzIPC("PluginState.%m")]
    [LuaFunction(description: "Aborts all tasks")]
    public readonly Action AbortAllTasks = null!;

    [EzIPC("PluginState.%m")]
    [LuaFunction(description: "Disables all functions")]
    public readonly Action DisableAllFunctions = null!;

    [EzIPC("PluginState.%m")]
    [LuaFunction(description: "Enables multi-mode")]
    public readonly Action EnableMultiMode = null!;

    /// <summary>
    /// Action onFailure
    /// </summary>
    [EzIPC("PluginState.%m")]
    [LuaFunction(
        description: "Enqueues a high-end task",
        parameterDescriptions: ["onFailure"])]
    public readonly Action<Action> EnqueueHET = null!;

    [EzIPC("PluginState.%m")]
    [LuaFunction(description: "Checks if auto-login is possible")]
    public readonly Func<bool> CanAutoLogin = null!;

    /// <summary>
    /// string charaNameWithWorld
    /// </summary>
    [EzIPC("PluginState.%m")]
    [LuaFunction(
        description: "Relogs to a specific character",
        parameterDescriptions: ["charaNameWithWorld"])]
    public readonly Func<string, bool> Relog = null!;

    [EzIPC("PluginState.%m")]
    [LuaFunction(description: "Gets whether retainer sense is enabled")]
    public readonly Func<bool> GetOptionRetainerSense = null!;

    [EzIPC("PluginState.%m")]
    [LuaFunction(
        description: "Sets whether retainer sense is enabled",
        parameterDescriptions: ["enabled"])]
    public readonly Action<bool> SetOptionRetainerSense = null!;

    [EzIPC("PluginState.%m")]
    [LuaFunction(description: "Gets the retainer sense threshold")]
    public readonly Func<int> GetOptionRetainerSenseThreshold = null!;

    [EzIPC("PluginState.%m")]
    [LuaFunction(
        description: "Sets the retainer sense threshold",
        parameterDescriptions: ["threshold"])]
    public readonly Action<int> SetOptionRetainerSenseThreshold = null!;

    /// <summary>
    /// ulong CID
    /// </summary>
    [EzIPC("PluginState.%m")]
    [LuaFunction(
        description: "Gets the closest retainer venture seconds remaining",
        parameterDescriptions: ["cid"])]
    public readonly Func<ulong, long?> GetClosestRetainerVentureSecondsRemaining = null!;

    [EzIPC("GC.%m")]
    [LuaFunction(description: "Enqueues initiation")]
    public readonly Action EnqueueInitiation = null!;

    [EzIPC("GC.%m")]
    [LuaFunction(description: "Gets GC information")]
    public readonly Func<GCInfo?> GetGCInfo = null!;
}
