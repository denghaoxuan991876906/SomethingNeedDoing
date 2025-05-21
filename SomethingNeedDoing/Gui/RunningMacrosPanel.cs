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

        // Header with collapse button using FontAwesome icons
        FontAwesomeIcon collapseIcon = _isCollapsed ? FontAwesomeHelper.IconExpanded : FontAwesomeHelper.IconCollapsed;
        
        bool collapseClicked = ImGuiX.IconButton(collapseIcon, _isCollapsed ? "Collapse" : "Expand");

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

            // Display the desktop icon properly
            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(center.X, center.Y - 30)); 
            ImGui.PushFont(UiBuilder.IconFont);
            var iconText = FontAwesomeIcon.Desktop.ToIconString();
            var iconSize = ImGui.CalcTextSize(iconText);
            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(-iconSize.X / 2, 0));
            ImGui.TextColored(ImGuiColors.DalamudGrey, iconText);
            ImGui.PopFont();

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

        // Control buttons with proper FontAwesome icons
        var state = _scheduler.GetMacroState(macro.Id);
        FontAwesomeIcon actionIcon = state == MacroState.Paused ? FontAwesomeHelper.IconPlay : FontAwesomeHelper.IconPause;
        string actionText = state == MacroState.Paused ? "Resume" : "Pause";

        bool actionClicked = ImGuiX.IconTextButton(actionIcon, actionText);

        if (actionClicked)
        {
            if (state == MacroState.Paused)
                _scheduler.ResumeMacro(macro.Id);
            else
                _scheduler.PauseMacro(macro.Id);
        }

        ImGui.SameLine();

        bool stopClicked = ImGuiX.IconTextButton(FontAwesomeHelper.IconStop, "Stop");

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

        // Control buttons with proper FontAwesome icons
        var state = _scheduler.GetMacroState(macro.Id);
        FontAwesomeIcon actionIcon = state == MacroState.Paused ? FontAwesomeHelper.IconPlay : FontAwesomeHelper.IconPause;
        string actionText = state == MacroState.Paused ? "Resume" : "Pause";

        bool actionClicked = ImGuiX.IconTextButton(actionIcon, actionText, new Vector2(80, 0));

        if (actionClicked)
        {
            if (state == MacroState.Paused)
                _scheduler.ResumeMacro(macro.Id);
            else
                _scheduler.PauseMacro(macro.Id);
        }

        ImGui.SameLine();
        bool stopClicked = ImGuiX.IconTextButton(FontAwesomeHelper.IconStop, "Stop", new Vector2(80, 0));

        if (stopClicked)
        {
            _scheduler.StopMacro(macro.Id);
        }
    }

    private string GetStatusText(MacroState state)
    {
        FontAwesomeIcon icon = state switch
        {
            MacroState.Completed => FontAwesomeHelper.IconCompletedStatus,
            MacroState.Running => FontAwesomeHelper.IconRunningStatus,
            MacroState.Paused => FontAwesomeHelper.IconPausedStatus,
            MacroState.Error => FontAwesomeHelper.IconErrorStatus,
            _ => FontAwesomeIcon.Circle
        };
        
        ImGui.PushFont(UiBuilder.IconFont);
        string iconText = icon.ToIconString();
        ImGui.PopFont();
        return iconText;
    }
}
