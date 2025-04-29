using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Waits for a specified duration.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WaitCommand"/> class.
/// </remarks>
public class WaitCommand(string text, WaitModifier? wait) : MacroCommandBase(text, wait)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await Task.Delay(WaitDuration, token);
    }

    /// <summary>
    /// Parses a wait command from text.
    /// </summary>
    public override WaitCommand Parse(string text)
    {
        var match = Regex.Match(text, @"^/wait\s+(?<duration>\d+(?:\.\d+)?)\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var durationInSeconds = double.Parse(match.Groups["duration"].Value, CultureInfo.InvariantCulture);
        var durationInMs = (int)(durationInSeconds * 1000);

        return new(text, durationInMs);
    }
}
