using ImGuiColorTextEditNet;

namespace SomethingNeedDoing.Gui.Editor;

/// <summary>
/// Highlighter implementation for native macros.
///  - Highlights slash commands, string literal args and modifiers
/// </summary>
public class NativeMacroHighlighter : ISyntaxHighlighter
{
    static readonly object DefaultState = new();

    public bool AutoIndentation => false;
    public int MaxLinesPerFrame => 1000;
    public string? GetTooltip(string id) => null;

    public object Colorize(Span<Glyph> line, object? state)
    {
        var i = 0;
        while (i < line.Length)
        {
            var result = Tokenize(line[i..]);
            Util.Assert(result != 0);
            i += result > 0 ? result : 1;
        }

        return state ?? DefaultState;
    }

    int Tokenize(Span<Glyph> span)
    {
        if (span.Length == 0)
            return -1;

        int result;
        if ((result = TokenizeCommand(span)) != -1) return result;
        if ((result = TokenizeArguments(span)) != -1) return result;
        if ((result = TokenizeModifier(span)) != -1) return result;

        return -1;
    }

    static int TokenizeCommand(Span<Glyph> span)
    {
        if (span[0].Char != '/')
            return -1;

        var i = 1;
        while (i < span.Length && char.IsLetter(span[i].Char))
        {
            span[i] = new Glyph(span[i].Char, PaletteIndex.Keyword);
            i++;
        }

        span[0] = new Glyph(span[0].Char, PaletteIndex.Keyword);
        return i;
    }

    static int TokenizeArguments(Span<Glyph> span)
    {
        if (span[0].Char != '"')
            return -1;

        var i = 1;
        while (i < span.Length)
        {
            if (span[i].Char == '"')
            {
                for (var j = 0; j <= i; j++)
                    span[j] = new Glyph(span[j].Char, PaletteIndex.String);
                return i + 1;
            }
            i++;
        }

        return -1;
    }

    static int TokenizeModifier(Span<Glyph> span)
    {
        if (span.Length < 3 || span[0].Char != '<')
            return -1;

        var i = 1;
        while (i < span.Length && span[i].Char != '>')
            i++;

        if (i >= span.Length)
            return -1;

        for (var j = 0; j <= i; j++)
        {
            var c = span[j].Char;
            var index = c switch
            {
                '<' or '>' or '.' => PaletteIndex.Punctuation,
                >= '0' and <= '9' => PaletteIndex.KnownIdentifier,
                _ => PaletteIndex.KnownIdentifier
            };
            span[j] = new Glyph(c, index);
        }

        return i + 1;
    }
}
