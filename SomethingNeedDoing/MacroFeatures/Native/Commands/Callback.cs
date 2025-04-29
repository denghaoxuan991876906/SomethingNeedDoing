using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Executes a callback on a game addon.
/// </summary>
public class CallbackCommand(string text, string addonName, bool updateState, object[] values, WaitModifier? waitMod = null) : MacroCommandBase(text, waitMod)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.RunOnFramework(() =>
        {
            unsafe
            {
                if (!TryGetAddonByName<AtkUnitBase>(addonName, out var addon))
                {
                    if (C.StopMacroIfAddonNotFound)
                        throw new MacroException($"Addon {addonName} not found");
                    return;
                }

                if (IsAddonReady(addon))
                {
                    Svc.Log.Debug($"Sending callback to {addonName} with args [{string.Join(", ", values)}]");
                    Callback.Fire(addon, updateState, values);
                }
            }
        });

        await PerformWait(token);
    }

    /// <summary>
    /// Parses a callback command from text.
    /// </summary>
    public override CallbackCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);

        var match = Regex.Match(text, @"^/callback\s+(?<addon>\b\w+\b)\s+(?<state>true|false)\s+(?<values>(true|false|\b\w+\b|-?\d+|""[^""]+"")(\s+(true|false|\b\w+\b|-?\d+|""[^""]+""))*)", RegexOptions.Compiled);

        if (!match.Success)
            throw new MacroSyntaxError(text, "Invalid callback format. Use: /callback <addon> <updateState> <values...>");

        var addonName = match.Groups["addon"].Value;
        var updateState = bool.Parse(match.Groups["state"].Value);
        var valuesList = ParseCallbackValues(match.Groups["values"].Value);

        return new(text, addonName, updateState, valuesList, waitMod as WaitModifier);
    }

    private static object[] ParseCallbackValues(string valuesText)
    {
        var values = new List<object>();
        var current = "";
        var inQuotes = false;

        foreach (var token in valuesText.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!inQuotes)
            {
                if (token.StartsWith('"'))
                {
                    inQuotes = true;
                    current = token[1..];
                }
                else
                {
                    values.Add(ParseValue(token));
                }
            }
            else
            {
                if (token.EndsWith('"'))
                {
                    inQuotes = false;
                    current += " " + token[..^1];
                    values.Add(current);
                    current = "";
                }
                else
                {
                    current += " " + token;
                }
            }
        }

        if (!string.IsNullOrEmpty(current))
            throw new MacroSyntaxError(valuesText, "Unclosed quotes in values");

        return [.. values];
    }

    private static object ParseValue(string value) => value switch
    {
        _ when bool.TryParse(value, out var boolValue) => boolValue,
        _ when int.TryParse(value, out var intValue) => intValue,
        _ when uint.TryParse(value.TrimEnd('U', 'u'), out var uintValue) => uintValue,
        _ => value,
    };
}
