using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Managers;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Gui;

/// <summary>
/// Macro editor with IDE-like features
/// </summary>
public class MacroEditor(IMacroScheduler scheduler, GitMacroManager gitManager, MacroStatusWindow statusWindow)
{
    private readonly IMacroScheduler _scheduler = scheduler;
    private readonly GitMacroManager _gitManager = gitManager;
    private readonly MacroStatusWindow _statusWindow = statusWindow;
    private bool _showLineNumbers = true;
    private bool _highlightSyntax = true;
    private UpdateState _updateState = UpdateState.Unknown;

    private enum UpdateState
    {
        Unknown,
        None,
        Available
    }

    public void Draw(IMacro? macro)
    {
        using var child = ImRaii.Child("RightPanel", new Vector2(0, -1), false);
        if (!child) return;

        if (macro == null)
        {
            DrawEmptyState();
            return;
        }

        DrawMacroHeader(macro);
        DrawEditorToolbar(macro);
        ImGui.Separator();

        var editorHeight = ImGui.GetContentRegionAvail().Y - ImGui.GetFrameHeightWithSpacing() * 2;
        DrawCodeEditor(macro, editorHeight);
        DrawStatusBar(macro);
    }

    private void DrawEmptyState()
    {
        var center = ImGui.GetContentRegionAvail() / 2;
        var text = "Select a macro or create a new one";
        var textSize = ImGui.CalcTextSize(text);
        ImGui.SetCursorPos(ImGui.GetCursorPos() + center - textSize / 2);
        ImGui.TextColored(ImGuiColors.DalamudGrey, text);
    }

    private void DrawMacroHeader(IMacro macro)
    {
        using var group = ImRaii.Group();
        var macroState = _scheduler.GetMacroState(macro.Id);
        if (macroState == MacroState.Unknown) return;

        var (statusColor, statusIcon) = GetStatusInfo(macroState);
        var contentWidth = ImGui.GetContentRegionAvail().X;
        ImGui.SameLine(contentWidth - 300);

        using (ImRaii.PushFont(UiBuilder.IconFont))
            ImGui.TextColored(statusColor, statusIcon.ToIconString());

        ImGui.SameLine(0, 5);
        ImGui.TextColored(statusColor, macroState.ToString());

        if (macroState is MacroState.Running or MacroState.Paused)
            DrawControlButtons(macro, macroState);
    }

    private (Vector4 color, FontAwesomeIcon icon) GetStatusInfo(MacroState state) => state switch
    {
        MacroState.Running => (ImGuiColors.HealerGreen, FontAwesomeIcon.Play),
        MacroState.Paused => (ImGuiColors.DalamudOrange, FontAwesomeIcon.Pause),
        MacroState.Error => (ImGuiColors.DalamudRed, FontAwesomeIcon.ExclamationTriangle),
        MacroState.Completed => (ImGuiColors.ParsedBlue, FontAwesomeIcon.CheckCircle),
        _ => (ImGuiColors.DalamudGrey, FontAwesomeIcon.Circle)
    };

    private void DrawControlButtons(IMacro macro, MacroState state)
    {
        ImGui.SameLine(0, 10);
        if (ImGuiUtils.IconButton(state == MacroState.Paused ? FontAwesomeIcon.Play : FontAwesomeIcon.Pause,
            state == MacroState.Paused ? "Resume" : "Pause"))
        {
            if (state == MacroState.Paused)
                _scheduler.ResumeMacro(macro.Id);
            else
                _scheduler.PauseMacro(macro.Id);
        }

        ImGui.SameLine(0, 5);
        if (ImGuiUtils.IconButton(FontAwesomeIcon.Stop, "Stop"))
            _scheduler.StopMacro(macro.Id);
    }

    private void DrawEditorToolbar(IMacro macro)
    {
        using var toolbar = ImRaii.Child("ToolbarChild", new Vector2(-1, ImGui.GetFrameHeight() * 2f), false);
        if (!toolbar) return;

        ImGui.Spacing();
        ImGui.Spacing();
        DrawActionButtons(macro);
        DrawRightAlignedControls(macro);
    }

    private void DrawActionButtons(IMacro macro)
    {
        var group = new ImGuiEx.EzButtonGroup();
        var baseStyle = new ImGuiEx.EzButtonGroup.ButtonStyle() { TextColor = ImGuiColors.DalamudGrey };
        group.AddIconOnly(FontAwesomeIcon.PlayCircle, () => _scheduler.StartMacro(macro), "Run", baseStyle);
        group.AddIconOnly(FontAwesomeIcon.PauseCircle, () => _scheduler.PauseMacro(macro.Id), "Pause",
            baseStyle + new ImGuiEx.EzButtonGroup.ButtonStyle() { NoButtonBg = true, Condition = () => _scheduler.GetMacroState(macro.Id) == MacroState.Running });
        group.AddIconOnly(FontAwesomeIcon.StopCircle, () => _scheduler.StopMacro(macro.Id), "Stop", baseStyle);
        group.AddIconOnly(FontAwesomeIcon.Clipboard, () => Copy(macro.Content), "Copy", baseStyle);
        group.Draw();
    }

    private void DrawRightAlignedControls(IMacro macro)
    {
        ImGui.SameLine(ImGui.GetWindowWidth() - (macro is ConfigMacro { IsGitMacro: true } ? 145 : 120));

        using var _ = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);

