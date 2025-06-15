using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Scheduler;

namespace SomethingNeedDoing.Gui;

public class StatusWindow : Window
{
    private readonly IMacroScheduler _scheduler;
    private readonly MacroHierarchyManager _macroHierarchy;
    private readonly TitleBarButton _minimiseBtn;
    private bool _minimised;

    public StatusWindow(IMacroScheduler scheduler, MacroHierarchyManager macroHierarchy) : base($"{P.Name} - Macro Status###{P.Name}_{nameof(StatusWindow)}", ImGuiWindowFlags.NoScrollbar)
    {
        _scheduler = scheduler;
        _macroHierarchy = macroHierarchy;
        Size = new Vector2(400, 200);
        SizeCondition = ImGuiCond.FirstUseEver;
        _minimiseBtn = new TitleBarButton()
        {
            Icon = FontAwesomeIcon.Minus,
            IconOffset = new Vector2(1.5f, 1),
            Priority = int.MinValue,
            Click = _ =>
            {
                _minimised = !_minimised;
                _minimiseBtn!.Icon = _minimised ? FontAwesomeIcon.WindowMaximize : FontAwesomeIcon.Minus;
            },
            ShowTooltip = () => { using var _ = ImRaii.Tooltip(); ImGuiEx.Text(_minimised ? "Show All Macros" : "Show Running Macros Only"); },
            AvailableClickthrough = true,
        };
        TitleBarButtons.Add(_minimiseBtn);
    }

    public override void Draw()
    {
        var macros = _minimised ? _scheduler.GetMacros().Where(m => m.State is MacroState.Running or MacroState.Paused) : _scheduler.GetMacros();
        var parents = macros.Where(m => _macroHierarchy.GetParentMacro(m.Id) == null).ToList();

        foreach (var parent in parents)
        {
            DrawMacro(parent);

            if (_macroHierarchy.GetChildMacros(parent.Id) is { Count: > 0 } children)
                foreach (var childMacro in children)
                    DrawMacro(childMacro, true);
        }
    }

    private void DrawMacro(IMacro macro, bool indent = false)
    {
        using var _ = ImRaii.PushIndent(condition: indent); // TODO: this doesn't work?
        var (statusColor, statusIcon) = GetStatusInfo(macro.State);
        ImGuiEx.Icon(statusColor, statusIcon);
        ImGui.SameLine();
        ImGuiEx.IconWithText(ImGuiUtils.Icons.GetMacroIcon(macro), macro.Name);
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 100);
        DrawControlButtons(macro);
    }

    private void DrawControlButtons(IMacro macro)
    {
        if (macro.State == MacroState.Paused)
        {
            if (ImGuiUtils.IconButton(FontAwesomeIcon.Play, "Resume"))
                _scheduler.ResumeMacro(macro.Id);
        }
        else
        {
            if (ImGuiUtils.IconButton(FontAwesomeIcon.Pause, "Pause"))
                _scheduler.PauseMacro(macro.Id);
        }

        ImGui.SameLine();
        if (ImGuiUtils.IconButton(FontAwesomeIcon.Stop, "Stop"))
            _scheduler.StopMacro(macro.Id);
    }

    private (Vector4 color, FontAwesomeIcon icon) GetStatusInfo(MacroState state) => state switch
    {
        MacroState.Running => (ImGuiColors.HealerGreen, FontAwesomeIcon.Spinner),
        MacroState.Paused => (ImGuiColors.DalamudOrange, FontAwesomeIcon.Pause),
        MacroState.Error => (ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationTriangle),
        MacroState.Completed => (ImGuiColors.ParsedBlue, FontAwesomeIcon.CheckCircle),
        MacroState.Ready => (ImGuiColors.DalamudGrey, FontAwesomeIcon.Circle),
        _ => (ImGuiColors.DalamudGrey, FontAwesomeIcon.QuestionCircle)
    };
}
