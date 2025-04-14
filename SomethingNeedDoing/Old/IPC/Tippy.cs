using ECommons.EzIpcManager;

namespace SomethingNeedDoing.Old.IPC;

#nullable disable
public class Tippy
{
    public Tippy() => EzIPC.Init(this, "Tippy", SafeWrapper.IPCException);

    /// <summary>
    /// Register Tip.
    /// This will be added to the standard tip queue and will be displayed eventually at random.
    /// This can be used when you want to add your own tips.
    /// </summary>
    /// <param name="text">the text of the tip.</param>
    /// <returns>indicator if tip was successfully registered.</returns>
    [EzIPC] public readonly Func<string, bool> RegisterTip;

    /// <summary>
    /// Register Message.
    /// This will be added to the priority message queue and likely display right away.
    /// This can be used to have Tippy display messages in near real-time you want to show to the user.
    /// </summary>
    /// <param name="text">the text of the message.</param>
    /// <returns>indicator if message was successfully registered.</returns>
    [EzIPC] public readonly Func<string, bool> RegisterMessage;
}
