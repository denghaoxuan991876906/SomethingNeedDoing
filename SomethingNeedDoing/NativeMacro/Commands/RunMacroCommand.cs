using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.Core.Interfaces;
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
    private readonly IMacroScheduler? _scheduler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunMacroCommand"/> class.
    /// </summary>
    /// <param name="text">The command text.</param>
    /// <param name="macroName">The name of the macro to run.</param>
    /// <param name="scheduler">The macro scheduler.</param>
    public RunMacroCommand(string text, string macroName, IMacroScheduler scheduler) : base(text)
    {
        _macroName = macroName;
        _scheduler = scheduler;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RunMacroCommand"/> class.
    /// </summary>
    /// <param name="text">The command text.</param>
    /// <param name="macroName">The name of the macro to run.</param>
    public RunMacroCommand(string text, string macroName) : base(text)
    {
        _macroName = macroName;
        _scheduler = null;
    }

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        if (C.GetMacroByName(_macroName) is { } macro)
        {
            if (_scheduler != null)
            {
                _ = _scheduler.StartMacro(macro);
            }
            else
            {
                // Raise event for macro execution request
                context.OnMacroExecutionRequested(this, new MacroExecutionRequestedEventArgs(macro));
            }
        }
        else
        {
            Svc.Chat.PrintError($"No macro found with name: {_macroName}");
        }

        await PerformWait(token);
    }

    /// <summary>
    /// Parses a run macro command from text.
    /// </summary>
    //public override RunMacroCommand Parse(string text)
    //{
    //    _ = WaitModifier.TryParse(ref text, out var waitMod);

    //    var match = Regex.Match(text, @"^/runmacro\s+(?<name>.*?)\s*$", RegexOptions.Compiled);
    //    if (!match.Success)
    //        throw new MacroSyntaxError(text);

    //    var macroName = match.Groups["name"].Value.Trim('"');
    //    return new(text, macroName, waitMod as WaitModifier);
    //}
}
