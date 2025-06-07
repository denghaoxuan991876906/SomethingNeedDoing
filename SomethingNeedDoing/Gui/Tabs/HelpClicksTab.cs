using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.Automation.UIInput;
using ECommons.ImGuiMethods;

namespace SomethingNeedDoing.Gui.Tabs;
public static class HelpClicksTab
{
    public static void DrawTab()
    {
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Click Commands");
        ImGui.TextWrapped("Click commands can be used to interact with game UI elements. You can use these in your macros.");
        ImGui.TextWrapped("Items in red are properties that themselves have methods (not callable directly).");
        ImGui.Separator();

        // Common scenarios section
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Common Usage Examples:");

        var commonExamples = new[]
        {
            ("Confirm a Yes/No dialog", "/click SelectYesno Yes"),
            ("Cancel a dialog", "/click SelectYesno No"),
            ("Select an option from a dropdown", "/click SelectString 2 (selects the 2nd option)"),
            ("Click on a context menu item", "/click ContextMenu Open\n/click ContextMenu 3 (selects the 3rd item)"),
            ("Click a specific item in your inventory", "/click Inventory 5 (clicks the 5th inventory slot)"),
            ("Press a button in a crafting window", "/click SynthesisResult Synthesize"),
            ("Close the current window", "/click Escape")
        };

        ImGui.BeginTable("ClickExamplesTable", 2, ImGuiTableFlags.Borders);
        ImGui.TableSetupColumn("Scenario", ImGuiTableColumnFlags.WidthFixed, 250 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        foreach (var (scenario, command) in commonExamples)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.TextWrapped(scenario);
            ImGui.TableSetColumnIndex(1);
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudOrange);
            ImGui.TextWrapped(command);
            ImGui.PopStyleColor();
        }

        ImGui.EndTable();
        ImGui.Separator();

        // Get all available clicks from helper method
        var clickNames = ClickHelper.GetAvailableClicks();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Available Click Commands:");
        ImGui.BeginChild("ClicksList", new Vector2(-1, 300), true);

        foreach (var name in clickNames)
        {
            var isProperty = name.StartsWith('p');
            var displayName = isProperty ? name[1..] : name;
            var color = isProperty ? ImGuiColors.DalamudRed : ImGui.GetStyle().Colors[(int)ImGuiCol.Text];

            using var textColor = ImRaii.PushColor(ImGuiCol.Text, color);
            if (ImGui.Selectable($"/click {displayName}"))
                ImGui.SetClipboardText($"/click {displayName}");

            ImGuiEx.Tooltip(isProperty ? "This is a property with methods. Cannot be called directly." : "Click to copy to clipboard");
        }

        ImGui.EndChild();
    }
}
