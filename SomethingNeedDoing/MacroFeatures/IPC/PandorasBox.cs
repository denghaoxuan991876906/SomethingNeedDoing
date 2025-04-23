using Dalamud.Plugin.Ipc;

namespace SomethingNeedDoing.MacroFeatures.IPC;

internal static class PandorasBoxIPC
{
    internal static string Name = "PandorasBox";
    internal static ICallGateSubscriber<string, bool?>? GetFeatureEnabled;
    internal static ICallGateSubscriber<string, bool, object>? SetFeatureEnabled;
    internal static ICallGateSubscriber<string, string, bool?>? GetConfigEnabled;
    internal static ICallGateSubscriber<string, string, bool, object>? SetConfigEnabled;
    internal static ICallGateSubscriber<string, int, object>? PauseFeature;

    internal static void Init()
    {
        GetFeatureEnabled = Svc.PluginInterface.GetIpcSubscriber<string, bool?>($"PandorasBox.GetFeatureEnabled");
        SetFeatureEnabled = Svc.PluginInterface.GetIpcSubscriber<string, bool, object>($"PandorasBox.SetFeatureEnabled");
        GetConfigEnabled = Svc.PluginInterface.GetIpcSubscriber<string, string, bool?>($"PandorasBox.GetConfigEnabled");
        SetConfigEnabled = Svc.PluginInterface.GetIpcSubscriber<string, string, bool, object>($"PandorasBox.SetConfigEnabled");
        PauseFeature = Svc.PluginInterface.GetIpcSubscriber<string, int, object>("PandorasBox.PauseFeature");
    }
}
