using Dalamud.Plugin.Ipc;
using ECommons;

namespace SomethingNeedDoing.MacroFeatures.IPC;
internal static class AutoDutyIPC
{
    internal static readonly string Name = "AutoDuty";
    internal static ICallGateSubscriber<object>? ListConfig;
    internal static ICallGateSubscriber<string, string?>? GetConfig;
    internal static ICallGateSubscriber<string, string, object>? SetConfig;
    internal static ICallGateSubscriber<uint, int, bool, object>? Run;
    internal static ICallGateSubscriber<bool, object>? Start;
    internal static ICallGateSubscriber<object>? Stop;
    internal static ICallGateSubscriber<bool?>? IsNavigating;
    internal static ICallGateSubscriber<bool?>? IsLooping;
    internal static ICallGateSubscriber<bool?>? IsStopped;
    internal static ICallGateSubscriber<uint, bool?>? ContentHasPath;

    internal static void Init()
    {
        try
        {
            ListConfig = Svc.PluginInterface.GetIpcSubscriber<object>($"{Name}.ListConfig");
            GetConfig = Svc.PluginInterface.GetIpcSubscriber<string, string?>($"{Name}.GetConfig");
            SetConfig = Svc.PluginInterface.GetIpcSubscriber<string, string, object>($"{Name}.SetConfig");
            Run = Svc.PluginInterface.GetIpcSubscriber<uint, int, bool, object>($"{Name}.Run");
            Start = Svc.PluginInterface.GetIpcSubscriber<bool, object>($"{Name}.Start");
            Stop = Svc.PluginInterface.GetIpcSubscriber<object>($"{Name}.Stop");
            IsNavigating = Svc.PluginInterface.GetIpcSubscriber<bool?>($"{Name}.IsNavigating");
            IsLooping = Svc.PluginInterface.GetIpcSubscriber<bool?>($"{Name}.IsLooping");
            IsStopped = Svc.PluginInterface.GetIpcSubscriber<bool?>($"{Name}.IsStopped");
            ContentHasPath = Svc.PluginInterface.GetIpcSubscriber<uint, bool?>($"{Name}.ContentHasPath");
        }
        catch (Exception ex) { ex.Log(); }
    }
}