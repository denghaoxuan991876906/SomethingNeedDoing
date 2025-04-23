using ECommons.EzIpcManager;

namespace SomethingNeedDoing.MacroFeatures.IPC;

#nullable disable
public class Artisan
{
    public const string Name = "Artisan";
    public Artisan() => EzIPC.Init(this, Name, SafeWrapper.AnyException);

    [EzIPC] public Func<bool> GetEnduranceStatus;
    [EzIPC] public Action<bool> SetEnduranceStatus;
    [EzIPC] public Func<bool> IsListRunning;
    [EzIPC] public Func<bool> IsListPaused;
    [EzIPC] public Action<bool> SetListPause;
    [EzIPC] public Func<bool> GetStopRequest;
    [EzIPC] public Action<bool> SetStopRequest;
    [EzIPC] public Action<ushort, int> CraftItem;
}
