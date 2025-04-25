using ECommons.EzIpcManager;
using SomethingNeedDoing.Attributes;

namespace SomethingNeedDoing.External;

public class Tippy : IPC
{
    public override string Name => "Tippy";
    public override string Repo => Repos.FirstParty;

    /// <summary>
    /// Register Tip.
    /// This will be added to the standard tip queue and will be displayed eventually at random.
    /// This can be used when you want to add your own tips.
    /// </summary>
    /// <param name="text">the text of the tip.</param>
    /// <returns>indicator if tip was successfully registered.</returns>
    [EzIPC]
    [LuaFunction(
        description: "Registers a tip to be displayed at random",
        parameterDescriptions: ["text"])]
    public readonly Func<string, bool> RegisterTip = null!;

    /// <summary>
    /// Register Message.
    /// This will be added to the priority message queue and likely display right away.
    /// This can be used to have Tippy display messages in near real-time you want to show to the user.
    /// </summary>
    /// <param name="text">the text of the message.</param>
    /// <returns>indicator if message was successfully registered.</returns>
    [EzIPC]
    [LuaFunction(
        description: "Registers a message to be displayed immediately",
        parameterDescriptions: ["text"])]
    public readonly Func<string, bool> RegisterMessage = null!;
}
