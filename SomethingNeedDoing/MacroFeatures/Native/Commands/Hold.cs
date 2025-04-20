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
public class HoldCommand(string text, VirtualKey[] keys, VirtualKey[] modifiers, WaitModifier? waitMod = null) : MacroCommandBase(text, waitMod?.WaitDuration ?? 0)
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
    public static HoldCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);

        var match = Regex.Match(text, @"^/hold\s+(?<keys>.*?)\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var keyStrings = match.Groups["keys"].Value.Split('+');
        var keys = new[] { ParseVirtualKey(keyStrings[^1]) };
        var modifiers = keyStrings.Length > 1 ? keyStrings[..^1].Select(ParseVirtualKey).ToArray() : [];

        return new(text, keys, modifiers, waitMod as WaitModifier);
    }

    private static VirtualKey ParseVirtualKey(string key)
        => Enum.TryParse<VirtualKey>(key, true, out var vk) ? vk : throw new MacroSyntaxError($"Invalid virtual key: {key}");
}
