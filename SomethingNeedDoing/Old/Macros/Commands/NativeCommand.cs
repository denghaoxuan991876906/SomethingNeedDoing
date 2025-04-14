using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Misc;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class NativeCommand : MacroCommand
{
    private NativeCommand(string text, WaitModifier wait) : base(text, wait) { }

    public static NativeCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        return new NativeCommand(text, waitModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        Old.Service.ChatManager.SendMessage($"{(new[] { "/", "<" }.Any(Text.StartsWith) ? Text : $"/e {Text}")}");

        await PerformWait(token);
    }
}
