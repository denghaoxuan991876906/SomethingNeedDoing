using Dalamud.Game.ClientState.Keys;
using ECommons.Interop;
using System.Runtime.InteropServices;

namespace SomethingNeedDoing.Old.Misc;

/// <summary>
/// Simulate pressing keyboard input.
/// </summary>
internal static class Keyboard
{
    //private static readonly IntPtr? handle = null;

    /// <summary>
    /// Send a virtual key.
    /// </summary>
    /// <param name="key">Key to send.</param>
    public static void Send(VirtualKey key) => Send(key, null);

    /// <summary>
    /// Send a virtual key with modifiers.
    /// </summary>
    /// <param name="key">Key to send.</param>
    /// <param name="mods">Modifiers to press.</param>
    public static void Send(VirtualKey key, IEnumerable<VirtualKey>? mods)
    {
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;

        if (key != 0)
        {
            //var hWnd = handle ??= Process.GetCurrentProcess().MainWindowHandle;
            if (WindowFunctions.TryFindGameWindow(out var hWnd))
            {
                if (mods != null)
                    foreach (var mod in mods)
                        _ = SendMessage(hWnd, WM_KEYDOWN, (nint)mod, nint.Zero);

                _ = SendMessage(hWnd, WM_KEYDOWN, (nint)key, nint.Zero);
                _ = SendMessage(hWnd, WM_KEYUP, (nint)key, nint.Zero);

                if (mods != null)
                    foreach (var mod in mods)
                        _ = SendMessage(hWnd, WM_KEYUP, (nint)mod, nint.Zero);
            }
        }
    }

    public static void Hold(VirtualKey key) => Hold(key, null);

    public static void Hold(VirtualKey key, IEnumerable<VirtualKey>? mods)
    {
        const int WM_KEYDOWN = 0x100;

        if (key != 0)
        {
            //var hWnd = handle ??= Process.GetCurrentProcess().MainWindowHandle;
            if (WindowFunctions.TryFindGameWindow(out var hWnd))
            {
                if (mods != null)
                    foreach (var mod in mods)
                        _ = SendMessage(hWnd, WM_KEYDOWN, (nint)mod, nint.Zero);

                _ = SendMessage(hWnd, WM_KEYDOWN, (nint)key, nint.Zero);
            }
        }
    }

    public static void Release(VirtualKey key) => Release(key, null);

    public static void Release(VirtualKey key, IEnumerable<VirtualKey>? mods)
    {
        const int WM_KEYUP = 0x101;

        if (key != 0)
        {
            //var hWnd = handle ??= Process.GetCurrentProcess().MainWindowHandle;
            if (WindowFunctions.TryFindGameWindow(out var hWnd))
            {
                _ = SendMessage(hWnd, WM_KEYUP, (nint)key, nint.Zero);

                if (mods != null)
                    foreach (var mod in mods)
                        _ = SendMessage(hWnd, WM_KEYUP, (nint)mod, nint.Zero);
            }
        }
    }

    [DllImport("user32.dll")]
    private static extern int SendMessage(nint hWnd, int wMsg, nint wParam, nint lParam);
}
