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
        ImGui.TextWrapped("Use the /send command to simulate keyboard input. This is useful for interacting with UI elements that don't have dedicated click commands.");
        ImGui.Separator();

        // Get all valid virtual keys
        var validKeys = Svc.KeyState.GetValidVirtualKeys().ToHashSet();

        // Create a more organized layout with columns
        var columns = 4;
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
