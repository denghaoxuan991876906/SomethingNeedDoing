using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using SomethingNeedDoing.Framework.Interfaces;
using SomethingNeedDoing.Utils;
using System.Numerics;
using System.Linq;

namespace SomethingNeedDoing.Gui;

public class RunningMacrosPanel
{
    private readonly IMacroScheduler _scheduler;
    private static bool _isCollapsed = false;

    public RunningMacrosPanel(IMacroScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public void Draw()
    {
        using var panel = ImRaii.Child("RunningMacrosPanel", new Vector2(-1, _isCollapsed ? 30 : 150));
        if (!panel) return;

        // Header with collapse button
        string collapseText = _isCollapsed ? "▼" : "▶";
        
        bool collapseClicked = ImGui.Button($"{collapseText}##collapse");

        if (collapseClicked)
        {
            _isCollapsed = !_isCollapsed;
        }

        ImGui.SameLine();
        ImGui.Text("Running Macros");

        if (_isCollapsed) return;

        ImGui.Separator();

        // Get all running macros
        var runningMacros = _scheduler.GetMacros();
        if (runningMacros.Any())
        {
            foreach (var macro in runningMacros)
            {
                DrawMacroControl(macro);
            }
        }
        else
        {
            ImGui.TextColored(ImGuiColors.DalamudGrey, "No running macros");
        }
    }

    public void DrawDetailed()
    {
        // Get all running macros
        var runningMacros = _scheduler.GetMacros();
        if (!runningMacros.Any())
        {
            var center = ImGui.GetContentRegionAvail() / 2;

            var icon = "◙"; // Simple text replacement for icon
            var iconSize = ImGui.CalcTextSize(icon);
            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(center.X - iconSize.X / 2, center.Y - 30));
            ImGui.TextColored(ImGuiColors.DalamudGrey, icon);

            var text = "No running macros";
            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(center.X - textSize.X / 2, center.Y));
            ImGui.TextColored(ImGuiColors.DalamudGrey, text);
            return;
        }

        // Draw detailed info for each macro
        foreach (var macro in runningMacros)
        {
            string statusText = GetStatusText(macro.State);
            bool isOpen = ImGui.CollapsingHeader($"{statusText} {macro.Name}");

            if (isOpen)
            {
                DrawDetailedMacroInfo(macro);
            }
        }
    }

    private void DrawMacroControl(IMacro macro)
    {
        using var _ = ImRaii.PushId(macro.Id);

        // Macro name and status with a simple text indicator
        string statusText = GetStatusText(macro.State);
        ImGui.Text(statusText);

        ImGui.SameLine();
        ImGui.Text(macro.Name);
        ImGui.SameLine(ImGui.GetWindowWidth() - 200);

        // Control buttons with text instead of icons
        var state = _scheduler.GetMacroState(macro.Id);
        string actionText = state == MacroState.Paused ? "▶ Resume" : "⏸ Pause";

        bool actionClicked = ImGui.Button($"{actionText}##action");

        if (actionClicked)
        {
            if (state == MacroState.Paused)
                _scheduler.ResumeMacro(macro.Id);
            else
                _scheduler.PauseMacro(macro.Id);
        }

        ImGui.SameLine();

        bool stopClicked = ImGui.Button("⏹ Stop##stop");

        if (stopClicked)
        {
            _scheduler.StopMacro(macro.Id);
        }
    }

    private void DrawDetailedMacroInfo(IMacro macro)
    {
        // Draw more detailed information about the macro
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Status:");

        string statusText = GetStatusText(macro.State);
        ImGui.Text(statusText);

        ImGui.SameLine();
        ImGui.Text(macro.State.ToString());

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Control buttons with text
        var state = _scheduler.GetMacroState(macro.Id);
        string actionText = state == MacroState.Paused ? "▶ Resume" : "⏸ Pause";

        bool actionClicked = ImGui.Button($"{actionText}##action", new Vector2(80, 0));

        if (actionClicked)
        {
            if (state == MacroState.Paused)
                _scheduler.ResumeMacro(macro.Id);
            else
                _scheduler.PauseMacro(macro.Id);
        }

        ImGui.SameLine();
        bool stopClicked = ImGui.Button("⏹ Stop##stop", new Vector2(80, 0));

        if (stopClicked)
        {
            _scheduler.StopMacro(macro.Id);
        }
    }

    private string GetStatusText(MacroState state)
    {
        return state switch
        {
            MacroState.Completed => "✓",
            MacroState.Running => "⏵",
            MacroState.Paused => "⏸",
            MacroState.Error => "✗",
            _ => "◌"
        };
    }
}
