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

        ImGui.Separator();

        // Key features section
        if (ImGui.CollapsingHeader("Key Features", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);

            var features = new[]
            {
                ("Native Macros", "Enhanced version of in-game macros with additional commands and modifiers"),
                ("Lua Scripting", "Full Lua scripting support for complex automation needs"),
                ("UI Automation", "Interact with game UI elements through clicks and keyboard inputs"),
                ("Conditional Logic", "Run commands based on game state using conditions"),
                ("Looping", "Repeat actions multiple times with the loop command"),
                ("Git Integration", "Import and update macros directly from GitHub repositories")
            };

            foreach (var (feature, description) in features)
            {
                ImGui.TextColored(ImGuiColors.DalamudOrange, feature);
                ImGui.SameLine();
                ImGui.TextWrapped(description);
                ImGui.Spacing();
            }

            ImGui.Unindent(10);
        }

        ImGui.Separator();

        // Quick start section
        if (ImGui.CollapsingHeader("Quick Start Guide", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);

            ImGui.TextWrapped("1. Create a new macro using the 'New Macro' button in the main window");
            ImGui.TextWrapped("2. Enter your commands or Lua script in the editor");
            ImGui.TextWrapped("3. Run the macro with the 'Run' button or with the command:");
            ImGui.TextColored(ImGuiColors.DalamudYellow, $"    /{P.Aliases[0]} run MyMacroName");
            ImGui.TextWrapped("4. For more detailed information, check the other tabs in this help window");

            ImGui.Unindent(10);
        }

        ImGui.Separator();

        // Common use cases
        if (ImGui.CollapsingHeader("Common Use Cases", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);

            var useCases = new[]
            {
                ("Crafting Automation", "Create macros that craft multiple items in sequence"),
                ("Gathering Routes", "Automate gathering rotations and teleport sequences"),
                ("UI Navigation", "Navigate through multiple game menus automatically"),
                ("Retainer Management", "Automate sorting, selling, and ventures"),
                ("Custom Triggers", "Set up macros that run automatically on certain game events")
            };

            foreach (var (useCase, description) in useCases)
            {
                ImGui.TextColored(ImGuiColors.DalamudOrange, useCase);
                ImGui.SameLine();
                ImGui.TextWrapped(description);
                ImGui.Spacing();
            }

            ImGui.Unindent(10);
        }

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Features Guide");
        ImGui.TextWrapped("This guide explains the main features and components of SomethingNeedDoing.");
        ImGui.Separator();

        // Macro Types section
        if (ImGui.CollapsingHeader("Macro Types", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);

            // ConfigMacro
            ImGui.TextColored(ImGuiColors.DalamudOrange, "ConfigMacro");
            ImGui.TextWrapped("The standard editable macro type. You can create and edit these directly in the application.");
            ImGui.TextWrapped("ConfigMacros include metadata like author, version, and trigger events.");
            ImGui.Spacing();

            // GitMacro
            ImGui.TextColored(ImGuiColors.DalamudOrange, "GitMacro");
            ImGui.TextWrapped("Macros linked to GitHub repositories. Only the link is editable - content is fetched from GitHub.");
            ImGui.TextWrapped("GitMacros can be configured to automatically update when the source repository changes.");
            ImGui.TextWrapped("To create one, copy a GitHub URL and use the New Macro button.");

            ImGui.Unindent(10);
        }

        // Status Monitoring section
        if (ImGui.CollapsingHeader("Status Monitoring", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);

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

            ImGui.Unindent(10);
        }

        // Scheduling and Triggers section
        if (ImGui.CollapsingHeader("Scheduling and Triggers", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);

            ImGui.TextWrapped("Macros can be scheduled to run automatically based on various game events.");
            ImGui.Spacing();

            ImGui.TextColored(ImGuiColors.DalamudOrange, "Trigger Events");
            ImGui.TextWrapped("Each macro can be configured to trigger on specific events:");
            ImGui.BulletText("OnLogin: Triggers when you log into the game");
            ImGui.BulletText("OnLogout: Triggers when you log out of the game");
            ImGui.BulletText("OnTerritoryChange: Triggers when you change zones");
            ImGui.BulletText("OnCraftingStart: Triggers when you begin crafting");
            ImGui.BulletText("OnCraftingEnd: Triggers when crafting completes");
            ImGui.BulletText("OnAutoRetainerCharacterPostProcess: Triggers after Auto Retainer finishes");
            ImGui.Spacing();

            ImGui.TextColored(ImGuiColors.DalamudOrange, "Manual Scheduling");
            ImGui.TextWrapped("Macros can also be scheduled manually using the command line interface:");
            ImGui.TextWrapped($"/{P.Aliases[0]} run MyMacro - Run a macro immediately");
            ImGui.TextWrapped($"/{P.Aliases[0]} run loop 5 MyMacro - Run a macro and loop it 5 times");

            ImGui.Unindent(10);
        }

        // Folder Organization section
        if (ImGui.CollapsingHeader("Folder Organization", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);

            ImGui.TextWrapped("Macros can be organized into folders for better management.");
            ImGui.Spacing();

            ImGui.TextColored(ImGuiColors.DalamudOrange, "Folder Features");
            ImGui.BulletText("Create folders to group related macros");
            ImGui.BulletText("Move macros between folders using the context menu");
            ImGui.BulletText("Collapse/expand folder sections to manage screen space");
            ImGui.BulletText("Each folder shows a count of contained macros");
            ImGui.Spacing();

            ImGui.TextColored(ImGuiColors.DalamudOrange, "Default Folder");
            ImGui.TextWrapped("A default 'General' folder exists for all macros. If a folder is deleted, its macros move here.");

            ImGui.Unindent(10);
        }
    }
}
