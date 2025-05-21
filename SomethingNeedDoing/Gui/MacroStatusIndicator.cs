using Dalamud.Interface.Colors;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using SomethingNeedDoing.Framework.Interfaces;
using SomethingNeedDoing.Utils;
using System;
using System.Numerics;
using System.Linq;

namespace SomethingNeedDoing.Gui;

/// <summary>
/// A compact status indicator that can be embedded in the titlebar or other small spaces
/// to show macro execution status
/// </summary>
public class MacroStatusIndicator
{
    private readonly IMacroScheduler _scheduler;

    public MacroStatusIndicator(IMacroScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public void Draw(float width, float height)
    {
        var runningMacros = _scheduler.GetMacros();
        int macroCount = runningMacros.Count();
        int runningCount = 0;
        int pausedCount = 0;
        int completedCount = 0;
        int errorCount = 0;

        foreach (var macro in runningMacros)
        {
            switch (macro.State)
            {
                case MacroState.Running:
                    runningCount++;
                    break;
                case MacroState.Paused:
                    pausedCount++;
                    break;
                case MacroState.Completed:
                    completedCount++;
                    break;
                case MacroState.Error:
                    errorCount++;
                    break;
            }
        }

        // Draw the background
        var drawList = ImGui.GetWindowDrawList();
        var pos = ImGui.GetCursorScreenPos();
        var statusColor = GetStatusColor(runningCount, pausedCount, errorCount);
        
        drawList.AddRectFilled(pos, pos + new Vector2(width, height), ImGui.ColorConvertFloat4ToU32(statusColor), 4);
        
        // Get appropriate status icon
        FontAwesomeIcon statusIcon = GetStatusIcon(runningCount, pausedCount, completedCount, errorCount);
        string statusText = GetStatusText(macroCount, runningCount, pausedCount, completedCount, errorCount);
        
        // Draw the icon and text with proper font handling
        var padding = 5f;
        float originalCursorPosX = ImGui.GetCursorPosX();
        float originalCursorPosY = ImGui.GetCursorPosY();
        
        // Create a temporary position for positioning the icon and text
        ImGui.SetCursorScreenPos(new Vector2(pos.X + padding, pos.Y + (height - ImGui.GetTextLineHeight()) / 2));
        
        // Draw the icon with the proper font
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextColored(new Vector4(1, 1, 1, 1), statusIcon.ToIconString());
        ImGui.PopFont();
        
        // Get the width of the icon to position the text
        float iconWidth = ImGui.GetItemRectSize().X;
        
        // Draw the text right after the icon
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(1, 1, 1, 1), statusText);
        
        // Restore the original cursor position
        ImGui.SetCursorPos(new Vector2(originalCursorPosX, originalCursorPosY));
    }

    private Vector4 GetStatusColor(int runningCount, int pausedCount, int errorCount)
    {
        if (errorCount > 0)
            return ImGuiColors.DalamudRed;
        if (pausedCount > 0)
            return ImGuiColors.DalamudOrange;
        if (runningCount > 0)
            return ImGuiColors.HealerGreen;
        
        return ImGuiColors.DalamudGrey;
    }
    
    private FontAwesomeIcon GetStatusIcon(int runningCount, int pausedCount, int completedCount, int errorCount)
    {
        if (errorCount > 0)
            return FontAwesomeHelper.IconErrorStatus;
        if (pausedCount > 0)
            return FontAwesomeHelper.IconPausedStatus;
        if (runningCount > 0)
            return FontAwesomeHelper.IconRunning;
        if (completedCount > 0)
            return FontAwesomeHelper.IconCompletedStatus;
            
        return FontAwesomeHelper.IconMacros;
    }

    private string GetStatusText(int macroCount, int runningCount, int pausedCount, int completedCount, int errorCount)
    {
        if (macroCount == 0)
            return "No macros running";
        
        if (errorCount > 0)
            return $"{errorCount} error(s)";
        
        if (pausedCount > 0)
            return $"{pausedCount} paused";
        
        if (runningCount > 0)
            return $"{runningCount} running";
        
        if (completedCount > 0)
            return $"{completedCount} completed";
        
        return $"{macroCount} macros";
    }
}
