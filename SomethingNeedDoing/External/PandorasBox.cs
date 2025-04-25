using Dalamud.Plugin.Ipc;
using ECommons.EzIpcManager;
using SomethingNeedDoing.Attributes;

namespace SomethingNeedDoing.External;

public class PandorasBoxIPC : IPC
{
    public override string Name => "PandorasBox";
    public override string Repo => Repos.Punish;

    [EzIPC]
    [LuaFunction(
        description: "Gets whether a feature is enabled",
        parameterDescriptions: ["featureName"]
    )]
    public readonly Func<string, bool?> GetFeatureEnabled = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets whether a configuration is enabled",
        parameterDescriptions: ["configName", "configValue"]
    )]
    public readonly Func<string, string, bool?> GetConfigEnabled = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets whether a feature is enabled",
        parameterDescriptions: ["featureName", "enabled"]
    )]
    public readonly Action<string, bool> SetFeatureEnabled = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets whether a configuration is enabled",
        parameterDescriptions: ["configName", "configValue", "enabled"]
    )]
    public readonly Action<string, string, bool> SetConfigEnabled = null!;

    [EzIPC]
    [LuaFunction(
        description: "Pauses a feature",
        parameterDescriptions: ["featureName", "duration"]
    )]
    public readonly Action<string, int> PauseFeature = null!;
}
