using Dalamud.Interface.Colors;

namespace SomethingNeedDoing.Gui.Tabs;
public static class HelpGeneralTab
{
    public static void DrawTab()
    {
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Something Need Doing - Macro Helper");

        // Introduction section
        ImGui.TextWrapped(
            "Something Need Doing is a powerful macro automation tool for FFXIV that extends the game's native macro " +
            "system with additional features like loops, waits, and Lua scripting.");

        ImGuiUtils.Section("Status Monitoring", () =>
        {
            ImGui.TextWrapped("The Status Window provides real-time information about running macros.");
            ImGui.Spacing();

            ImGui.TextColored(ImGuiColors.DalamudOrange, "Macro States");
            ImGui.BulletText("Ready: Macro has been loaded but hasn't started running");
            ImGui.BulletText("Running: Macro is currently executing");
            ImGui.BulletText("Paused: Macro execution has been temporarily stopped");
            ImGui.BulletText("Completed: Macro has finished execution");
            ImGui.BulletText("Failed: Macro encountered an error during execution");
            ImGui.Spacing();

            ImGui.TextColored(ImGuiColors.DalamudOrange, "Status Window");
            ImGui.TextWrapped("The status window can be accessed in several ways:");
            ImGui.BulletText("Click the status indicator in the editor toolbar");
            ImGui.BulletText("Use the /sndstatus command");
            ImGui.BulletText("Toggle between compact and detailed views with the button in the title bar");
        });

        ImGuiUtils.Section("Trigger Events", () =>
        {
            ImGui.TextWrapped("Macros can be configured to trigger automatically based on specific game events:");
            ImGui.BulletText("OnLogin: Triggers when you log into the game");
            ImGui.BulletText("OnLogout: Triggers when you log out of the game");
            ImGui.BulletText("OnTerritoryChange: Triggers when you change zones");
            ImGui.BulletText("OnCraftingStart: Triggers when you begin crafting");
            ImGui.BulletText("OnCraftingEnd: Triggers when crafting completes");
            ImGui.BulletText("OnAutoRetainerCharacterPostProcess: Triggers after Auto Retainer finishes");
        });

        ImGuiUtils.Section("Plugin Features", () =>
        {
            ImGui.TextColored(ImGuiColors.DalamudOrange, "Core Features");
            ImGui.BulletText("Enhanced native macro system with additional commands and modifiers");
            ImGui.BulletText("Full Lua scripting support for complex automation");
            ImGui.BulletText("UI automation through clicks and keyboard inputs");
            ImGui.BulletText("Conditional logic using game state conditions");
            ImGui.BulletText("Looping and wait commands for precise timing");
            ImGui.BulletText("Git integration for macro sharing and updates");
        });
    }
}
