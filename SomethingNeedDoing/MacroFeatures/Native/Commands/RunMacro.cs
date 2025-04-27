using SomethingNeedDoing.MacroFeatures.Native.Modifiers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Executes another macro from within the current macro.
/// </summary>
public class RunMacroCommand(string text, string macroName, IMacroScheduler scheduler, WaitModifier? waitMod = null) : MacroCommandBase(text, waitMod?.WaitDuration ?? 0)
{
    private readonly string _macroName = macroName;
    private readonly IMacroScheduler _scheduler = scheduler;

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        if (C.GetMacroByName(_macroName) is { } macro)
            _ = _scheduler.StartMacro(macro);
        else
            Svc.Chat.PrintError($"No macro found with name: {_macroName}");
        await PerformWait(token);
    }

    /// <summary>
    /// Parses a run macro command from text.
    /// </summary>
    public static RunMacroCommand Parse(string text, IMacroScheduler scheduler)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);

        var match = Regex.Match(text, @"^/runmacro\s+(?<name>.*?)\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var macroName = match.Groups["name"].Value.Trim('"');
        return new(text, macroName, scheduler, waitMod as WaitModifier);
    }
}
