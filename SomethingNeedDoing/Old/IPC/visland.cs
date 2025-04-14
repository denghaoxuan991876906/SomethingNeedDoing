using ECommons.EzIpcManager;

namespace SomethingNeedDoing.Old.IPC;
#nullable disable
public class VislandIPC
{
    public const string Name = "visland";
    public VislandIPC() => EzIPC.Init(this, Name, SafeWrapper.AnyException);

    [EzIPC] public Func<bool> IsRouteRunning;
    [EzIPC] public Func<bool> IsRoutePaused;
    [EzIPC] public Action<bool> SetRoutePaused;
    [EzIPC] public Action StopRoute;
    [EzIPC] public Action<string, bool> StartRoute;
}
