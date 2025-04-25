using Dalamud.Plugin.Ipc;
using ECommons.EzIpcManager;
using SomethingNeedDoing.Attributes;

namespace SomethingNeedDoing.External;

public class DeliverooIPC : IPC
{
    public override string Name => "Deliveroo";
    public override string Repo => Repos.Liza;

    [EzIPC]
    [LuaFunction(description: "Checks if a turn-in is currently running")]
    public Func<bool> IsTurnInRunning = null!;
}
