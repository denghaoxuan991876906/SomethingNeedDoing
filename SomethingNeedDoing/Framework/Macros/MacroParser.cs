using System.Reflection;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Framework;

/// <summary>
/// Parses macro text into commands.
/// </summary>
public static class MacroParser
{
    private static readonly Dictionary<string, Type> CommandTypes = [];
    private static readonly Dictionary<string, Type> ModifierTypes = [];

    /// Registers a command type.
    /// </summary>
    /// <param name="commandType">The command type.</param>
    /// <param name="prefix">The command prefix.</param>
    public static void RegisterCommand(Type commandType, string prefix)
    {
        if (!typeof(IMacroCommand).IsAssignableFrom(commandType))
            throw new ArgumentException($"Type {commandType.Name} does not implement IMacroCommand", nameof(commandType));

        CommandTypes[prefix] = commandType;
    }

    /// <summary>
    /// Registers a modifier type.
    /// </summary>
    /// <param name="modifierType">The modifier type.</param>
    /// <param name="prefix">The modifier prefix.</param>
    public static void RegisterModifier(Type modifierType, string prefix)
    {
        if (!typeof(IMacroModifier).IsAssignableFrom(modifierType))
            throw new ArgumentException($"Type {modifierType.Name} does not implement IMacroModifier", nameof(modifierType));

        ModifierTypes[prefix] = modifierType;
    }

    /// <summary>
    /// Parses a macro text into a list of commands.
    /// </summary>
    /// <param name="text">The macro text.</param>
    /// <returns>A list of commands.</returns>
    public static List<IMacroCommand> Parse(string text)
    {
        var commands = new List<IMacroCommand>();
        var lines = text.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                continue;

            if (ParseLine(trimmedLine) is { } cmd)
                commands.Add(cmd);
        }

        return commands;
    }

    /// <summary>
    /// Parses a single line into a command.
    /// </summary>
    /// <param name="line">The line to parse.</param>
    /// <returns>The parsed command, or null if the line is empty or a comment.</returns>
    /// <exception cref="MacroSyntaxError">Thrown when the line cannot be parsed as a valid command.</exception>
    public static IMacroCommand? ParseLine(string line)
    {
        var trimmedLine = line.Trim();
        if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
            return null;

        // Extract the command prefix
        var match = Regex.Match(trimmedLine, @"^/(\w+)\s*(.*)$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError($"Invalid command format: {line}");

        var prefix = match.Groups[1].Value.ToLowerInvariant();
        var args = match.Groups[2].Value.Trim();

        // Find the command type
        if (!CommandTypes.TryGetValue(prefix, out var commandType))
            throw new MacroSyntaxError($"Unknown command: /{prefix}");

        // Create the command
        try
        {
            // Use reflection to call the static Parse method on the command type
            var parseMethod = commandType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static) ?? throw new MacroSyntaxError($"Command type {commandType.Name} does not have a static Parse method");
            if (parseMethod.Invoke(null, [$"/{prefix} {args}"]) is not IMacroCommand command)
                throw new MacroSyntaxError($"Failed to parse command: /{prefix} {args}");

            return command;
        }
        catch (Exception ex) when (ex is not MacroSyntaxError)
        {
            throw new MacroSyntaxError($"Error parsing command /{prefix}: {ex.Message}");
        }
    }

    /// <summary>
    /// Tries to parse a modifier from text.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <param name="modifier">The parsed modifier, if successful.</param>
    /// <returns>True if the modifier was successfully parsed, false otherwise.</returns>
    public static bool TryParseModifier(ref string text, out IMacroModifier? modifier)
    {
        modifier = null;

        // Try each registered modifier type
        foreach (var modifierType in ModifierTypes.Values)
        {
            try
            {
                // Use reflection to call the static TryParse method on the modifier type
                var tryParseMethod = modifierType.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static);
                if (tryParseMethod == null)
                    continue;

                var parameters = new object[] { text, null };
                var result = (bool)tryParseMethod.Invoke(null, parameters)!;

                if (result)
                {
                    text = (string)parameters[0];
                    modifier = (IMacroModifier)parameters[1]!;
                    return true;
                }
            }
            catch
            {
                // Ignore errors and try the next modifier type
            }
        }

        return false;
    }
}
