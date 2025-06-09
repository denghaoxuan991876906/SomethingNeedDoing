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
public class MacroEditor(IMacroScheduler scheduler, GitMacroManager gitManager, MacroStatusWindow? statusWindow = null)
{
    private readonly IMacroScheduler _scheduler = scheduler;
    private readonly GitMacroManager _gitManager = gitManager;
    private readonly MacroStatusWindow? _statusWindow = statusWindow;
    private bool _showLineNumbers = true;
    private bool _highlightSyntax = true;
    private bool _isCheckingForUpdates;

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
        using var toolbar = ImRaii.Child("ToolbarChild", new Vector2(-1, ImGui.GetFrameHeight() * 1.5f), false);
        if (!toolbar) return;

        DrawActionButtons(macro);
        DrawRightAlignedControls();
    }

    private void DrawActionButtons(IMacro macro)
    {
        var group = new ImGuiEx.EzButtonGroup();
        var baseStyle = new ImGuiEx.EzButtonGroup.ButtonStyle() { NoButtonBg = true, TextColor = ImGuiUtils.Colours.Gold };
        group.AddIconOnly(FontAwesomeIcon.PlayCircle, () => _scheduler.StartMacro(macro), "Run", baseStyle);
        group.AddIconOnly(FontAwesomeIcon.PauseCircle, () => _scheduler.PauseMacro(macro.Id), "Pause", baseStyle + new ImGuiEx.EzButtonGroup.ButtonStyle() { NoButtonBg = true, Condition = () => _scheduler.GetMacroState(macro.Id) == MacroState.Running });
        group.AddIconOnly(FontAwesomeIcon.StopCircle, () => _scheduler.StopMacro(macro.Id), "Stop", baseStyle);
        group.AddIconOnly(FontAwesomeIcon.Clipboard, () => ImGui.SetClipboardText(macro.Content), "Copy", baseStyle);
        group.Draw();
        ImGui.SameLine();
        if (macro is ConfigMacro { IsGitMacro: true } configMacro)
        {
            if (_isCheckingForUpdates)
            {
                using (ImRaii.Disabled())
                    ImGuiUtils.Button(new Vector4(0.2f, 0.4f, 0.8f, 1.0f), FontAwesomeIcon.Sync, "Checking...");
            }
            else if (ImGuiUtils.Button(new Vector4(0.2f, 0.4f, 0.8f, 1.0f), FontAwesomeIcon.Sync, "Check for updates"))
            {
                _isCheckingForUpdates = true;
                Task.Run(async () =>
                {
                    try
                    {
                        await _gitManager.CheckForUpdates(configMacro);
                    }
                    finally
                    {
                        _isCheckingForUpdates = false;
                    }
                });
            }
        }
    }

    private void DrawRightAlignedControls()
    {
        var windowWidth = ImGui.GetWindowWidth();
        ImGui.SameLine(windowWidth - 120);

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
                if (_statusWindow != null)
                {
                    _statusWindow.IsOpen = !_statusWindow.IsOpen;
                    if (_statusWindow.IsOpen)
                        _statusWindow.BringToFront();
                }
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
