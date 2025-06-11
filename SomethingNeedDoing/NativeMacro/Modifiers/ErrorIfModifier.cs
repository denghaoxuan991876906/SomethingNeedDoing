using SomethingNeedDoing.Core.Interfaces;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.NativeMacro.Modifiers;

/// <summary>
/// Represents the different error conditions that can be checked.
/// </summary>
public enum ErrorCondition
{
    /// <summary>
    /// Error when an action times out.
    /// </summary>
    ActionTimeout,

    /// <summary>
    /// Error when an item is not found.
    /// </summary>
    ItemNotFound,

    /// <summary>
    /// Error when an item cannot be used.
    /// </summary>
    CantUseItem,

    /// <summary>
    /// Error when a target is not found.
    /// </summary>
    TargetNotFound,

    /// <summary>
    /// Error when an addon is not found.
    /// </summary>
    AddonNotFound
}

/// <summary>
/// Modifier for specifying error conditions that should stop the macro.
/// </summary>
[GenericDoc(
    "Stop the macro if a specific error condition occurs",
    ["condition"],
    ["<errorif.actiontimeout>", "<errorif.itemnotfound>", "<errorif.cantuseitem>", "<errorif.targetnotfound>", "<errorif.addonnotfound>"]
)]
public class ErrorIfModifier(string text, ErrorCondition condition) : MacroModifierBase(text)
{
    private static readonly Regex Regex = new(@"(?<modifier><errorif\.(?<condition>[^>]+)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets the error condition to check for.
    /// </summary>
    public ErrorCondition Condition { get; } = condition;

    /// <summary>
    /// Tries to parse an error if modifier from the given text.
    /// </summary>
    public static bool TryParse(ref string text, out IMacroModifier modifier)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (success)
        {
            var group = match.Groups["modifier"];
            text = text.Remove(group.Index, group.Length);
            var conditionStr = match.Groups["condition"].Value;

            if (Enum.TryParse<ErrorCondition>(conditionStr, true, out var condition))
            {
                modifier = new ErrorIfModifier(match.Value, condition);
                return true;
            }
        }

        modifier = new ErrorIfModifier(string.Empty, ErrorCondition.ActionTimeout);
        return false;
    }
}
