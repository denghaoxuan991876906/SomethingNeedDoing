using ECommons.EzIpcManager;
using SomethingNeedDoing.Framework.Interfaces;

namespace SomethingNeedDoing.External;
public class AutoDuty : IPC
{
    public override string Name => "AutoDuty";
    public override string Repo => Repos.Herc;

    [EzIPC]
    [LuaFunction(description: "Opens the list configuration")]
    public readonly Action ListConfig = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets a configuration value",
        parameterDescriptions: ["key"])]
    public readonly Func<string, string?> GetConfig = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets a configuration value",
        parameterDescriptions: ["key", "value"])]
    public readonly Func<string, string> SetConfig = null!;

    [EzIPC]
    [LuaFunction(
        description: "Runs a specific duty",
        parameterDescriptions: ["dutyId", "count"])]
    public readonly Func<uint, int, bool> Run = null!;

    [EzIPC]
    [LuaFunction(description: "Starts the auto duty process")]
    public readonly Func<bool> Start = null!;

    [EzIPC]
    [LuaFunction(description: "Stops the auto duty process")]
    public readonly Action Stop = null!;

    [EzIPC]
    [LuaFunction(description: "Checks if currently navigating")]
    public readonly Func<bool?> IsNavigating = null!;

    [EzIPC]
    [LuaFunction(description: "Checks if currently looping")]
    public readonly Func<bool?> IsLooping = null!;

    [EzIPC]
    [LuaFunction(description: "Checks if the process is stopped")]
    public readonly Func<bool?> IsStopped = null!;

    [EzIPC]
    [LuaFunction(
        description: "Checks if a content has a path",
        parameterDescriptions: ["contentId"])]
    public readonly Func<uint, bool?> ContentHasPath = null!;
}
