using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.NativeMacro;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.LuaMacro.Modules;

/// <summary>
/// Engine for executing native commands.
/// </summary>
public class NativeEngine(MacroParser parser) : IEngine
{
    /// <summary>
    /// Event raised when a macro execution is requested.
    /// </summary>
    public event EventHandler<MacroExecutionRequestedEventArgs>? MacroExecutionRequested;

    public string Name => "Native";

    public async Task ExecuteAsync(string content, CancellationToken cancellationToken = default)
    {
        try
        {
            var commands = parser.Parse(content);
            var tempMacro = new TemporaryMacro(content);

            foreach (var command in commands)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var context = new MacroContext(tempMacro);

                // Subscribe to macro execution requests from commands
                context.MacroExecutionRequested += (sender, e) =>
                    MacroExecutionRequested?.Invoke(this, e);

                if (command.RequiresFrameworkThread)
                    await Svc.Framework.RunOnTick(() => command.Execute(context, cancellationToken), cancellationToken: cancellationToken);
                else
                    await command.Execute(context, cancellationToken);
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
