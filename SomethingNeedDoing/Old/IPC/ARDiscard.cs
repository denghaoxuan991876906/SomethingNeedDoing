using ECommons.EzIpcManager;

namespace SomethingNeedDoing.Old.IPC;

#nullable disable
public class ARDiscard
{
    public ARDiscard() => EzIPC.Init(this, "ARDiscard");

    [EzIPC] public readonly Func<IReadOnlySet<uint>> GetItemsToDiscard;
}
