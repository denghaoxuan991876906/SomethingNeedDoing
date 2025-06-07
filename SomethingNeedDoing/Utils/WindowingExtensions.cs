using Dalamud.Interface.Windowing;

namespace SomethingNeedDoing.Utils;
public static class WindowingExtensions
{
    public static Window? GetWindow<T>(this WindowSystem ws) where T : Window => ws.Windows.FirstOrDefault(w => w is T);
    public static void Toggle<T>(this WindowSystem ws) where T : Window => GetWindow<T>(ws)?.IsOpen ^= true;
}
