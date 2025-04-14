using Dalamud.Interface.Utility.Raii;

namespace SomethingNeedDoing.Utils;
public static class ImGuiUtils
{
    public static void ContextMenu(string id, params (string menuName, Action action)[] items)
    {
        using var ctx = ImRaii.ContextPopupItem(id);
        if (ctx)
        {
            foreach (var item in items)
                if (ImGui.MenuItem(item.menuName))
                    item.action();
        }
    }
}
