using ECommons.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
public class NativeCommand(string text) : MacroCommandBase(text)
{
    public override bool RequiresFrameworkThread => true;

    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        Chat.Instance.SendMessage(text.StartsWith('/') ? text : $"/e {text}");
        await PerformWait(token);
    }
}
