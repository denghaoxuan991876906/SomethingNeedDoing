using ECommons.EzIpcManager;
using GCInfo = (uint ShopDataID, uint ExchangeDataID, System.Numerics.Vector3 Position);

namespace SomethingNeedDoing.MacroFeatures.IPC;

#nullable disable
public class AutoRetainer
{
    public AutoRetainer() => EzIPC.Init(this, "AutoRetainer");

    [EzIPC] public readonly Func<bool> GetMultiModeEnabled;
    [EzIPC] public readonly Action<bool> SetMultiModeEnabled;

    [EzIPC("PluginState.%m")] public readonly Func<bool> IsBusy;
    [EzIPC("PluginState.%m")] public readonly Func<int> GetInventoryFreeSlotCount;
    [EzIPC("PluginState.%m")] public readonly Func<Dictionary<ulong, HashSet<string>>> GetEnabledRetainers;
    [EzIPC("PluginState.%m")] public readonly Func<bool> AreAnyRetainersAvailableForCurrentChara;
    [EzIPC("PluginState.%m")] public readonly Action AbortAllTasks;
    [EzIPC("PluginState.%m")] public readonly Action DisableAllFunctions;
    [EzIPC("PluginState.%m")] public readonly Action EnableMultiMode;
    /// <summary>
    /// Action onFailure
    /// </summary>
    [EzIPC("PluginState.%m")] public readonly Action<Action> EnqueueHET;
    [EzIPC("PluginState.%m")] public readonly Func<bool> CanAutoLogin;
    /// <summary>
    /// string charaNameWithWorld
    /// </summary>
    [EzIPC("PluginState.%m")] public readonly Func<string, bool> Relog;
    [EzIPC("PluginState.%m")] public readonly Func<bool> GetOptionRetainerSense;
    [EzIPC("PluginState.%m")] public readonly Action<bool> SetOptionRetainerSense;
    [EzIPC("PluginState.%m")] public readonly Func<int> GetOptionRetainerSenseThreshold;
    [EzIPC("PluginState.%m")] public readonly Action<int> SetOptionRetainerSenseThreshold;
    /// <summary>
    /// ulong CID
    /// </summary>
    [EzIPC("PluginState.%m")] public readonly Func<ulong, long?> GetClosestRetainerVentureSecondsRemaining;

    [EzIPC("GC.%m")] public readonly Action EnqueueInitiation;
    [EzIPC("GC.%m")] public readonly Func<GCInfo?> GetGCInfo;
}
