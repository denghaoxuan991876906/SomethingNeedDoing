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
        ImGui.BeginTable("CommandReferenceTable", 2, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersInner);

        ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthFixed, 180 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        // CLI command data
        var cliData = new[]
        {
            ("help", "Show the help window."),
            ("run", "Run a macro, the name must be unique."),
            ("run loop #", "Run a macro and then loop N times, the name must be unique."),
            ("pause", "Pause the currently executing macro."),
            ("pause loop", "Pause the currently executing macro at the next loop point."),
            ("resume", "Resume the currently paused macro."),
            ("stop", "Stop the currently executing macro."),
            ("stop loop", "Stop the currently executing macro at the next loop point."),
            ("cfg", "Change a configuration value."),
        };

        foreach (var (name, desc) in cliData)
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.TextUnformatted($"{P.Aliases[0]} {name}");

            ImGui.TableSetColumnIndex(1);
            ImGui.TextWrapped(desc);
        }

        ImGui.EndTable();
    }
}
