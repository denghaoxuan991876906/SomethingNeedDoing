using Dalamud.Game.ClientState.Keys;
using ECommons.Automation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Holds keyboard keys down until released.
/// </summary>
public class HoldCommand(string text, VirtualKey[] keys, VirtualKey[] modifiers, WaitModifier? waitMod = null) : MacroCommandBase(text, waitMod)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        if (modifiers.Length == 0)
        {
            foreach (var key in keys)
                WindowsKeypress.SendKeyHold(key, null);
        }
        else
        {
            foreach (var key in keys)
                WindowsKeypress.SendKeyHold(key, modifiers);
        }

        await PerformWait(token);
    }

    /// <summary>
    /// Parses a hold command from text.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>The parsed command.</returns>
    /// <exception cref="MacroSyntaxError">Thrown when the text cannot be parsed as a valid command.</exception>
    public override IMacroCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);

        var match = Regex.Match(text, @"^/hold\s+(?<keys>.*?)\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var keyStrings = match.Groups["keys"].Value.Split('+');
        var keys = new[] { ParseVirtualKey(keyStrings[^1]) };
        var modifiers = keyStrings.Length > 1 ? keyStrings[..^1].Select(ParseVirtualKey).ToArray() : [];

        return new HoldCommand(text, keys, modifiers, waitMod as WaitModifier);
    }

    private static VirtualKey ParseVirtualKey(string key)
        => Enum.TryParse<VirtualKey>(key, true, out var vk) ? vk : throw new MacroSyntaxError($"Invalid virtual key: {key}");
}
