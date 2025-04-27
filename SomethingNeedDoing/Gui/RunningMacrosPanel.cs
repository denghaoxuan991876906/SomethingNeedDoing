using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;

namespace SomethingNeedDoing.Gui;

public class RunningMacrosPanel(IMacroScheduler scheduler)
{
    private readonly IMacroScheduler _scheduler = scheduler;
    private static bool isCollapsed = false;

    public void Draw()
    {
        using var panel = ImRaii.Child("RunningMacrosPanel", new Vector2(-1, isCollapsed ? 30 : 150));
        if (!panel) return;

        // Header with collapse button
        if (ImGui.Button(isCollapsed ? "v Running Macros" : "^ Running Macros"))
            isCollapsed = !isCollapsed;

        if (isCollapsed) return;

        ImGui.Separator();

        // Get all running and enabled macros
        var runningMacros = _scheduler.GetRunningMacros();
        var enabledMacros = C.Macros.Where(m => m.Metadata.TriggerEvents.HasAny());
        // Draw running macros section
        if (runningMacros.Any())
        {
            ImGui.TextColored(EzColor.OrangeBright.Vector4, "Running Macros:");
            foreach (var macro in runningMacros)
            {
                DrawMacroControl(macro, true);
            }
        }

        // Draw enabled macros section
        if (enabledMacros.HasAny())
        {
            if (runningMacros.HasAny()) ImGui.Spacing();
            ImGui.TextColored(EzColor.GreenBright.Vector4, "Enabled Macros:");
            foreach (var macro in enabledMacros)
            {
                DrawMacroControl(macro, false);
            }
        }

        if (!runningMacros.HasAny() && !enabledMacros.HasAny())
        {
            ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1f), "No running or enabled macros");
        }
    }

    private void DrawMacroControl(IMacro macro, bool isRunning)
    {
        using var _ = ImRaii.PushId(macro.Id);

        // Macro name and status
        ImGui.Text(macro.Name);
        ImGui.SameLine(ImGui.GetWindowWidth() - 200);
        if (macro is not ConfigMacro m) return;
        // Control buttons
        if (isRunning)
        {
            var state = _scheduler.GetMacroState(macro.Id);
            if (ImGui.Button(state == MacroState.Paused ? "Resume" : "Pause"))
            {
                if (state == MacroState.Paused)
                    _scheduler.ResumeMacro(m.Id);
                else
                    _scheduler.PauseMacro(m.Id);
            }
            ImGui.SameLine();
            if (ImGui.Button("Stop"))
                _scheduler.StopMacro(m.Id);
        }
        else
        {
            if (ImGui.Button("Disable"))
            {
                m.Metadata.TriggerEvents.Clear();
                C.Save();
            }
        }
    }
}
