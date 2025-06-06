using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;

namespace SomethingNeedDoing.Gui.Tabs;
public static class HelpConditionsTab
{
    public static void DrawTab()
    {
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Condition Modifiers");
        ImGui.TextWrapped("These conditions can be used with the <condition> modifier in native macros. For example:");
        ImGui.TextColored(ImGuiColors.DalamudOrange, "/ac \"Byregot's Blessing\" <condition.crafting>");
        ImGui.TextWrapped("This command will only execute if you are currently crafting.");
        ImGui.Separator();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Usage Examples:");
        ImGui.Indent(10);
        ImGui.TextWrapped("/ac \"Inner Quiet\" <condition.crafting> - Only use if crafting");
        ImGui.TextWrapped("/ac \"Veneration\" <condition.crafting> <condition.notboundbydutyfinder> - Only use if crafting and not in a duty");
        ImGui.TextWrapped("/ac \"Standard Step\" <condition.incombat> - Only use if in combat");
        ImGui.Unindent(10);
        ImGui.Separator();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Available Conditions:");
        ImGui.TextWrapped("(Green conditions are currently active)");

        // Create a more organized layout with columns
        var columns = 3;
        ImGui.Columns(columns, "ConditionsColumns", false);

        var counter = 0;
        var totalConditions = Enum.GetValues<ConditionFlag>().Length;

        // Display all game condition flags
        foreach (var condition in Enum.GetValues<ConditionFlag>())
        {
            var name = condition.ToString().ToLower();

            // Skip obsolete conditions
            if (name.Contains("obsolete", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("dummy", StringComparison.OrdinalIgnoreCase))
                continue;

            var isActive = Svc.Condition[condition];
            using var colour = ImRaii.PushColor(ImGuiCol.Text, isActive ? ImGuiColors.HealerGreen : ImGui.GetStyle().Colors[(int)ImGuiCol.Text]);

            if (ImGui.Selectable($"<condition.{name}>"))
                ImGui.SetClipboardText($"<condition.{name}>");

            ImGuiEx.Tooltip($"Current state: {(isActive ? "Active" : "Inactive")}\n" + $"Usage example: /ac \"Some Action\" <condition.{name}>");

            counter++;
            if (counter % (totalConditions / columns + 1) == 0)
                ImGui.NextColumn();
        }

        ImGui.Columns(1);
    }
}
