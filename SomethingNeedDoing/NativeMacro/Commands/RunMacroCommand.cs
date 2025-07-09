using SomethingNeedDoing.Core.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Executes another macro from within the current macro.
/// </summary>
[GenericDoc(
    "Start a macro from within another macro",
    ["macroName"],
    ["/runmacro \"Macro Name\"", "/runmacro \"Macro Name\" <wait.1>"]
)]
public class RunMacroCommand : MacroCommandBase
{
    private readonly string _macroName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunMacroCommand"/> class.
    /// </summary>
    /// <param name="text">The command text.</param>
    /// <param name="macroName">The name of the macro to run.</param>
    public RunMacroCommand(string text, string macroName) : base(text)
    {
        _macroName = macroName;
    }

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        if (C.GetMacroByName(_macroName) is { } macro)
        {
            // Raise event for macro execution request
            context.OnMacroExecutionRequested(this, new MacroExecutionRequestedEventArgs(macro));
        }
        else
        {
            Svc.Chat.PrintError($"No macro found with name: {_macroName}");
        }

        await PerformWait(token);
    }
}
