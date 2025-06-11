using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.Automation.UIInput;
using ECommons.ImGuiMethods;

namespace SomethingNeedDoing.Gui.Tabs;
public static class HelpClicksTab
{
    public static void DrawTab()
    {
        ImGuiUtils.Section("Click Commands", () =>
        {
            ImGui.TextWrapped("Click commands can be used to interact with game UI elements. You can use these in your macros.");
            ImGui.TextWrapped("Items in red are properties that themselves have methods (not callable directly).");
        });

        ImGuiUtils.Section("Available Clicks", () =>
        {
            using var _ = ImRaii.Child("ClicksList", new(-1, 300), true);
            foreach (var name in ClickHelper.GetAvailableClicks())
            {
                var isProperty = name.StartsWith('p');
                var displayName = isProperty ? name[1..] : name;
                var color = isProperty ? ImGuiColors.DalamudRed : ImGui.GetStyle().Colors[(int)ImGuiCol.Text];

                using var textColor = ImRaii.PushColor(ImGuiCol.Text, color);
                if (ImGui.Selectable($"/click {displayName}"))
                    ImGui.SetClipboardText($"/click {displayName}");

                ImGuiEx.Tooltip(isProperty ? "This is a property with methods. Cannot be called directly." : "Click to copy to clipboard");
            }
        });
    }
}
