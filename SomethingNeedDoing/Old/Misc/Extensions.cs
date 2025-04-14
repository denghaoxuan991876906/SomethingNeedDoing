namespace SomethingNeedDoing.Old.Misc;

/// <summary>
/// Extension methods.
/// </summary>
internal static class Extensions
{
    /// <inheritdoc cref="string.Join(char, string?[])"/>
    public static string Join(this IEnumerable<string> values, char separator)
        => string.Join(separator, values);

    /// <inheritdoc cref="string.Join(string, string?[])"/>
    public static string Join(this IEnumerable<string> values, string separator)
        => string.Join(separator, values);

    public static string GetLast(this string source, int tail_length) => tail_length >= source.Length ? source : source[^tail_length..];

    public static int ToUnixTimestamp(this DateTime value) => (int)Math.Truncate(value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

    public static Language FileExtensionToLanguage(this string extension) => extension switch
    {
        ".txt" => Language.Native,
        ".lua" => Language.Lua,
        //".cs" => Language.CSharp,
        //".py" => Language.Python,
        _ => Language.Native,
    };

    public static string LanguageToFileExtension(this Language language) => language switch
    {
        Language.Native => ".txt",
        Language.Lua => ".lua",
        //Language.CSharp => ".cs",
        //Language.Python => ".py",
        _ => ".txt",
    };
}
