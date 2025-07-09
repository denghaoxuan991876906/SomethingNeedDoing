using ECommons.EzIpcManager;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.External;

public class CBT : IPC
{
    public override string Name => "Automaton";
    public override string Repo => Repos.Croizat;

    [EzIPC]
    [LuaFunction(
        description: "Checks if a CBT tweak is enabled",
        parameterDescriptions: ["className"])]
    public readonly Func<string, bool> IsTweakEnabled = null!;

    [EzIPC]
    [LuaFunction(
        description: "Sets a CBT tweak's state",
        parameterDescriptions: ["className", "state"])]
    public readonly Action<string, bool> SetTweakState = null!;
}
