using ECommons.EzIpcManager;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.External;
public class Lifestream : IPC
{
    public override string Name => "Lifestream";
    public override string Repo => Repos.Limiana;

    [EzIPC]
    [LuaFunction(
        description: "Teleports to an aethernet location",
        parameterDescriptions: ["destination"]
    )]
    public Func<string, bool> AethernetTeleport = null!;

    [EzIPC]
    [LuaFunction(
        description: "Teleports to a specific location",
        parameterDescriptions: ["territoryId", "subIndex"]
    )]
    public Func<uint, byte, bool> Teleport = null!;

    [EzIPC]
    [LuaFunction(description: "Teleports to home")]
    public Func<bool> TeleportToHome = null!;

    [EzIPC]
    [LuaFunction(description: "Teleports to free company")]
    public Func<bool> TeleportToFC = null!;

    [EzIPC]
    [LuaFunction(description: "Teleports to apartment")]
    public Func<bool> TeleportToApartment = null!;

    [EzIPC]
    [LuaFunction(description: "Checks if the plugin is busy")]
    public Func<bool> IsBusy = null!;

    [EzIPC]
    [LuaFunction(
        description: "Executes a command",
        parameterDescriptions: ["command"]
    )]
    public Action<string> ExecuteCommand = null!;

    [EzIPC]
    [LuaFunction(description: "Aborts the current operation")]
    public Action Abort = null!;
}