        var runningMacros = _scheduler.GetMacros().ToList();
        var macroCount = runningMacros.Count;
        var (statusColor, statusIcon) = macroCount > 0
            ? (ImGuiColors.HealerGreen, FontAwesomeIcon.Play)
            : (ImGuiColors.DalamudGrey, FontAwesomeIcon.Desktop);

        using (ImRaii.PushColor(ImGuiCol.Text, statusColor))
        {
            if (ImGuiUtils.IconButton(statusIcon, macroCount > 0 ? $"{macroCount} running" : "No macros running"))
            {
                _statusWindow.IsOpen = !_statusWindow.IsOpen;
                if (_statusWindow.IsOpen)
                    _statusWindow.BringToFront();
            }
        }

        ImGui.SameLine();
        if (ImGuiUtils.IconButton(
            _showLineNumbers ? FontAwesomeHelper.IconSortAsc : FontAwesomeHelper.IconSortDesc,
            "Toggle Line Numbers"))
            _showLineNumbers = !_showLineNumbers;

        ImGui.SameLine();
        if (ImGuiUtils.IconButton(
            _highlightSyntax ? FontAwesomeHelper.IconCheck : FontAwesomeHelper.IconXmark,
            "Syntax Highlighting (not currently available)"))
            _highlightSyntax = !_highlightSyntax;

        if (macro is ConfigMacro { IsGitMacro: true } configMacro)
        {
            ImGui.SameLine();
            var (updateIndicator, updateColor, tooltip) = _updateState switch
            {
                UpdateState.None => ("0", ImGuiColors.DalamudGrey, "No updates available"),
                UpdateState.Available => ("1", ImGuiColors.DPSRed, "Update available (click to update)"),
                _ => ("?", ImGuiColors.DalamudGrey, "Check for updates")
            };

            if (ImGuiUtils.IconButtonWithNotification(FontAwesomeIcon.Bell, updateIndicator, updateColor, tooltip))
            {
                if (_updateState == UpdateState.Available)
                    Task.Run(async () => await _gitManager.UpdateMacro(configMacro));
                else
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _gitManager.CheckForUpdates(configMacro);
                            _updateState = configMacro.GitInfo.HasUpdate ? UpdateState.Available : UpdateState.None;
                        }
                        catch
                        {
                            _updateState = UpdateState.Unknown;
                        }
                    });
            }
        }
    }

    private void DrawCodeEditor(IMacro macro, float height)
    {
        using var group = ImRaii.Group();
        var lineNumberWidth = CalculateLineNumberWidth(macro.Content);
        var lineHeight = ImGui.GetTextLineHeight();
        var editorPadding = 5.0f;

        if (_showLineNumbers)
            DrawLineNumbers(macro.Content, lineNumberWidth, height, lineHeight, editorPadding);

        ImGui.SameLine(0, 0);
        DrawTextEditor(macro, height, editorPadding);
    }

    private float CalculateLineNumberWidth(string content) => content.Split('\n').Length switch
    {
        > 999 => 60,
        > 99 => 50,
        _ => 40
    };

    private void DrawLineNumbers(string content, float width, float height, float lineHeight, float padding)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.ChildBg, new Vector4(0.1f, 0.1f, 0.1f, 1.0f)).Push(ImGuiCol.Text, ImGuiColors.DalamudGrey);
        using var __ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0)).Push(ImGuiStyleVar.WindowPadding, new Vector2(0, padding));

        using var child = ImRaii.Child("LineNumbers", new Vector2(width, height), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        if (child)
        {
            var startY = ImGui.GetCursorPosY();
            var lines = content.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                ImGui.SetCursorPosY(startY + (i * lineHeight));
                var textWidth = ImGui.CalcTextSize($"{i + 1}").X;
                ImGui.SetCursorPosX(width - textWidth - 6);
                ImGui.Text($"{i + 1}");
            }
        }
    }

    private void DrawTextEditor(IMacro macro, float height, float padding)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.FrameBg, new Vector4(0.15f, 0.15f, 0.15f, 1.0f));
        using var __ = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, new Vector2(5, padding));

        var flags = ImGuiInputTextFlags.AllowTabInput;
        var editorWidth = ImGui.GetContentRegionAvail().X;

        if (macro is ConfigMacro configMacro)
        {
            var contents = configMacro.Content;
            if (ImGui.InputTextMultiline("##MacroEditor", ref contents, 1_000_000, new Vector2(editorWidth, height), flags))
            {
                configMacro.Content = contents;
                C.Save();
            }
        }
        else if (macro is ConfigMacro m)
        {
            var contents = m.Content;
            ImGui.InputTextMultiline("##MacroEditor", ref contents, 1_000_000, new Vector2(editorWidth, height), flags | ImGuiInputTextFlags.ReadOnly);
        }
    }

    private void DrawStatusBar(IMacro macro)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.FrameBg, new Vector4(0.1f, 0.1f, 0.1f, 1.0f));

        var lines = macro.Content.Split('\n').Length;
        var chars = macro.Content.Length;
        ImGuiEx.Text(ImGuiColors.DalamudGrey, $"Name: {macro.Name}  |  Lines: {lines}  |  Chars: {chars}  |  Type: {macro.Type}");

        if (macro is ConfigMacro { IsGitMacro: true } configMacro)
        {
            ImGui.SameLine(0, 0);
            ImGuiEx.Text(ImGuiColors.DalamudGrey, " | ");
            ImGui.SameLine(0, 0);
            ImGuiUtils.DrawLink(ImGuiColors.DalamudGrey, $"Git: {configMacro.GitInfo}", configMacro.GitInfo.RepositoryUrl);
        }
    }
}
