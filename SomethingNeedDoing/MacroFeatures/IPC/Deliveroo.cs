using Dalamud.Plugin.Ipc;

namespace SomethingNeedDoing.MacroFeatures.IPC;

internal static class DeliverooIPC
{
    private const string IsTurnInRunningStr = "Deliveroo.IsTurnInRunning";
    private const string TurnInStartedStr = "Deliveroo.TurnInStarted";
    private const string TurnInStoppedStr = "Deliveroo.TurnInStopped";

    internal static ICallGateSubscriber<bool>? IsTurnInRunning;
    internal static ICallGateSubscriber<object>? TurnInStarted;
    internal static ICallGateSubscriber<object>? TurnInStopped;

    internal static void Init()
    {
        IsTurnInRunning = Svc.PluginInterface.GetIpcSubscriber<bool>(IsTurnInRunningStr);
        TurnInStarted = Svc.PluginInterface.GetIpcSubscriber<object>(TurnInStartedStr);
        TurnInStopped = Svc.PluginInterface.GetIpcSubscriber<object>(TurnInStoppedStr);
    }
}
