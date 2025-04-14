using SomethingNeedDoing.Macros.Native.Modifiers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Macros.Native.Commands;
/// <summary>
/// Executes another macro from within the current macro.
/// </summary>
public class RunMacroCommand(string text, string macroName, WaitModifier? waitMod = null) : MacroCommandBase(text, waitMod?.WaitDuration ?? 0)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        if (C.GetMacroByName(macroName) is { } macro)
            _ = Service.MacroScheduler.StartMacro(macro);
        else
            Svc.Chat.PrintError($"No macro found with name: {macroName}");
        await PerformWait(token);
    }

    /// <summary>
    /// Parses a run macro command from text.
    /// </summary>
    public static RunMacroCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);

        var match = Regex.Match(text, @"^/runmacro\s+(?<name>.*?)\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var macroName = match.Groups["name"].Value.Trim('"');
        return new(text, macroName, waitMod as WaitModifier);
    }
}
