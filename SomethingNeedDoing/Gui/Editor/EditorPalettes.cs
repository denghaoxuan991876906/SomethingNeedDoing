namespace SomethingNeedDoing.Gui.Editor;

/// <summary>
/// Color palletes for the Code Editor.
/// </summary>
public static class EditorPalettes
{
    // All text is white, only background and highlight is dark
    public static readonly uint[] NoHighlight = [
        0xffffffff, // Default
        0xffffffff, // Keyword
        0xffffffff, // Number
        0xffffffff, // String
        0xffffffff, // CharLiteral
        0xffffffff, // Punctuation
        0xffffffff, // Preprocessor
        0xffffffff, // Identifier
        0xffffffff, // KnownIdentifier
        0xffffffff, // PreprocIdentifier
        0xffffffff, // Comment
        0xffffffff, // MultiLineComment
        0xff212121, // Background
        0xffffffff, // Cursor
        0xffffffff, // Selection
        0xffffffff, // ErrorMarker
        0xffffffff, // Breakpoint
        0xffffffff, // LineNumber
        0xffffffff, // CurrentLineFill
        0x40282828, // CurrentLineFillInactive
        0x403c3f41, // CurrentLineEdge
        0xffffffff, // ExecutingLine
        0xffffffff, // Function
    ];

    public static readonly uint[] Highlight = [
        0xffc5c8c6, // Default
        0xffc792ea, // Keyword
        0xfff78c6c, // Number
        0xffc3e88d, // String
        0xfffcbf7e, // CharLiteral
        0xff89ddff, // Punctuation
        0xff82aaff, // Preprocessor
        0xffd0d0d0, // Identifier
        0xffaddb67, // KnownIdentifier
        0xffffcb6b, // PreprocIdentifier
        0xff616161, // Comment
        0xff616161, // MultiLineComment
        0xff212121, // Background
        0xffffffff, // Cursor
        0x8039adb5, // Selection
        0x80ff5370, // ErrorMarker
        0x40ffcb6b, // Breakpoint
        0xff4a4a4a, // LineNumber
        0x40282828, // CurrentLineFill
        0x403c3f41, // CurrentLineFillInactive
        0x403c3f41, // CurrentLineEdge
        0xa03dd3b0, // ExecutingLine
        0xffc792ea, // Function
    ];
}
