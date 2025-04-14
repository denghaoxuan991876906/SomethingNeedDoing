using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Macros.Exceptions;
using SomethingNeedDoing.Old.Misc;

namespace SomethingNeedDoing.Old.Macros.Commands;

/// <summary>
/// The /runmacro command.
/// </summary>
internal class RunMacroCommand : MacroCommand
{
    public static string[] Commands => ["runmacro"];
    public static string Description => "Start a macro from within another macro.";
    public static string[] Examples => ["/runmacro \"Sub macro\""];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string macroName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunMacroCommand"/> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="macroName">Macro name.</param>
    /// <param name="wait">Wait value.</param>
    private RunMacroCommand(string text, string macroName, WaitModifier wait)
        : base(text, wait) => this.macroName = macroName;

    /// <summary>
    /// Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static RunMacroCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");

        return new RunMacroCommand(text, nameValue, waitModifier);
    }

    /// <inheritdoc/>
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        var macroNode = C.GetAllNodes().OfType<MacroNode>().FirstOrDefault(macro => macro.Name == macroName);
        if (macroNode == default)
            throw new MacroCommandError("No macro with that name");

        macroNode.Run();

        await PerformWait(token);
    }
}
