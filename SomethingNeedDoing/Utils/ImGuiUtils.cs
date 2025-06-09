using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SomethingNeedDoing.Utils;
public static class ImGuiUtils
{
    public static class Colours
    {
        public static EzColor Gold => new(0.847f, 0.733f, 0.49f);
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
        var group = new ImGuiEx.EzButtonGroup() { IsCentered = true };
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

    public static void DrawLink(Vector4 colour, string label, string url)
    {
        ImGuiEx.Text(colour, label);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

            using var tooltip = ImRaii.Tooltip();
            if (tooltip.Success)
            {
                var pos = ImGui.GetCursorPos();
                ImGui.GetWindowDrawList().AddText(
                    UiBuilder.IconFont, 12,
                    ImGui.GetWindowPos() + pos + new Vector2(2),
                    ImGuiColors.DalamudGrey.ToUint(),
                    FontAwesomeIcon.ExternalLinkAlt.ToIconString()
                );
                ImGui.SetCursorPos(pos + new Vector2(20, 0));
                ImGuiEx.Text(ImGuiColors.DalamudGrey, url);
            }
        }

        if (ImGui.IsItemClicked())
        {
            Task.Run(() => Dalamud.Utility.Util.OpenLink(url));
        }
    }

    public static bool Button(Vector4 btnColour, FontAwesomeIcon icon, string text)
    {
        using var group = ImRaii.Group();
        using var _ = ImRaii.PushColor(ImGuiCol.Button, btnColour)
            .Push(ImGuiCol.ButtonHovered, new Vector4(btnColour.X + 1, btnColour.Y + 1, btnColour.Z + 1, btnColour.W))
            .Push(ImGuiCol.ButtonActive, new Vector4(btnColour.X + 2, btnColour.Y + 2, btnColour.Z + 2, btnColour.W));

        var id = $"##Button_{icon}_{text}";

        // Calculate minimum width based on text and icon
        float iconWidth;
        using (ImRaii.PushFont(UiBuilder.IconFont))
            iconWidth = ImGui.CalcTextSize(icon.ToIconString()).X;
        var textWidth = ImGui.CalcTextSize(text).X;
        var style = ImGui.GetStyle();
        var minWidth = iconWidth + textWidth + style.FramePadding.X * 2 + style.ItemSpacing.X;
        var buttonSize = new Vector2(minWidth, ImGui.GetFrameHeight());

        var result = ImGui.Button(id, buttonSize);

        var buttonX = ImGui.GetItemRectMin().X;
        var buttonY = ImGui.GetItemRectMin().Y;
        var buttonHeight = ImGui.GetItemRectSize().Y;

        ImGui.SetCursorScreenPos(new Vector2(buttonX + style.FramePadding.X, buttonY));
        ImGui.AlignTextToFramePadding();

        using (ImRaii.PushFont(UiBuilder.IconFont))
            ImGui.TextUnformatted(icon.ToIconString());

        ImGui.SameLine(0, style.ItemSpacing.X);
        ImGui.Text(text);

        return result;
    }

    public static void Section(string name, Action content, bool indentContent = false, ImFontPtr? contentFont = null)
    {
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.CellPadding, new Vector2(7f));
        using var table = ImRaii.Table($"##Section_{name}", 1, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit);
        if (!table) return;

        ImGui.TableSetupColumn(name, ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, new EzColor(ImGuiColors.DalamudViolet) with { A = 0.2f });
        ImGuiEx.Text(name);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        using var font = ImRaii.PushFont(contentFont ?? ImGui.GetFont());
        using (ImRaii.PushIndent(condition: indentContent))
            content();
    }
}
