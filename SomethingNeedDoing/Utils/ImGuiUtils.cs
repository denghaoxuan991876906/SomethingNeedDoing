using Dalamud.Interface;
using Dalamud.Interface.Utility;
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

    public static bool IconButton(FontAwesomeIcon icon, string tooltip = "", string id = "SNDButton", Vector2 size = default, bool disabled = false, bool active = false)
    {
        using var iconFont = ImRaii.PushFont(UiBuilder.IconFont);
        if (!id.StartsWith("##")) id = "##" + id;

        var disposables = new List<IDisposable>();

        if (disabled)
        {
            disposables.Add(ImRaii.PushColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]));
            disposables.Add(ImRaii.PushColor(ImGuiCol.ButtonActive, ImGui.GetStyle().Colors[(int)ImGuiCol.Button]));
            disposables.Add(ImRaii.PushColor(ImGuiCol.ButtonHovered, ImGui.GetStyle().Colors[(int)ImGuiCol.Button]));
        }
        else if (active)
        {
            disposables.Add(ImRaii.PushColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonActive]));
        }

        var pressed = ImGui.Button(icon.ToIconString() + id, size);

        foreach (var disposable in disposables)
            disposable.Dispose();

        iconFont?.Dispose();

        if (tooltip != string.Empty && ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);

        return pressed;
    }

    public static T EnumRadioButtons<T>(T currentValue, string label = "", float itemWidth = 0) where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>();
        var names = Enum.GetNames(typeof(T));
        var result = currentValue;

        if (!string.IsNullOrEmpty(label))
        {
            ImGui.TextUnformatted(label);
            ImGui.SameLine();
        }

        for (var i = 0; i < values.Count(); i++)
        {
            if (i > 0)
                ImGui.SameLine();

            if (itemWidth > 0)
                ImGui.SetNextItemWidth(itemWidth);

            if (ImGui.RadioButton(names[i], EqualityComparer<T>.Default.Equals(currentValue, values.ElementAt(i))))
                result = values.ElementAt(i);
        }
        return result;
    }

    public static void SetFocusIfAppearing()
    {
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
    }

    public static void CenteredButtons(params (string text, Action action)[] buttons)
    {
        var group = new ImGuiHelpers.HorizontalButtonGroup()
        {
            IsCentered = true,
            Height = ImGui.GetTextLineHeight() + ImGui.GetStyle().FramePadding.Y * 2
        };
        foreach (var button in buttons)
            group.Add(button.text, button.action);
        group.Draw();
    }

    public static bool EnumCheckboxes<T>(ref List<T> selectedValues, IEnumerable<T>? excludeValues = null) where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>();
        var names = Enum.GetNames(typeof(T));
        var changed = false;

        for (var i = 0; i < values.Count(); i++)
        {
            var currentValue = values.ElementAt(i);
            if (excludeValues?.Contains(currentValue) == true)
                continue;

            var isSelected = selectedValues.Contains(currentValue);
            if (ImGui.Checkbox(names[i], ref isSelected))
            {
                changed = true;
                if (isSelected)
                    selectedValues.Add(currentValue);
                else
                    selectedValues.Remove(currentValue);
            }
        }
        return changed;
    }
}
