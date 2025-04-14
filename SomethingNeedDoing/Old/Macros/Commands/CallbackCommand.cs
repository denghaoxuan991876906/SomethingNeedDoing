using ECommons.Automation;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Misc;
using SomethingNeedDoing.Old.Macros.Exceptions;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class CallbackCommand : MacroCommand
{
    public static string[] Commands => ["callback"];
    public static string Description => "Send arbitrary inputs to most addons in the game.";
    public static string[] Examples => ["/callback AddonName UpdateState [AtkValues]", "/callback FashionCheck true -1"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<addon>\b\w+\b)\s+(?<updateState>true|false)\s+(?<values>(true|false|\b\w+\b|-?\d+|""[^""]+"")(\s+(true|false|\b\w+\b|-?\d+|""[^""]+""))*).*$", RegexOptions.Compiled);
    private readonly unsafe string addon;
    private readonly bool updateState;
    private readonly List<object> valueArgs = [];

    private unsafe CallbackCommand(string addon, bool updateState, List<object> valueArgs, WaitModifier wait) : base("", wait)
    {
        this.addon = addon;
        this.updateState = updateState;
        this.valueArgs = valueArgs;
    }

    public static unsafe CallbackCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);

        var addonGroup = match.Groups["addon"];
        var boolGroup = match.Groups["updateState"];
        var valueGroup = match.Groups["values"];

        if (!addonGroup.Success)
            throw new MacroSyntaxError(text, $"Invalid addon {addonGroup.Value}. Please follow \"/callback <addon> <bool> <atkValues>\"");
        if (!boolGroup.Success || !bool.TryParse(boolGroup.Value, out var boolArg))
            throw new MacroSyntaxError(text, $"Invalid bool {boolGroup.Value}. Please follow \"/callback <addon> <bool> <atkValues>\"");
        if (!valueGroup.Success)
            throw new MacroSyntaxError(text, $"Invalid values {valueGroup.Value}. Please follow \"/callback <addon> <bool> <atkValues>\"");

        var rawValues = valueGroup.Value.Split(' ');
        var valueArgs = new List<object>();

        var current = "";
        var inQuotes = false;

        for (var i = 0; i < rawValues.Length; i++)
        {
            if (!inQuotes)
            {
                if (rawValues[i].StartsWith('\"'))
                {
                    inQuotes = true;
                    current = rawValues[i].TrimStart('"');
                }
                else
                {
                    if (int.TryParse(rawValues[i], out var iValue)) valueArgs.Add(iValue);
                    else if (uint.TryParse(rawValues[i].TrimEnd('U', 'u'), out var uValue)) valueArgs.Add(uValue);
                    else if (bool.TryParse(rawValues[i], out var bValue)) valueArgs.Add(bValue);
                    else valueArgs.Add(rawValues[i]);
                }
            }
            else
            {
                if (rawValues[i].EndsWith('\"'))
                {
                    inQuotes = false;
                    current += " " + rawValues[i].TrimEnd('"');
                    valueArgs.Add(current);
                    current = "";
                }
                else
                {
                    current += " " + rawValues[i];
                }
            }
        }

        if (!string.IsNullOrEmpty(current))
            throw new MacroSyntaxError(text, "Unclosed quotes.");
        return new CallbackCommand(addonGroup.Value, boolArg, valueArgs, waitModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        unsafe
        {
            if (TryGetAddonByName<AtkUnitBase>(addon, out var addonArg))
            {
                if (IsAddonReady(addonArg))
                {
                    Svc.Log.Debug($"Sending callback to {addon} with args [{string.Join(", ", valueArgs)}]");
                    Svc.Framework.RunOnFrameworkThread(() =>
                    {
                        if (IsAddonReady(addonArg))
                            Callback.Fire(addonArg, updateState, [.. valueArgs]);
                    });
                }
            }
            else
            {
                Svc.Log.Debug($"Addon {addon} not found.");
                if (C.StopMacroIfAddonNotFound)
                    throw new MacroCommandError($"Addon {addon} not found.");
            }
        }
        await PerformWait(token);
    }
}
