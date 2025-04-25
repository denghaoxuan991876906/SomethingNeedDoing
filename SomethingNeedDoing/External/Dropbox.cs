using ECommons.EzIpcManager;
using SomethingNeedDoing.Attributes;

namespace SomethingNeedDoing.External;

public class Dropbox : IPC
{
    public override string Name => "Dropbox";
    public override string Repo => Repos.Kawaii;

    [EzIPC]
    [LuaFunction(description: "Checks if the plugin is busy")]
    public readonly Func<bool> IsBusy = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets the quantity of an item",
        parameterDescriptions: ["id", "hq"])]
    public readonly Func<uint, bool, int> GetItemQuantity = null!; // id, hq

    [EzIPC]
    [LuaFunction(
        description: "Sets the quantity of an item",
        parameterDescriptions: ["id", "hq", "quantity"])]
    public readonly Action<uint, bool, int> SetItemQuantity = null!; // id, hq, quantity

    [EzIPC]
    [LuaFunction(description: "Begins the trading queue")]
    public readonly Action BeginTradingQueue = null!;

    [EzIPC]
    [LuaFunction(description: "Stops the trading queue")]
    public readonly Action Stop = null!;
}
