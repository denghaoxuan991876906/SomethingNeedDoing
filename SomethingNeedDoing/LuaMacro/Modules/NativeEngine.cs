using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.NativeMacro;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.LuaMacro.Modules;

/// <summary>
/// Engine for executing native commands.
/// </summary>
public class NativeEngine(IMacroScheduler scheduler, MacroParser parser) : IEngine
{

    public string Name => "Native";

    public async Task ExecuteAsync(string content, CancellationToken cancellationToken = default)
    {
        try
        {
            var commands = parser.Parse(content, scheduler);
            var tempMacro = new TemporaryMacro(content);

            foreach (var command in commands)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                if (command.RequiresFrameworkThread)
                    await Svc.Framework.RunOnTick(() => command.Execute(new MacroContext(tempMacro), cancellationToken), cancellationToken: cancellationToken);
                else
                    await command.Execute(new MacroContext(tempMacro), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error executing native command '{content}': {ex}");
            throw;
        }
    }

    public bool CanExecute(string content) => !string.IsNullOrWhiteSpace(content) && content.StartsWith('/');
}
