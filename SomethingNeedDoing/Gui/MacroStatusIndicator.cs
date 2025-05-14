using Dalamud.Interface.Colors;
using ImGuiNET;
using SomethingNeedDoing.Framework.Interfaces;
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
        
        // Draw the text
        string statusText = GetStatusText(macroCount, runningCount, pausedCount, completedCount, errorCount);
        var textSize = ImGui.CalcTextSize(statusText);
        var textPos = pos + new Vector2((width - textSize.X) / 2, (height - textSize.Y) / 2);
        
        drawList.AddText(textPos, 0xFFFFFFFF, statusText);
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

    private string GetStatusText(int macroCount, int runningCount, int pausedCount, int completedCount, int errorCount)
    {
        if (macroCount == 0)
            return "No macros running";
        
        if (errorCount > 0)
            return $"⚠ {errorCount} error(s)";
        
        if (pausedCount > 0)
            return $"⏸ {pausedCount} paused";
        
        if (runningCount > 0)
            return $"⏵ {runningCount} running";
        
        if (completedCount > 0)
            return $"✓ {completedCount} completed";
        
        return $"{macroCount} macros";
    }
}
