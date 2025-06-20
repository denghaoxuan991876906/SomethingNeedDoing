using ImGuiColorTextEditNet;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.Gui.Editor;

/// <summary>
/// ImGuiColorTextEditNetDalamud TextEditor wrapper.
/// </summary>
public class CodeEditor
{
    private readonly TextEditor _editor = new();
    private readonly Dictionary<MacroType, ISyntaxHighlighter> highlighters = new()
    {
        {MacroType.Lua, new LuaHighlighter()},
        {MacroType.Native, new NativeMacroHighlighter()},
    };

    private IMacro? macro = null;
    private string previousText = "";

    public int Lines => _editor.TotalLines;

    public CodeEditor()
    {
        _editor.Renderer.Palette = EditorPalettes.Highlight;
    }

    public void SetMacro(IMacro macro)
    {
        if (this.macro?.Id == macro.Id)
            return;

        this.macro = macro;
        _editor.AllText = macro.Content;
        previousText = _editor.AllText;

        if (highlighters.TryGetValue(macro.Type, out var highlighter))
            _editor.SyntaxHighlighter = highlighter;
    }

    public void SetHighlightSyntax(bool highlightSyntax)
        => _editor.Renderer.Palette = highlightSyntax ? EditorPalettes.Highlight : EditorPalettes.NoHighlight;

    public string GetContent() => _editor.AllText;

    public bool HasChanged()
    {
        if (previousText != _editor.AllText)
        {
            previousText = _editor.AllText;
            return true;
        }

        return false;
    }

    public bool Draw()
    {
        if (macro == null)
            return false;

        _editor.Render(macro.Name);
        return HasChanged();
    }
}
