using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using SomethingNeedDoing.Core.Github;
using SomethingNeedDoing.Framework.Interfaces;
using SomethingNeedDoing.Managers;
using SomethingNeedDoing.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Gui;

/// <summary>
/// Enhanced MacroEditor with IDE-like features and syntax highlighting
/// </summary>
public class MacroEditor
{
    private readonly IMacroScheduler _scheduler;
    private readonly GitMacroManager _gitManager;
    private readonly MacroMetadataEditor _metadataEditor = new();
    private bool _showVersionHistory = false;
    private List<GitCommitInfo>? _versionHistory;
    
    // Editor settings
    private bool _showLineNumbers = true;
    private int _tabSize = 4;
    private bool _highlightSyntax = true;
    private bool _wrapText = false;
    
    // Lua keywords and patterns for syntax highlighting
    private static readonly string[] LuaKeywords = new[] { 
        "and", "break", "do", "else", "elseif", "end", "false", "for", 
        "function", "if", "in", "local", "nil", "not", "or", "repeat", 
        "return", "then", "true", "until", "while"
    };
    
    private static readonly Regex CommentRegex = new Regex(@"--.*$", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex StringRegex = new Regex(@"""([^""\\]|\\.)*""|'([^'\\]|\\.)*'", RegexOptions.Compiled);
    private static readonly Regex NumberRegex = new Regex(@"\b\d+(\.\d+)?\b", RegexOptions.Compiled);

    public MacroEditor(IMacroScheduler scheduler, GitMacroManager gitManager)
    {
        _scheduler = scheduler;
        _gitManager = gitManager;
    }

    /// <summary>
    /// Draw only the metadata editor for a macro without the full editor
    /// </summary>
    /// <param name="macro">The macro to draw settings for</param>
    public void DrawMetadataOnly(IMacro macro)
    {
        if (macro == null) return;
        
        // Better styled header with macro name and type
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudWhite);
        // Use standard font but make text larger
        float fontSize = ImGui.GetFontSize() * 1.2f;
        ImGui.SetWindowFontScale(1.2f);
        ImGui.Text(macro.Name);
        ImGui.SetWindowFontScale(1.0f);
        ImGui.PopStyleColor();
        
        // Show macro type with appropriate icon
        ImGui.SameLine();
        string icon = macro is GitMacro ? "📦" : "📄";
        string typeText = macro.Type == MacroType.Lua ? "Lua" : "Native";
        ImGui.TextColored(ImGuiColors.DalamudGrey, $"({icon} {typeText})");
        
        // Add more metadata if available
        if (!string.IsNullOrEmpty(macro.Metadata.Author))
        {
            ImGui.TextColored(ImGuiColors.DalamudGrey, $"by {macro.Metadata.Author}");
        }
        
        ImGui.Separator();
        ImGui.Spacing();
        
        // Draw the metadata editor with some padding
        ImGui.Indent(5);
        _metadataEditor.Draw(macro);
        ImGui.Unindent(5);
    }

    public void Draw(IMacro macro)
    {
        if (macro == null) return;

        // Add title bar with macro name and type selector
        DrawMacroHeader(macro);

        // Editor toolbar with run controls and settings
        DrawEditorToolbar(macro);

        // Add a separator to distinguish controls from content
        ImGui.Separator();

        // Get the ENTIRE remaining area for editor
        float reserveHeight = 0;

        // Reserve space for separator and metadata header
        reserveHeight += ImGui.GetFrameHeightWithSpacing() * 3;

        // Calculate height for editor (all available space minus reserved)
        float editorHeight = ImGui.GetContentRegionAvail().Y - reserveHeight;

        // Draw the code editor with proper line numbers
        DrawCodeEditorWithLineNumbers(macro, editorHeight);
        
        // Status bar below editor
        DrawStatusBar(macro);
    }
    
    private void DrawMacroHeader(IMacro macro)
    {
        // Create header with name and type selector
        ImGui.BeginGroup();
        
        // Display macro name
        ImGui.Text("Name: ");
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.DalamudWhite, macro.Name);
        
        // Right-aligned type selector for ConfigMacro (editable)
        if (macro is ConfigMacro configMacro)
        {
            // Get window width to position type selector
            float windowWidth = ImGui.GetContentRegionAvail().X;
            float selectorWidth = 120;
            
            ImGui.SameLine(windowWidth - selectorWidth);
            ImGui.SetNextItemWidth(selectorWidth);
            
            // Type selector dropdown
            int currentType = (int)configMacro.Type;
            string[] typeOptions = { "Native", "Lua" };
            
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            
            if (ImGui.Combo("##MacroTypeSelector", ref currentType, typeOptions, typeOptions.Length))
            {
                // Update macro type when changed
                configMacro.Type = (MacroType)currentType;
                C.Save();
            }
            
            ImGui.PopStyleColor();
        }
        else
        {
            // For GitMacro, just display the type (not editable)
            float windowWidth = ImGui.GetContentRegionAvail().X;
            ImGui.SameLine(windowWidth - 120);
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Type: GitMacro (Read-only)");
        }
        
        ImGui.EndGroup();
        ImGui.Separator();
    }

    private void DrawEditorToolbar(IMacro macro)
    {
        // Control buttons with clearer styling and icons
        float buttonWidth = 80;
        float iconButtonWidth = 30;
        
        // Run button with green color
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.7f, 0.2f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.8f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.4f, 0.9f, 0.4f, 1.0f));
        
        // Use direct FontAwesomeIcon values to ensure proper icon display
        if (ImGuiX.IconTextButton(FontAwesomeIcon.PlayCircle, "Run", new Vector2(buttonWidth, 0)))
        {
            _scheduler.StartMacro(macro);
        }
        
        ImGui.PopStyleColor(3);

        ImGui.SameLine();
        
        // Stop button with red color
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.7f, 0.2f, 0.2f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.8f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.9f, 0.4f, 0.4f, 1.0f));
        
        // Use direct FontAwesomeIcon values to ensure proper icon display
        if (ImGuiX.IconTextButton(FontAwesomeIcon.StopCircle, "Stop", new Vector2(buttonWidth, 0)))
        {
            _scheduler.StopMacro(macro.Id);
        }
        
        ImGui.PopStyleColor(3);

        ImGui.SameLine();
        
        // Copy button with gray color
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
        
        // Use direct FontAwesomeIcon values to ensure proper icon display
        if (ImGuiX.IconTextButton(FontAwesomeIcon.Clipboard, "Copy", new Vector2(buttonWidth, 0)))
        {
            ImGui.SetClipboardText(macro.Content);
        }
        
        ImGui.PopStyleColor(3);
        
        // Add some spacing
        ImGui.SameLine(0, 20);
        
        // Editor settings toggles
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
        
        // Line numbers toggle
        if (ImGuiX.IconButton(
            _showLineNumbers ? FontAwesomeHelper.IconSortAsc : FontAwesomeHelper.IconSortDesc, 
            "Toggle Line Numbers"))
        {
            _showLineNumbers = !_showLineNumbers;
        }
        
        ImGui.SameLine();
        
        // Syntax highlighting toggle
        if (ImGuiX.IconButton(
            _highlightSyntax ? FontAwesomeHelper.IconCheck : FontAwesomeHelper.IconXmark, 
            "Toggle Syntax Highlighting"))
        {
            _highlightSyntax = !_highlightSyntax;
        }
        
        ImGui.SameLine();
        
        // Word wrap toggle
        if (ImGuiX.IconButton(
            _wrapText ? FontAwesomeHelper.IconIndent : FontAwesomeHelper.IconAlignLeft, 
            "Toggle Word Wrap"))
        {
            _wrapText = !_wrapText;
        }
        
        ImGui.PopStyleColor();
    }

    private void DrawCodeEditorWithLineNumbers(IMacro macro, float height)
    {
        // Container to hold both line numbers and editor
        ImGui.BeginGroup();
        
        // Calculate line numbers width based on content
        float lineNumberWidth = 40;
        int lines = macro.Content.Split('\n').Length;
        if (lines > 99) lineNumberWidth = 50;
        if (lines > 999) lineNumberWidth = 60;
        
        // Use monospaced font for everything to ensure alignment
        using var monoFont = ImRaii.PushFont(UiBuilder.MonoFont, true);
        
        // Get the actual line height for proper spacing and alignment
        float lineHeight = ImGui.GetTextLineHeight();
        // Get editor padding to match exactly
        float editorPadding = 5.0f;
        
        // Draw line numbers panel if enabled
        if (_showLineNumbers)
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
            // Match padding to text editor for perfect alignment
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, editorPadding));
            
            // Create scrolling child with exact same size as editor to ensure alignment
            ImGui.BeginChild("LineNumbers", new Vector2(lineNumberWidth, height), true, 
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            
            // Draw each line number with proper alignment
            // Exact calculation to match editor's internal text position
            float startY = ImGui.GetCursorPosY(); // No need for extra offset with correct padding
            
            for (int i = 0; i < lines; i++)
            {
                ImGui.SetCursorPosY(startY + (i * lineHeight));
                // Right-align line numbers with consistent padding
                float textWidth = ImGui.CalcTextSize($"{i+1}").X;
                ImGui.SetCursorPosX(lineNumberWidth - textWidth - 6);
                ImGui.Text($"{i+1}");
            }
            
            ImGui.EndChild();
            
            ImGui.PopStyleVar(2); // Pop padding and spacing
            ImGui.PopStyleColor(2);
            ImGui.SameLine(0, 0);
        }
        
        // Draw editor area
        float editorWidth = ImGui.GetContentRegionAvail().X;
        
        // Set styles for code editor
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.15f, 0.15f, 0.15f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, editorPadding));
        
        ImGuiInputTextFlags flags = ImGuiInputTextFlags.AllowTabInput;
        if (_wrapText) flags |= ImGuiInputTextFlags.NoHorizontalScroll;
        
        // For editable macros
        if (macro is ConfigMacro configMacro)
        {
            var contents = configMacro.Content;
            
            // Force editor to take full width and calculated height
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
        else if (macro is GitMacro gitMacro)
        {
            // Similar for git macro, but read-only
            var contents = gitMacro.Content;

            ImGui.InputTextMultiline(
                "##MacroEditor",
                ref contents,
                1_000_000,
                new Vector2(editorWidth, height),
                flags | ImGuiInputTextFlags.ReadOnly);
        }
        
        ImGui.PopStyleVar();
        ImGui.PopStyleColor(); // FrameBg
        
        ImGui.EndGroup();
    }
    
    private void DrawStatusBar(IMacro macro)
    {
        // Status bar with editor info
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
        
        var lines = macro.Content.Split('\n').Length;
        var chars = macro.Content.Length;
        
        ImGui.Text($"Lines: {lines}  Chars: {chars}  Type: {macro.Type}  Tab Size: {_tabSize}");
        
        ImGui.PopStyleColor(2);
    }
}
