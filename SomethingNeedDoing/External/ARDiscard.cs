using ECommons.EzIpcManager;
using SomethingNeedDoing.Framework.Interfaces;

namespace SomethingNeedDoing.External;

public class ARDiscard : IPC
{
    public override string Name => "ARDiscard";
    public override string Repo => Repos.Liza;

    [EzIPC]
    [LuaFunction(description: "Gets a list of item IDs that should be discarded")]
    public readonly Func<IReadOnlySet<uint>> GetItemsToDiscard = null!;
}
