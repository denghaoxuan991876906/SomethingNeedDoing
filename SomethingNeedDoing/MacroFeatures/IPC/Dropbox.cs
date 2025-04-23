using ECommons.EzIpcManager;

namespace SomethingNeedDoing.MacroFeatures.IPC;

#nullable disable
public class Dropbox
{
    public static string Name = "Dropbox";
    public Dropbox() => EzIPC.Init(this, Name);

    [EzIPC] public readonly Func<bool> IsBusy;
    [EzIPC] public readonly Func<uint, bool, int> GetItemQuantity; // id, hq
    [EzIPC] public readonly Action<uint, bool, int> SetItemQuantity; // id, hq, quantity

    [EzIPC] public readonly Action BeginTradingQueue;
    [EzIPC] public readonly Action Stop;
}