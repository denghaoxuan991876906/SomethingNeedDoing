using ECommons.EzIpcManager;

namespace SomethingNeedDoing.External;

public class DeliverooIPC : IPC
{
    public override string Name => "Deliveroo";
    public override string Repo => Repos.Liza;

    [EzIPC]
    [LuaFunction(description: "Checks if a turn-in is currently running")]
    public Func<bool> IsTurnInRunning = null!;
}
