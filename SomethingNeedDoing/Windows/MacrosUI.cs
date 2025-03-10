using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.SimpleGui;
using ImGuiNET;
using SomethingNeedDoing.Interface;
using SomethingNeedDoing.Misc;
using System;
using System.Numerics;

namespace SomethingNeedDoing.Windows;
public class MacrosUI : Window
{
    private static NodeDrawing NodesUI = null!;
    public MacrosUI() : base($"Something Need Doing {P.GetType().Assembly.GetName().Version}###SomethingNeedDoing")
    {
        Size = new Vector2(525, 600);
        SizeCondition = ImGuiCond.FirstUseEver;
        RespectCloseHotkey = false;
        //LockButton = new()
        //{
        //    Click = OnLockButtonClick,
        //    Icon = C.LockWindow ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen,
        //    IconOffset = new(3, 2),
        //    ShowTooltip = () => ImGui.SetTooltip("Lock window position and size"),
        //};
        //TitleBarButtons.Add(LockButton);
        NodesUI = new NodeDrawing();
    }

    public override void Draw()
    {
        //using var tabs = ImRaii.TabBar("MacrosSelector");
        //using (var tab = ImRaii.TabItem("Native"))
        //    if (tab)
        //    {
        using var table = ImRaii.Table("Native", 2, ImGuiTableFlags.SizingStretchProp);
        if (!table) return;
        ImGui.TableNextColumn();
        NodesUI.DisplayNodeTree();
        ImGui.TableNextColumn();
        DrawStateHeader();
        DrawRunningMacro();
        NodesUI.DrawSelected();
        //    }
        //using (var tab = ImRaii.TabItem("Disk"))
        //    if (tab)
        //    {
        //        FS.Selector.Draw(200f);
        //        ImGui.SameLine();
        //        using var group = ImRaii.Group();
        //        DrawStateHeader();
        //        DrawRunningMacro();
        //        FS.Selector.DrawSelected();
        //    }
    }

    private static void DrawStateHeader()
    {
        ImGui.TextUnformatted("Macro Queue");

        var state = Service.MacroManager.State;

        var stateName = state switch
        {
            LoopState.NotLoggedIn => "Not Logged In",
            LoopState.Running when Service.MacroManager.PauseAtLoop => "Pausing Soon",
            LoopState.Running when Service.MacroManager.StopAtLoop => "Stopping Soon",
            _ => Enum.GetName(state),
        };

        var buttonCol = ImGuiX.GetStyleColorVec4(ImGuiCol.Button);
        using (var _ = ImRaii.PushColor(ImGuiCol.ButtonActive, buttonCol).Push(ImGuiCol.ButtonHovered, buttonCol))
            ImGui.Button($"{stateName}##LoopState", new Vector2(100, 0));

        ImGui.SameLine();
        if (ImGuiX.IconButton(FontAwesomeIcon.QuestionCircle, "Help"))
            EzConfigGui.GetWindow<HelpUI>()!.Toggle();
        ImGui.SameLine();
        if (ImGuiX.IconButton(FontAwesomeIcon.FileExcel, "Excel Browser"))
            EzConfigGui.GetWindow<ExcelWindow>()!.Toggle();

        if (Service.MacroManager.State == LoopState.NotLoggedIn) { /* Nothing to do */ }
        else if (Service.MacroManager.State == LoopState.Stopped) { /* Nothing to do */ }
        else if (Service.MacroManager.State == LoopState.Waiting) { /* Nothing to do */ }
        else if (Service.MacroManager.State == LoopState.Paused)
        {
            ImGui.SameLine();
            if (ImGuiX.IconButton(FontAwesomeIcon.Play, "Resume"))
                Service.MacroManager.Resume();

            ImGui.SameLine();
            if (ImGuiX.IconButton(FontAwesomeIcon.StepForward, "Step"))
                Service.MacroManager.NextStep();

            ImGui.SameLine();
            if (ImGuiX.IconButton(FontAwesomeIcon.TrashAlt, "Clear"))
                Service.MacroManager.Stop();
        }
        else if (Service.MacroManager.State == LoopState.Running)
        {
            ImGui.SameLine();
            if (ImGuiX.IconButton(FontAwesomeIcon.Pause, "Pause (hold control to pause at next /loop)"))
                Service.MacroManager.Pause(ImGui.GetIO().KeyCtrl);

            ImGui.SameLine();
            if (ImGuiX.IconButton(FontAwesomeIcon.Stop, "Stop (hold control to stop at next /loop)"))
                Service.MacroManager.Stop(ImGui.GetIO().KeyCtrl);
        }
    }

    public static void DrawRunningMacro()
    {
        ImGui.PushItemWidth(-1);
        var style = ImGui.GetStyle();
        var runningHeight = ImGui.CalcTextSize("CalcTextSize").Y * ImGuiHelpers.GlobalScale * 3 + style.FramePadding.Y * 2 + style.ItemSpacing.Y * 2;
        using (var runningMacros = ImRaii.ListBox("##running-macros", new Vector2(-1, runningHeight)))
        {
            if (runningMacros)
            {
                var macroStatus = Service.MacroManager.MacroStatus;
                for (var i = 0; i < macroStatus.Length; i++)
                {
                    var (name, stepIndex) = macroStatus[i];
                    var text = name;
                    if (i == 0 || stepIndex > 1)
                        text += $" (step {stepIndex})";
                    ImGui.Selectable($"{text}##{Guid.NewGuid()}", i == 0);
                }
            }
        }

        var contentHeight = ImGui.CalcTextSize("CalcTextSize").Y * ImGuiHelpers.GlobalScale * 5 + style.FramePadding.Y * 2 + style.ItemSpacing.Y * 4;
        var macroContent = Service.MacroManager.CurrentMacroContent();
        using (var currentMacro = ImRaii.ListBox("##current-macro", new Vector2(-1, contentHeight)))
        {
            if (currentMacro)
            {
                var stepIndex = Service.MacroManager.CurrentMacroStep();
                if (stepIndex == -1)
                    ImGui.Selectable("Looping", true);
                else
                {
                    for (var i = stepIndex; i < macroContent.Length; i++)
                    {
                        var step = macroContent[i];
                        var isCurrentStep = i == stepIndex;
                        ImGui.Selectable(step, isCurrentStep);
                    }
                }
            }
        }

        ImGui.PopItemWidth();
    }
}
