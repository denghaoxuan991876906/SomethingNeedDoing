using Dalamud.Game.ClientState.Keys;
using ECommons.Automation;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Sends keyboard input to the game.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SendCommand"/> class.
/// </remarks>
public class SendCommand(string text, VirtualKey[] keys, VirtualKey[] modifiers, WaitModifier? waitDuration = null) : MacroCommandBase(text, waitDuration)
{
    private readonly VirtualKey[] keys = keys;
    private readonly VirtualKey[] modifiers = modifiers;

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => false;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        foreach (var key in keys)
            WindowsKeypress.SendKeypress(key, modifiers);

        await PerformWait(token);
    }

    /// <summary>
    /// Parses a send command from text.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>The parsed command.</returns>
    /// <exception cref="MacroSyntaxError">Thrown when the text cannot be parsed as a valid command.</exception>
    public override IMacroCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);

        var match = Regex.Match(text, @"^/send\s+(?<keys>.*?)\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var keyStrings = match.Groups["keys"].Value.Split('+');
        var keys = new[] { ParseVirtualKey(keyStrings[^1]) };
        var modifiers = keyStrings.Length > 1 ? keyStrings[..^1].Select(ParseVirtualKey).ToArray() : [];

        return new SendCommand(text, keys, modifiers, waitMod as WaitModifier);
    }

    private static VirtualKey ParseVirtualKey(string key)
        => Enum.TryParse<VirtualKey>(key, true, out var vk) ? vk : throw new MacroSyntaxError($"Invalid virtual key: {key}");
}
