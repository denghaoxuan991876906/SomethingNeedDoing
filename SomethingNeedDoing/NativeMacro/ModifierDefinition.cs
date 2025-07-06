using System.Text.RegularExpressions;

namespace SomethingNeedDoing.NativeMacro;

/// <summary>
/// Defines a modifier with its name, regex pattern, and parameter extraction logic.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ModifierDefinition"/> class.
/// </remarks>
/// <param name="name">The name of the modifier.</param>
/// <param name="regexPattern">The regex pattern for matching this modifier.</param>
/// <param name="parameterExtractor">The function to extract parameters from a regex match.</param>
/// <param name="isSndSpecific">Whether this modifier is SND-specific.</param>
public class ModifierDefinition(string name, string regexPattern, Func<Match, string> parameterExtractor, bool isSndSpecific = true)
{
    /// <summary>
    /// Gets the name of the modifier.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the regex pattern for matching this modifier.
    /// </summary>
    public string RegexPattern { get; } = regexPattern;

    /// <summary>
    /// Gets the function to extract parameters from a regex match.
    /// </summary>
    public Func<Match, string> ParameterExtractor { get; } = parameterExtractor;

    /// <summary>
    /// Gets whether this modifier is SND-specific (should be extracted from native commands).
    /// </summary>
    public bool IsSndSpecific { get; } = isSndSpecific;
}

/// <summary>
/// Static class containing all modifier definitions.
/// </summary>
public static class ModifierDefinitions
{
    /// <summary>
    /// Gets all SND-specific modifier definitions.
    /// </summary>
    public static readonly ModifierDefinition[] SndModifiers = [
        new("wait", @"<wait\.(?<wait>\d+(?:\.\d+)?)(?:-(?<until>\d+(?:\.\d+)?))?>",
            match => {
                var waitValue = match.Groups["wait"].Value;
                var untilValue = match.Groups["until"].Success ? match.Groups["until"].Value : null;
                return untilValue != null ? $"{waitValue}-{untilValue}" : waitValue;
            }),

        new("maxwait", @"<maxwait\.(?<maxwait>\d+(?:\.\d+)?)>",
            match => match.Groups["maxwait"].Value),

        new("echo", @"<echo>",
            _ => string.Empty),

        new("unsafe", @"<unsafe>",
            _ => string.Empty),

        new("hq", @"<hq>",
            _ => string.Empty),

        new("condition", @"<condition\.(?<not>(not\.|\!))?(?<conditions>[^>]+)>",
            match => {
                var isNegated = match.Groups["not"].Success;
                var conditions = match.Groups["conditions"].Value;
                return isNegated ? $"!{conditions}" : conditions;
            }),

        new("index", @"<index\.(?<index>\d+)>",
            match => match.Groups["index"].Value),

        new("list", @"<list\.(?<list>\d+)>",
            match => match.Groups["list"].Value),

        new("distance", @"<distance\.(?<distance>\d+(?:\.\d+)?)>",
            match => match.Groups["distance"].Value),

        new("errorif", @"<errorif\.(?<errorif>[^>]+)>",
            match => match.Groups["errorif"].Value)
    ];

    /// <summary>
    /// Gets all modifier definitions (both SND-specific and game-supported).
    /// </summary>
    public static readonly ModifierDefinition[] AllModifiers =
    [
        .. SndModifiers,
        new ModifierDefinition("party", @"<(?<party>[1-8])>",
            match => match.Groups["party"].Value, isSndSpecific: false),
    ];

    /// <summary>
    /// Builds a regex pattern from the given modifier definitions.
    /// </summary>
    /// <param name="modifiers">The modifier definitions to include.</param>
    /// <returns>The combined regex pattern.</returns>
    public static string BuildRegexPattern(IEnumerable<ModifierDefinition> modifiers)
    {
        var patterns = modifiers.Select(m => $"(?<{m.Name}>{m.RegexPattern})");
        return string.Join("|", patterns);
    }

    /// <summary>
    /// Finds the modifier definition that matches the given regex match.
    /// </summary>
    /// <param name="match">The regex match.</param>
    /// <param name="modifiers">The modifier definitions to search.</param>
    /// <returns>The matching modifier definition, or null if none found.</returns>
    public static ModifierDefinition? FindMatchingModifier(Match match, IEnumerable<ModifierDefinition> modifiers)
    {
        foreach (var modifier in modifiers)
            if (match.Groups[modifier.Name].Success)
                return modifier;
        return null;
    }
}
