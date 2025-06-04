using SomethingNeedDoing.Core.Interfaces;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.NativeMacro.Modifiers;
/// <summary>
/// Modifier for specifying conditions in crafting commands.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ConditionModifier"/> class.
/// </remarks>
public class ConditionModifier(string text, string[] conditions, bool isNegated) : MacroModifierBase(text)
{
    private static readonly Regex Regex = new(@"(?<modifier><condition\.(?<not>(not\.|\!))?(?<names>[^>]+)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets the conditions to check for.
    /// </summary>
    public string[] Conditions { get; } = conditions;

    /// <summary>
    /// Gets whether the condition check should be negated.
    /// </summary>
    public bool IsNegated { get; } = isNegated;

    /// <inheritdoc/>
    public static bool TryParse(ref string text, out IMacroModifier modifier)
    {
        var match = Regex.Match(text);
        if (!match.Success)
        {
            modifier = new ConditionModifier(string.Empty, [], false);
            return false;
        }

        var group = match.Groups["modifier"];
        text = text.Remove(group.Index, group.Length);

        var conditions = match.Groups["names"].Value
            .ToLowerInvariant()
            .Split(",")
            .Select(name => name.Trim())
            .Where(name => !string.IsNullOrEmpty(name))
            .ToArray();

        var isNegated = match.Groups["not"].Success;

        modifier = new ConditionModifier(group.Value, conditions, isNegated);
        return true;
    }
}
