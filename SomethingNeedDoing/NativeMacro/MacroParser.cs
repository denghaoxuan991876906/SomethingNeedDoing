using System.Globalization;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Keys;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.NativeMacro.Commands;
using SomethingNeedDoing.NativeMacro.Modifiers;

namespace SomethingNeedDoing.NativeMacro;

/// <summary>
/// A two-step parser that first extracts modifiers and then parses the command.
/// </summary>
public class MacroParser
{
    // Combined regex pattern for all modifiers
    private static readonly Regex ModifierRegex = new(
        @"(?<modifier>" +
        @"<wait\.(?<wait>\d+(?:\.\d+)?)(?:-(?<until>\d+(?:\.\d+)?))?>" + "|" +  // Wait modifier
        @"<maxwait\.(?<maxwait>\d+(?:\.\d+)?)>" + "|" +                         // MaxWait modifier
        @"<echo>" + "|" +                                                        // Echo modifier
        @"<unsafe>" + "|" +                                                      // Unsafe modifier
        @"<condition\.(?<not>(not\.|\!))?(?<conditions>[^>]+)>" + "|" +         // Condition modifier
        @"<index\.(?<index>\d+)>" + "|" +                                       // Index modifier
        @"<list\.(?<list>\d+)>" + "|" +                                         // List modifier
        @"<(?<party>[1-8])>" + "|" +                                            // Party index modifier
        @"<distance\.(?<distance>\d+(?:\.\d+)?)>" + "|" +                       // Distance modifier
        @"<hq>" + "|" +                                                         // Item quality modifier
        @"<errorif\.(?<errorif>[^>]+)>" +                                       // ErrorIf modifier
        @")",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public List<IMacroCommand> Parse(string text, IMacroScheduler scheduler) => [.. text.Split('\n').Select(line => ParseLine(line, scheduler))];

    /// <summary>
    /// Parses a command line and returns the appropriate command.
    /// </summary>
    /// <param name="text">The command line to parse.</param>
    /// <param name="scheduler">The macro scheduler to use for commands that need it.</param>
    /// <returns>The parsed command.</returns>
    /// <exception cref="MacroSyntaxError">Thrown when the command line cannot be parsed.</exception>
    public IMacroCommand ParseLine(string text, IMacroScheduler scheduler)
    {
        // First pass: Extract modifiers from the text
        var (textWithoutModifiers, modifiers) = ExtractModifiers(text);
        Svc.Log.Verbose($"Extracted modifiers: {string.Join(", ", modifiers)}. Leftover text: [{textWithoutModifiers}]");

        // Second pass: Parse the command structure without modifiers
        var commandInfo = ParseCommandStructure(textWithoutModifiers) ?? throw new MacroSyntaxError(text);

        // Create command based on command type
        var command = CreateCommand(commandInfo, scheduler);

        // Apply modifiers to the command
        ApplyModifiers(modifiers, command);

        return command;
    }

    /// <summary>
    /// Extracts modifiers from the command text.
    /// </summary>
    /// <param name="text">The command text.</param>
    /// <returns>A tuple containing the text without modifiers and the list of modifiers.</returns>
    private (string TextWithoutModifiers, List<ModifierInfo> Modifiers) ExtractModifiers(string text)
    {
        var modifiers = new List<ModifierInfo>();
        var textWithoutModifiers = text;

        // Find all modifiers in the text from right to left
        var matches = ModifierRegex.Matches(text).Cast<Match>().Reverse();
        foreach (var match in matches)
        {
            var modifierGroup = match.Groups["modifier"];
            var modifierText = modifierGroup.Value;

            // Extract modifier name and parameters based on which groups are present
            string modifierName;
            var modifierParam = string.Empty;

            if (match.Groups["wait"].Success)
            {
                modifierName = "wait";
                var waitValue = match.Groups["wait"].Value;
                var untilValue = match.Groups["until"].Success ? match.Groups["until"].Value : null;
                modifierParam = untilValue != null ? $"{waitValue}-{untilValue}" : waitValue;
            }
            else if (match.Groups["maxwait"].Success)
            {
                modifierName = "maxwait";
                modifierParam = match.Groups["maxwait"].Value;
            }
            else if (match.Groups["conditions"].Success)
            {
                modifierName = "condition";
                var isNegated = match.Groups["not"].Success;
                var conditions = match.Groups["conditions"].Value;
                modifierParam = isNegated ? $"!{conditions}" : conditions;
            }
            else if (match.Groups["index"].Success)
            {
                modifierName = "index";
                modifierParam = match.Groups["index"].Value;
            }
            else if (match.Groups["list"].Success)
            {
                modifierName = "list";
                modifierParam = match.Groups["list"].Value;
            }
            else if (match.Groups["party"].Success)
            {
                modifierName = "party";
                modifierParam = match.Groups["party"].Value;
            }
            else if (match.Groups["distance"].Success)
            {
                modifierName = "distance";
                modifierParam = match.Groups["distance"].Value;
            }
            else if (modifierText == "<echo>")
                modifierName = "echo";
            else if (modifierText == "<unsafe>")
                modifierName = "unsafe";
            else if (modifierText == "<hq>")
                modifierName = "hq";
            else if (match.Groups["errorif"].Success)
            {
                modifierName = "errorif";
                modifierParam = match.Groups["errorif"].Value;
                Svc.Log.Debug($"Found errorif modifier with param: {modifierParam}");
            }
            else
            {
                Svc.Log.Warning($"Unknown modifier: {modifierText}");
                continue;
            }

            // Remove the modifier from the text
            textWithoutModifiers = textWithoutModifiers.Remove(modifierGroup.Index, modifierGroup.Length);

            // Add the modifier info
            modifiers.Add(new ModifierInfo(modifierName, modifierParam, modifierText));
        }

        return (textWithoutModifiers.Trim(), modifiers);
    }

    /// <summary>
    /// Parses the command structure from the text.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>The command parse info, or null if the text cannot be parsed.</returns>
    private CommandParseInfo? ParseCommandStructure(string text)
    {
        // Match the basic command pattern
        var match = Regex.Match(text, @"^/(\w+)(?:\s+(.*))?$");
        if (!match.Success)
            return null;

        var commandName = match.Groups[1].Value.ToLowerInvariant();
        var parameters = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;

        return new CommandParseInfo(commandName, parameters, text);
    }

    /// <summary>
    /// Creates a command based on the command parse info.
    /// </summary>
    /// <param name="info">The command parse info.</param>
    /// <param name="scheduler">The macro scheduler to use for commands that need it.</param>
    /// <returns>The created command.</returns>
    /// <exception cref="MacroSyntaxError">Thrown when the command cannot be created.</exception>
    private IMacroCommand CreateCommand(CommandParseInfo info, IMacroScheduler scheduler)
    {
        // Create appropriate command based on command name
        return info.CommandName switch
        {
            "target" => ParseTargetCommand(info.Parameters),
            "action" or "ac" => ParseActionCommand(info.Parameters),
            "callback" => ParseCallbackCommand(info.Parameters),
            "click" => ParseClickCommand(info.Parameters),
            "craft" => ParseGateCommand(info.Parameters),
            "wait" => ParseWaitCommand(info.Parameters),
            "loop" => ParseLoopCommand(info.Parameters, scheduler),
            "gate" => ParseGateCommand(info.Parameters),
            "item" => ParseItemCommand(info.Parameters),
            "keyitem" => ParseKeyItemCommand(info.Parameters),
            "recipe" => ParseRecipeCommand(info.Parameters),
            "require" => ParseRequireCommand(info.Parameters),
            "runmacro" => ParseRunMacroCommand(info.Parameters, scheduler),
            "send" => ParseKeyCommand<SendCommand>(info.Parameters),
            "hold" => ParseKeyCommand<HoldCommand>(info.Parameters),
            "release" => ParseKeyCommand<ReleaseCommand>(info.Parameters),
            "interact" => ParseInteractCommand(info.Parameters),
            "equipitem" => ParseEquipItemCommand(info.Parameters),
            "targetenemy" => ParseTargetEnemyCommand(info.Parameters),
            "waitaddon" => ParseWaitAddonCommand(info.Parameters),
            _ => ParseNativeCommand(info),
        };
    }

    /// <summary>
    /// Applies modifiers to the command.
    /// </summary>
    /// <param name="modifiers">The modifiers to apply.</param>
    /// <param name="command">The command to apply modifiers to.</param>
    private void ApplyModifiers(List<ModifierInfo> modifiers, IMacroCommand command)
    {
        var commandInfo = new CommandParseInfo(command.CommandText, string.Empty, string.Empty);

        foreach (var modifier in modifiers)
        {
            var macroModifier = CreateModifier(modifier, commandInfo);

            switch (macroModifier)
            {
                case WaitModifier waitMod:
                    command.WaitModifier = waitMod;
                    if (command is MacroCommandBase baseCommand)
                        baseCommand.WaitDuration = waitMod.WaitDuration;
                    break;
                case EchoModifier echoMod:
                    command.EchoModifier = echoMod;
                    break;
                case UnsafeModifier unsafeMod:
                    command.UnsafeModifier = unsafeMod;
                    break;
                case ConditionModifier conditionMod:
                    command.ConditionModifier = conditionMod;
                    break;
                case MaxWaitModifier maxWaitMod:
                    command.MaxWaitModifier = maxWaitMod;
                    break;
                case IndexModifier indexMod:
                    command.IndexModifier = indexMod;
                    break;
                case ListIndexModifier listMod:
                    command.ListIndexModifier = listMod;
                    break;
                case PartyIndexModifier partyMod:
                    command.PartyIndexModifier = partyMod;
                    break;
                case DistanceModifier distanceMod:
                    command.DistanceModifier = distanceMod;
                    break;
                case ItemQualityModifier qualityMod:
                    command.ItemQualityModifier = qualityMod;
                    break;
                case ErrorIfModifier errorIfMod:
                    command.ErrorIfModifier = errorIfMod;
                    break;
            }
        }
    }

    #region Command Parsing
    private NativeCommand ParseNativeCommand(CommandParseInfo info) => new($"/{info.CommandName} {info.Parameters}".TrimEnd());

    private TargetCommand ParseTargetCommand(string parameters)
    {
        var match = Regex.Match(parameters, @"^(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        if (!match.Success)
            throw new MacroSyntaxError(parameters);

        var nameValue = match.ExtractAndUnquote("name");
        return new TargetCommand(parameters, nameValue);
    }

    private ActionCommand ParseActionCommand(string parameters)
    {
        var match = Regex.Match(parameters, @"^(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        if (!match.Success)
            throw new MacroSyntaxError(parameters);

        var nameValue = match.ExtractAndUnquote("name");
        var fullCommandText = $"/ac \"{nameValue}\"";
        return new ActionCommand(fullCommandText, nameValue);
    }

    private CallbackCommand ParseCallbackCommand(string parameters)
    {
        var match = Regex.Match(parameters, @"^(?<addon>.*?)\s+(?<update>.*?)\s+(?<values>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        if (!match.Success)
            throw new MacroSyntaxError(parameters);

        var addonValue = match.ExtractAndUnquote("addon");
        var updateValue = bool.Parse(match.ExtractAndUnquote("update"));
        var valuesText = match.Groups["values"].Value;

        var values = ParseCallbackValues(valuesText);
        return new CallbackCommand(parameters, addonValue, updateValue, values);

        static object[] ParseCallbackValues(string valuesText)
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
                        values.Add(ParseValue(token));
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
                        current += " " + token;
                }
            }

            if (!string.IsNullOrEmpty(current))
                throw new MacroSyntaxError(valuesText, "Unclosed quotes in values");

            return [.. values];
        }

        static object ParseValue(string value) => value switch
        {
            _ when bool.TryParse(value, out var boolValue) => boolValue,
            _ when int.TryParse(value, out var intValue) => intValue,
            _ when uint.TryParse(value.TrimEnd('U', 'u'), out var uintValue) => uintValue,
            _ => value,
        };
    }

    private ClickCommand ParseClickCommand(string parameters)
    {
        var parts = parameters.Split([' '], 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            throw new MacroSyntaxError($"Invalid click command: {parameters}");

        var addonName = parts[0];
        var method = parts[1];
        var methodParams = parts.Length > 2 ? parts[2].Split(' ') : [];

        return new ClickCommand(parameters, addonName, method, methodParams);
    }

    private WaitCommand ParseWaitCommand(string parameters)
    {
        if (double.TryParse(parameters, CultureInfo.InvariantCulture, out var @double))
            return new WaitCommand(parameters) { WaitDuration = (int)(@double * 1000) };
        if (int.TryParse(parameters, CultureInfo.InvariantCulture, out var @int))
            return new WaitCommand(parameters) { WaitDuration = @int * 1000 };
        throw new MacroSyntaxError($"Invalid wait command: {parameters}");
    }

    private LoopCommand ParseLoopCommand(string parameters, IMacroScheduler scheduler)
    {
        if (string.IsNullOrEmpty(parameters))
            return new LoopCommand(parameters, int.MaxValue, scheduler);

        var count = int.Parse(parameters);
        return new LoopCommand(parameters, count, scheduler);
    }

    private GateCommand ParseGateCommand(string parameters)
    {
        if (string.IsNullOrEmpty(parameters))
            return new GateCommand(parameters, int.MaxValue);

        var count = int.Parse(parameters);
        return new GateCommand(parameters, count);
    }

    private ItemCommand ParseItemCommand(string parameters)
    {
        var itemName = parameters.Trim('"');
        return new ItemCommand(parameters, itemName);
    }

    private KeyItemCommand ParseKeyItemCommand(string parameters)
    {
        var itemName = parameters.Trim('"');
        return new KeyItemCommand(parameters, itemName);
    }

    private RecipeCommand ParseRecipeCommand(string parameters)
    {
        var recipeName = parameters.Trim('"');
        return new RecipeCommand(parameters, recipeName);
    }

    private RequireCommand ParseRequireCommand(string parameters)
    {
        var conditions = parameters.Split(',').Select(c => c.Trim()).ToArray();
        return new RequireCommand(parameters, conditions);
    }

    private RunMacroCommand ParseRunMacroCommand(string parameters, IMacroScheduler scheduler)
    {
        var macroName = parameters.Trim('"');
        return new RunMacroCommand(parameters, macroName, scheduler);
    }

    private IMacroCommand ParseKeyCommand<T>(string parameters) where T : class, IMacroCommand
    {
        static VirtualKey ParseVirtualKey(string key) => Enum.TryParse<VirtualKey>(key, true, out var vk) ? vk : throw new MacroSyntaxError($"Invalid virtual key: {key}");

        var keyStrings = parameters.Split('+');
        var keys = new[] { ParseVirtualKey(keyStrings[^1]) };
        var modifiers = keyStrings.Length > 1 ? keyStrings[..^1].Select(ParseVirtualKey).ToArray() : [];
        return typeof(T) switch
        {
            Type type when type == typeof(SendCommand) => new SendCommand(parameters, keys, modifiers),
            Type type when type == typeof(HoldCommand) => new HoldCommand(parameters, keys, modifiers),
            Type type when type == typeof(ReleaseCommand) => new ReleaseCommand(parameters, keys, modifiers),
            _ => throw new MacroSyntaxError($"Invalid key command: {parameters}"),
        };
    }

    private InteractCommand ParseInteractCommand(string parameters)
    {
        return new InteractCommand(parameters);
    }

    private EquipItemCommand ParseEquipItemCommand(string parameters)
    {
        var itemId = uint.Parse(parameters);
        return new EquipItemCommand(parameters, itemId);
    }

    private TargetEnemyCommand ParseTargetEnemyCommand(string parameters)
    {
        return new TargetEnemyCommand(parameters);
    }

    private WaitAddonCommand ParseWaitAddonCommand(string parameters)
    {
        var addonName = parameters.Trim('"');
        return new WaitAddonCommand(parameters, addonName);
    }
    #endregion

    #region Modifier Parsing
    /// <summary>
    /// Creates a modifier instance based on the modifier info.
    /// </summary>
    /// <param name="info">The modifier info.</param>
    /// <param name="commandInfo">The command parse info.</param>
    /// <returns>The created modifier.</returns>
    private IMacroModifier CreateModifier(ModifierInfo info, CommandParseInfo commandInfo)
    {
        var text = info.OriginalText;
        return info.Name.ToLowerInvariant() switch
        {
            "wait" => WaitModifier.TryParse(ref text, out var waitModifier) ? waitModifier : throw new MacroSyntaxError($"Invalid wait modifier: {info.Parameter}"),
            "echo" => new EchoModifier(info.OriginalText, true),
            "unsafe" => new UnsafeModifier(info.OriginalText, true),
            "condition" => ConditionModifier.TryParse(ref text, out var conditionModifier) ? conditionModifier : throw new MacroSyntaxError($"Invalid condition modifier: {info.Parameter}"),
            "maxwait" => MaxWaitModifier.TryParse(ref text, out var maxWaitModifier) ? maxWaitModifier : throw new MacroSyntaxError($"Invalid maxwait modifier: {info.Parameter}"),
            "index" => IndexModifier.TryParse(ref text, out var indexModifier) ? indexModifier : throw new MacroSyntaxError($"Invalid index modifier: {info.Parameter}"),
            "list" => ListIndexModifier.TryParse(ref text, out var listIndexModifier) ? listIndexModifier : throw new MacroSyntaxError($"Invalid list index modifier: {info.Parameter}"),
            "party" => PartyIndexModifier.TryParse(ref text, out var partyIndexModifier) ? partyIndexModifier : throw new MacroSyntaxError($"Invalid party index modifier: {info.Parameter}"),
            "distance" => DistanceModifier.TryParse(ref text, out var distanceModifier) ? distanceModifier : throw new MacroSyntaxError($"Invalid distance modifier: {info.Parameter}"),
            "hq" => new ItemQualityModifier(info.OriginalText, true),
            "errorif" => ErrorIfModifier.TryParse(ref text, out var errorIfModifier) ? errorIfModifier : throw new MacroSyntaxError($"Invalid error condition: {info.Parameter}"),
            _ => throw new ArgumentException($"Unknown modifier type: {info.Name}")
        };
    }
    #endregion
}
