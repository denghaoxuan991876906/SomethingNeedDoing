using FFXIVClientStructs.FFXIV.Client.UI;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Old.Macros.Commands.Modifiers;

internal class ConditionModifier : MacroModifier
{
    public static string Modifier => "<condition>";
    public static string Description => "Require a crafting condition to perform the action specified. This is taken from the Synthesis window and may be localized to your client language.";
    public static string[] Examples => ["/ac Observe <condition.poor>", "/ac \"Precise Touch\" <condition.good,excellent>", "/ac \"Byregot's Blessing\" <condition.not.poor>", "/ac \"Byregot's Blessing\" <condition.!poor>"];

    private static readonly Regex Regex = new(@"(?<modifier><condition\.(?<not>(not\.|\!))?(?<names>[^>]+)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string[] conditions;
    private readonly bool negated;

    private ConditionModifier(string[] conditions, bool negated)
    {
        this.conditions = conditions;
        this.negated = negated;
    }

    public static bool TryParse(ref string text, out ConditionModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (success)
        {
            var group = match.Groups["modifier"];
            text = text.Remove(group.Index, group.Length);

            var conditionNames = match.Groups["names"].Value
                .ToLowerInvariant().Split(",")
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray();
            var negated = match.Groups["not"].Success;

            command = new ConditionModifier(conditionNames, negated);
        }
        else
        {
            command = new ConditionModifier([], false);
        }

        return success;
    }

    public unsafe bool HasCondition()
    {
        if (conditions.Length == 0)
            return true;

        var addon = Svc.GameGui.GetAddonByName("Synthesis", 1);
        if (addon == nint.Zero)
        {
            Svc.Log.Debug("Could not find Synthesis addon");
            return true;
        }

        var addonPtr = (AddonSynthesis*)addon;
        var text = addonPtr->Condition->NodeText.ToString().ToLowerInvariant();

        var matchesText = conditions.Any(name => name == text);

        if (negated)
            matchesText ^= true;

        return matchesText;
    }
}
