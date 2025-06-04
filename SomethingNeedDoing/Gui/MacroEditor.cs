using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using SomethingNeedDoing.Core.Github;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Managers;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Gui;

/// <summary>
/// Macro editor with IDE-like features
/// </summary>
public class MacroEditor(IMacroScheduler scheduler, GitMacroManager gitManager, MacroStatusWindow? statusWindow = null)
{
    private readonly MacroMetadataEditor _metadataEditor = new();
    private bool _showVersionHistory = false;
    private List<GitCommitInfo>? _versionHistory;

    // Editor settings
    private bool _showLineNumbers = true;
    private int _tabSize = 4;
    private bool _highlightSyntax = true;

    // For syntax highlighting (future implementation)
    private static readonly string[] LuaKeywords = [
        "and", "break", "do", "else", "elseif", "end", "false", "for",
        "function", "if", "in", "local", "nil", "not", "or", "repeat",
        "return", "then", "true", "until", "while"
    ];

    private static readonly Regex CommentRegex = new Regex(@"--.*$", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex StringRegex = new Regex(@"""([^""\\]|\\.)*""|'([^'\\]|\\.)*'", RegexOptions.Compiled);
    private static readonly Regex NumberRegex = new Regex(@"\b\d+(\.\d+)?\b", RegexOptions.Compiled);

    public void Draw(IMacro? macro)
    {
        using var child = ImRaii.Child("RightPanel", new Vector2(0, -1), false);
        if (!child) return;
        if (macro == null)
        {
            var center = ImGui.GetContentRegionAvail() / 2;
            var text = "Select a macro or create a new one";
            var textSize = ImGui.CalcTextSize(text);

            ImGui.SetCursorPos(ImGui.GetCursorPos() + center - textSize / 2);
            ImGui.TextColored(ImGuiColors.DalamudGrey, text);
            return;
        }

        DrawMacroHeader(macro);
        DrawEditorToolbar(macro);
        ImGui.Separator();

        // Calculate space for editor
        var reserveHeight = ImGui.GetFrameHeightWithSpacing() * 2;
        var editorHeight = ImGui.GetContentRegionAvail().Y - reserveHeight;

        DrawCodeEditorWithLineNumbers(macro, editorHeight);
        DrawStatusBar(macro);
    }

    private void DrawMacroHeader(IMacro macro)
    {
        ImGui.BeginGroup();
        DisplayMacroStatus(macro);
        ImGui.EndGroup();
    }

    private void DisplayMacroStatus(IMacro macro)
    {
        var macroState = scheduler.GetMacroState(macro.Id);
        if (macroState == MacroState.Unknown)
            return;

        // Status color based on state
        var statusColor = macroState switch
        {
            MacroState.Running => ImGuiColors.HealerGreen,
            MacroState.Paused => ImGuiColors.DalamudOrange,
            MacroState.Error => ImGuiColors.DalamudRed,
            MacroState.Completed => ImGuiColors.ParsedBlue,
            _ => ImGuiColors.DalamudGrey
        };

        // Status icon based on state
        var statusIcon = macroState switch
        {
            MacroState.Running => FontAwesomeIcon.Play,
            MacroState.Paused => FontAwesomeIcon.Pause,
            MacroState.Error => FontAwesomeIcon.ExclamationTriangle,
            MacroState.Completed => FontAwesomeIcon.CheckCircle,
            _ => FontAwesomeIcon.Circle
        };

        // Position on right side
        var contentWidth = ImGui.GetContentRegionAvail().X;
        ImGui.SameLine(contentWidth - 300);

        // Icon and status text
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextColored(statusColor, statusIcon.ToIconString());
        ImGui.PopFont();

        ImGui.SameLine(0, 5);
        ImGui.TextColored(statusColor, macroState.ToString());

        // Control buttons if running or paused
        if (macroState is MacroState.Running or MacroState.Paused)
        {
            ImGui.SameLine(0, 10);

            if (ImGuiUtils.IconButton(macroState == MacroState.Paused ? FontAwesomeIcon.Play : FontAwesomeIcon.Pause,
                           macroState == MacroState.Paused ? "Resume" : "Pause"))
            {
                if (macroState == MacroState.Paused)
                    scheduler.ResumeMacro(macro.Id);
                else
                    scheduler.PauseMacro(macro.Id);
            }

            ImGui.SameLine(0, 5);

            if (ImGuiUtils.IconButton(FontAwesomeIcon.Stop, "Stop"))
                scheduler.StopMacro(macro.Id);
        }
    }

    private void DrawEditorToolbar(IMacro macro)
    {
        float buttonWidth = 80;

        using var toolbarChild = ImRaii.Child("ToolbarChild", new Vector2(-1, ImGui.GetFrameHeight() * 1.5f), false);

        // Run button (green)
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.7f, 0.2f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.8f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.4f, 0.9f, 0.4f, 1.0f));

        if (ImGuiX.IconTextButton(FontAwesomeIcon.PlayCircle, "Run", new Vector2(buttonWidth, 0)))
        {
            scheduler.StartMacro(macro);
        }

        ImGui.PopStyleColor(3);

        ImGui.SameLine();

        // Stop button (red)
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.7f, 0.2f, 0.2f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.8f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.9f, 0.4f, 0.4f, 1.0f));

        if (ImGuiX.IconTextButton(FontAwesomeIcon.StopCircle, "Stop", new Vector2(buttonWidth, 0)))
        {
            scheduler.StopMacro(macro.Id);
        }

        ImGui.PopStyleColor(3);

        ImGui.SameLine();

        // Copy button (gray)
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

        if (ImGuiX.IconTextButton(FontAwesomeIcon.Clipboard, "Copy", new Vector2(buttonWidth, 0)))
        {
            ImGui.SetClipboardText(macro.Content);
        }

        ImGui.PopStyleColor(3);

        // Right-aligned controls
        var windowWidth = ImGui.GetWindowWidth();
        ImGui.SameLine(windowWidth - 120);

        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);

        // Status indicator with running macro count
        var runningMacros = scheduler.GetMacros().ToList();
        var macroCount = runningMacros.Count();

        var statusColor = macroCount > 0 ? ImGuiColors.HealerGreen : ImGuiColors.DalamudGrey;
        var statusIcon = macroCount > 0 ? FontAwesomeIcon.Play : FontAwesomeIcon.Desktop;

        ImGui.PushStyleColor(ImGuiCol.Text, statusColor);
        if (ImGuiUtils.IconButton(statusIcon, macroCount > 0 ? $"{macroCount} running" : "No macros running"))
        {
            if (statusWindow != null)
            {
                statusWindow.IsOpen = !statusWindow.IsOpen;
                if (statusWindow.IsOpen)
                    statusWindow.BringToFront();
            }
        }
        ImGui.PopStyleColor();

        ImGui.SameLine();

        // Line numbers toggle
        if (ImGuiUtils.IconButton(
            _showLineNumbers ? FontAwesomeHelper.IconSortAsc : FontAwesomeHelper.IconSortDesc,
            "Toggle Line Numbers"))
        {
            _showLineNumbers = !_showLineNumbers;
        }

        ImGui.SameLine();

        // Syntax highlighting toggle
        if (ImGuiUtils.IconButton(
            _highlightSyntax ? FontAwesomeHelper.IconCheck : FontAwesomeHelper.IconXmark,
            "Syntax Highlighting (not currently available)"))
        {
            _highlightSyntax = !_highlightSyntax;
        }

        ImGui.PopStyleColor();
    }

    private void DrawCodeEditorWithLineNumbers(IMacro macro, float height)
    {
        ImGui.BeginGroup();

        // Size line numbers panel based on content
        float lineNumberWidth = 40;
        var lines = macro.Content.Split('\n').Length;
        if (lines > 99) lineNumberWidth = 50;
        if (lines > 999) lineNumberWidth = 60;

        using var monoFont = ImRaii.PushFont(UiBuilder.MonoFont, true);
        var lineHeight = ImGui.GetTextLineHeight();
        var editorPadding = 5.0f;

        // Draw line numbers if enabled
        if (_showLineNumbers)
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, editorPadding));

            ImGui.BeginChild("LineNumbers", new Vector2(lineNumberWidth, height), true,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            var startY = ImGui.GetCursorPosY();

            for (var i = 0; i < lines; i++)
            {
                ImGui.SetCursorPosY(startY + (i * lineHeight));
                var textWidth = ImGui.CalcTextSize($"{i + 1}").X;
                ImGui.SetCursorPosX(lineNumberWidth - textWidth - 6);
                ImGui.Text($"{i + 1}");
            }

            ImGui.EndChild();

            ImGui.PopStyleVar(2);
            ImGui.PopStyleColor(2);
            ImGui.SameLine(0, 0);
        }

        // Text editor area
        var editorWidth = ImGui.GetContentRegionAvail().X;

        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.15f, 0.15f, 0.15f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, editorPadding));

        var flags = ImGuiInputTextFlags.AllowTabInput;

        if (macro is ConfigMacro configMacro)
        {
            var contents = configMacro.Content;

            if (ImGui.InputTextMultiline(
                "##MacroEditor",
                ref contents,
                1_000_000,
                new Vector2(editorWidth, height),
                flags))
            {
                configMacro.Content = contents;
                C.Save();
            }
        }
        else if (macro is ConfigMacro m)
        {
            var contents = m.Content;

            ImGui.InputTextMultiline(
                "##MacroEditor",
                ref contents,
                1_000_000,
                new Vector2(editorWidth, height),
                flags | ImGuiInputTextFlags.ReadOnly);
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor();

        ImGui.EndGroup();
    }

    private void DrawStatusBar(IMacro macro)
    {
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);

        var lines = macro.Content.Split('\n').Length;
        var chars = macro.Content.Length;

        ImGui.Text($"Name: {macro.Name}  |  Lines: {lines}  |  Chars: {chars}  |  Type: {macro.Type}");

        ImGui.PopStyleColor(2);
    }
}
