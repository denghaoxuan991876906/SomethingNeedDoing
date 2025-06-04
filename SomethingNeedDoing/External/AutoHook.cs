using ECommons.EzIpcManager;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.External;

public class AutoHook : IPC
{
    public override string Name => "AutoHook";
    public override string Repo => Repos.Punish;

    [EzIPC]
    [LuaFunction(
        description: "Enables or disables the AutoHook plugin.",
        parameterDescriptions: ["True to enable, false to disable"])]
    public readonly Action<bool> SetPluginState = null!;

    [EzIPC]
    [LuaFunction(
        description: "Enables or disables auto-gig functionality.",
        parameterDescriptions: ["True to enable, false to disable"])]
    public readonly Action<bool> SetAutoGigState = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets the size for auto-gig functionality.",
        parameterDescriptions: ["The size to set (integer value)"])]
    public readonly Action<int> SetAutoGigSize = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets the speed for auto-gig functionality.",
        parameterDescriptions: ["The speed to set (integer value)"])]
    public readonly Action<int> SetAutoGigSpeed = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets the current preset by name.",
        parameterDescriptions: ["The name of the preset to set"])]
    public readonly Action<string> SetPreset = null!;

    [EzIPC]
    [LuaFunction(
        description: "Creates and selects an anonymous preset.",
        parameterDescriptions: ["The name for the anonymous preset"])]
    public readonly Action<string> CreateAndSelectAnonymousPreset = null!;

    [EzIPC]
    [LuaFunction(description: "Deletes the currently selected preset.")]
    public readonly Action DeleteSelectedPreset = null!;

    [EzIPC]
    [LuaFunction(description: "Deletes all anonymous presets.")]
    public readonly Action DeleteAllAnonymousPresets = null!;
}
