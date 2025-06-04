using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace SomethingNeedDoing.Gui.Tabs;
public static class HelpCliTab
{
    public static void DrawTab()
    {
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Command Line Interface (CLI)");
        ImGui.TextWrapped("The following commands can be used in chat or your macro text. You can use any of the aliases: " +
                         string.Join(", ", P.Aliases) + " or /somethingneeddoing");
        ImGui.Separator();

        // Command reference table
        ImGui.BeginTable("CommandReferenceTable", 3, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersInner);

        ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthFixed, 180 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("Example", ImGuiTableColumnFlags.WidthFixed, 200 * ImGuiHelpers.GlobalScale);
        ImGui.TableHeadersRow();

        // CLI command data
        var cliData = new[]
        {
            ("help", "Show the help window.", null),
            ("run", "Run a macro, the name must be unique.", $"{P.Aliases[0]} run MyMacro"),
            ("run loop #", "Run a macro and then loop N times, the name must be unique.", $"{P.Aliases[0]} run loop 5 MyMacro"),
            ("pause", "Pause the currently executing macro.", null),
            ("pause loop", "Pause the currently executing macro at the next loop point.", null),
            ("resume", "Resume the currently paused macro.", null),
            ("stop", "Stop the currently executing macro.", null),
            ("stop loop", "Stop the currently executing macro at the next loop point.", null),
            ("cfg", "Change a configuration value.", $"{P.Aliases[0]} cfg EnableAutoUpdates true"),
        };

        foreach (var (name, desc, example) in cliData)
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.TextUnformatted($"{P.Aliases[0]} {name}");

            ImGui.TableSetColumnIndex(1);
            ImGui.TextWrapped(desc);

            ImGui.TableSetColumnIndex(2);
            if (example != null)
                ImGui.TextColored(ImGuiColors.DalamudOrange, example);

        }

        ImGui.EndTable();

        ImGui.Separator();

        // Advanced examples section
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Advanced Examples:");

        ImGui.BeginTable("AdvancedExamplesTable", 2, ImGuiTableFlags.BordersOuter);
        ImGui.TableSetupColumn("Scenario", ImGuiTableColumnFlags.WidthFixed, 250 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthStretch);

        var advancedExamples = new[]
        {
            ("Run a macro that crafts 5 items:", $"{P.Aliases[0]} run loop 5 MyCraftingMacro"),
            ("Run a macro and pause it at every loop:", $"{P.Aliases[0]} run MyMacro\n{P.Aliases[0]} pause loop"),
            ("Stop all running macros:", $"{P.Aliases[0]} stop"),
            ("Resume a paused macro:", $"{P.Aliases[0]} resume"),
        };

        foreach (var (scenario, command) in advancedExamples)
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.TextWrapped(scenario);

            ImGui.TableSetColumnIndex(1);
            ImGui.TextColored(ImGuiColors.DalamudOrange, command);
        }

        ImGui.EndTable();
    }
}
