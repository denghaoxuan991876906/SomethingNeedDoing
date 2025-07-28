using System.IO;
using System.Runtime.CompilerServices;

namespace SomethingNeedDoing.Utils;

/// <summary>
/// A logger that automatically prefixes messages with the calling class name.
/// </summary>
public static class FrameworkLogger
{
    /// <summary>
    /// Logs an informational message with the calling class name as prefix.
    /// </summary>
    /// <param name="message">The message to log.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Info(string message, [CallerFilePath] string path = "")
    {
        Svc.Log.Information($"[{Path.GetFileNameWithoutExtension(path)}] {message}");
    }

    /// <summary>
    /// Logs a debug message with the calling member name as prefix.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="path">The calling member name (automatically provided by the compiler).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Debug(string message, [CallerFilePath] string path = "")
    {
        Svc.Log.Debug($"[{Path.GetFileNameWithoutExtension(path)}] {message}");
    }

    /// <summary>
    /// Logs a verbose message with the calling member name as prefix.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="path">The calling member name (automatically provided by the compiler).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Verbose(string message, [CallerFilePath] string path = "")
    {
        Svc.Log.Verbose($"[{Path.GetFileNameWithoutExtension(path)}] {message}");
    }

    /// <summary>
    /// Logs a warning message with the calling member name as prefix.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="path">The calling member name (automatically provided by the compiler).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Warning(string message, [CallerFilePath] string path = "")
    {
        Svc.Log.Warning($"[{Path.GetFileNameWithoutExtension(path)}] {message}");
    }

    /// <summary>
    /// Logs an error message with the calling member name as prefix.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="path">The calling member name (automatically provided by the compiler).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(string message, [CallerFilePath] string path = "")
    {
        Svc.Log.Error($"[{Path.GetFileNameWithoutExtension(path)}] {message}");
    }

    /// <summary>
    /// Logs an error message with an exception and the calling member name as prefix.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="path">The calling member name (automatically provided by the compiler).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Error(string message, Exception exception, [CallerFilePath] string path = "")
    {
        Svc.Log.Error(exception, $"[{Path.GetFileNameWithoutExtension(path)}] {message}");
    }

    public static void Error(Exception ex, string message, [CallerFilePath] string path = "") => Error(message, ex, path);

    /// <summary>
    /// Logs a fatal error message with the calling member name as prefix.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="path">The calling member name (automatically provided by the compiler).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fatal(string message, [CallerFilePath] string path = "")
    {
        Svc.Log.Fatal($"[{Path.GetFileNameWithoutExtension(path)}] {message}");
    }

    /// <summary>
    /// Logs a fatal error message with an exception and the calling member name as prefix.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="path">The calling member name (automatically provided by the compiler).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fatal(string message, Exception exception, [CallerFilePath] string path = "")
    {
        Svc.Log.Fatal(exception, $"[{Path.GetFileNameWithoutExtension(path)}] {message}");
    }
}
