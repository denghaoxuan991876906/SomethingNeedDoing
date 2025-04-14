using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using ECommons.ChatMethods;
using FFXIVClientStructs.FFXIV.Client.UI;
using System.Threading.Channels;

namespace SomethingNeedDoing.Old.Managers;

internal class ChatManager : IDisposable
{
    private readonly Channel<string> chatBoxMessages = Channel.CreateUnbounded<string>();

    public ChatManager()
    {
        Svc.Framework.Update += FrameworkUpdate;
    }

    private unsafe delegate void ProcessChatBoxDelegate(UIModule* uiModule, nint message, nint unused, byte a4);

    public void Dispose()
    {
        Svc.Framework.Update -= FrameworkUpdate;
        chatBoxMessages.Writer.Complete();
    }

    public void PrintMessage(string message)
        => Svc.Chat.Print(new XivChatEntry()
        {
            Type = C.ChatType,
            Message = $"[{P.Prefix}] {message}",
        });

    public void PrintColor(string message, UIColor color)
        => Svc.Chat.Print(new XivChatEntry()
        {
            Type = C.ChatType,
            Message = new SeString(
                new UIForegroundPayload((ushort)color),
                new TextPayload($"[{P.Prefix}] {message}"),
                UIForegroundPayload.UIForegroundOff),
        });

    public void PrintError(string message)
        => Svc.Chat.Print(new XivChatEntry()
        {
            Type = C.ErrorChatType,
            Message = $"[{P.Prefix}] {message}",
        });

    public async void SendMessage(string message) => await chatBoxMessages.Writer.WriteAsync(message);

    /// <summary>
    /// Clear the queue of messages to send to the chatbox.
    /// </summary>
    public void Clear()
    {
        var reader = chatBoxMessages.Reader;
        while (reader.Count > 0 && reader.TryRead(out var _))
            continue;
    }

    private void FrameworkUpdate(IFramework framework)
    {
        if (chatBoxMessages.Reader.TryRead(out var message))
            Chat.Instance.SendMessage(message);
    }
}
