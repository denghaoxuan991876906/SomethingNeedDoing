namespace SomethingNeedDoing.Old.IPC;

internal class AutoHookIPC
{
    public static void SetPluginState(bool state) => Svc.PluginInterface.GetIpcSubscriber<bool, object>("AutoHook.SetPluginState").InvokeAction(state);

    public static void SetAutoGigState(bool state) => Svc.PluginInterface.GetIpcSubscriber<bool, object>("AutoHook.SetAutoGigState").InvokeAction(state);

    public static void SetAutoGigSize(int size) => Svc.PluginInterface.GetIpcSubscriber<int, object>("AutoHook.SetAutoGigSize").InvokeAction(size);

    public static void SetAutoGigSpeed(int speed) => Svc.PluginInterface.GetIpcSubscriber<int, object>("AutoHook.SetAutoGigSpeed").InvokeAction(speed);

    public static void SetPreset(string preset) => Svc.PluginInterface.GetIpcSubscriber<string, object>("AutoHook.SetPreset").InvokeAction(preset);

    public static void CreateAndSelectAnonymousPreset(string preset) => Svc.PluginInterface.GetIpcSubscriber<string, object>("AutoHook.CreateAndSelectAnonymousPreset").InvokeAction(preset);

    public static void DeleteSelectedPreset() => Svc.PluginInterface.GetIpcSubscriber<object>("AutoHook.DeleteSelectedPreset").InvokeAction();

    public static void DeleteAllAnonymousPresets() => Svc.PluginInterface.GetIpcSubscriber<object>("AutoHook.DeleteAllAnonymousPresets").InvokeAction();
}
