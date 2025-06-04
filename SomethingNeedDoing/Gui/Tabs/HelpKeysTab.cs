using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;

namespace SomethingNeedDoing.Gui.Tabs;
public static class HelpKeysTab
{
    public static void DrawTab()
    {
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Virtual Key Commands");
        ImGui.TextWrapped("The /send command can be used to send virtual key presses to the game.");
        ImGui.TextWrapped("This can be useful for interacting with UI elements that don't have specific click commands.");
        ImGui.TextWrapped("Active keys will highlight green.");
        ImGui.Separator();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Usage Examples:");
        ImGui.Indent(10);
        ImGui.TextWrapped("/send ESC - Press the Escape key");
        ImGui.TextWrapped("/send RETURN - Press the Enter/Return key");
        ImGui.TextWrapped("/send F12 - Press the F12 key");
        ImGui.Unindent(10);
        ImGui.Separator();

        // Get all valid virtual keys
        var validKeys = Svc.KeyState.GetValidVirtualKeys().ToHashSet();

        // Create a more organized layout with columns
        var columns = 3;
        ImGui.Columns(columns, "VirtualKeysColumns", false);

        var counter = 0;

        // Display each key with active state
        foreach (var key in Enum.GetValues<VirtualKey>())
        {
            if (!validKeys.Contains(key))
                continue;

            var isActive = Svc.KeyState[key];
            using var colour = ImRaii.PushColor(ImGuiCol.Text,
                isActive ? ImGuiColors.HealerGreen : ImGui.GetStyle().Colors[(int)ImGuiCol.Text]);

            if (ImGui.Selectable($"/send {key}"))
            {
                ImGui.SetClipboardText($"/send {key}");
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text($"Click to copy to clipboard");
                ImGui.Text($"Current state: {(isActive ? "Active" : "Inactive")}");
                ImGui.EndTooltip();
            }

            counter++;
            if (counter % (validKeys.Count / columns + 1) == 0)
                ImGui.NextColumn();
        }

        ImGui.Columns(1);
    }
}
