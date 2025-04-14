using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Old.Misc;

internal static class ImGuiUtils
{
    public static readonly Vector4 ShadedColor = new(0.68f, 0.68f, 0.68f, 1.0f);

    public static void DrawLink(string label, string title, string url)
    {
        ImGui.TextUnformatted(label);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

            using var tooltip = ImRaii.Tooltip();
            if (tooltip.Success)
            {
                ImGuiEx.Text(ImGuiColors.DalamudWhite, title);

                var pos = ImGui.GetCursorPos();
                ImGui.GetWindowDrawList().AddText(
                    UiBuilder.IconFont, 12,
                    ImGui.GetWindowPos() + pos + new Vector2(2),
                    ColorVecToUInt(ImGuiColors.DalamudGrey),
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

    public static void URLLink(string URL, string textToShow = "", bool showTooltip = true, ImFontPtr? iconFont = null)
    {
        using (var _ = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.Button]))
            ImGui.TextUnformatted(textToShow.Length > 0 ? textToShow : URL);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                Process.Start(new ProcessStartInfo(URL) { UseShellExecute = true });

            AddUnderline(ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonHovered], 1.0f);

            if (showTooltip)
            {
                using var _ = ImRaii.Tooltip();
                if (iconFont != null)
                {
                    using (var font = ImRaii.PushFont(iconFont.Value))
                        ImGui.TextUnformatted("\uF0C1");
                    ImGui.SameLine();
                }
                ImGui.TextUnformatted(URL);
            }
        }
        else
            AddUnderline(ImGui.GetStyle().Colors[(int)ImGuiCol.Button], 1.0f);
    }

    public static unsafe void ClickToCopyText(string text, Vector4 colour = default, string? textCopy = null)
    {
        textCopy ??= text;
        if (colour == default) colour = *ImGui.GetStyleColorVec4(ImGuiCol.Text);
        ImGui.TextColored(colour, text);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (textCopy != text) ImGui.SetTooltip(textCopy);
        }
        if (ImGui.IsItemClicked()) ImGui.SetClipboardText($"{textCopy}");
    }

    public static void TextLink(Action callback, string textToShow = "")
    {
        using (var _ = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonHovered]))
            ImGui.TextUnformatted(textToShow);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                callback.Invoke();

            AddUnderline(ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonHovered], 1.0f);
        }
        else
            AddUnderline(ImGui.GetStyle().Colors[(int)ImGuiCol.Button], 1.0f);
    }

    public static void AddUnderline(Vector4 color, float thickness)
    {
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        min.Y = max.Y;
        ImGui.GetWindowDrawList().AddLine(min, max, ColorVecToUInt(color), thickness);
    }

    public static void AddOverline(Vector4 color, float thickness)
    {
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        max.Y = min.Y;
        ImGui.GetWindowDrawList().AddLine(min, max, ColorVecToUInt(color), thickness);
    }

    public static void RightAlignTableText(string str)
    {
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetColumnWidth() - ImGui.CalcTextSize(str).X - ImGui.GetScrollX() - 2 * ImGui.GetStyle().ItemSpacing.X);
        ImGui.TextUnformatted(str);
    }

    public static uint ColorVecToUInt(Vector4 color)
    {
        return
        (uint)(color.X * 255f) << 0 |
        (uint)(color.Y * 255f) << 8 |
        (uint)(color.Z * 255f) << 16 |
        (uint)(color.W * 255f) << 24;
    }

    public static Vector4 ColorUIntToVec(uint color)
    {
        return new Vector4()
        {
            X = (color & 0xFF) / 255f,
            Y = (color & 0xFF00) / 255f,
            Z = (color & 0xFF0000) / 255f,
            W = (color & 0xFF000000) / 255f
        };
    }

    public static void HelpMarker(string description, bool sameLine = true, string marker = "(?)")
    {
        if (sameLine) ImGui.SameLine();
        ImGui.TextDisabled(marker);
        if (ImGui.IsItemHovered())
        {
            using var _ = ImRaii.Tooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(description);
            ImGui.PopTextWrapPos();
        }
    }

    public static void TitleBarLockButton(Action callback, uint idxFromRight = 1, ImFontPtr? iconFont = null)
    {
        var storedCursorPos = ImGui.GetCursorPos();
        if (iconFont != null) ImGui.PushFont(iconFont.Value);
        ImGui.PushClipRect(ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize(), false);

        try
        {
            //string buttonText = iconFont != null ? "\uF059" : "(?)";
            var buttonText = iconFont != null ? FontAwesomeIcon.Lock.ToIconString() : "(?)";

            var iconSize = ImGui.CalcTextSize(buttonText);
            var titlebarHeight = iconSize.Y + ImGui.GetStyle().FramePadding.Y * 2f;
            Vector2 buttonPos = new(ImGui.GetWindowSize().X - (iconSize.X + ImGui.GetStyle().FramePadding.X) * (idxFromRight + 1) - ImGui.GetStyle().WindowPadding.X + ImGui.GetScrollX(),
                                        Math.Max(0f, (titlebarHeight - iconSize.Y) / 2f - 1f) + ImGui.GetScrollY());

            ImGui.SetCursorPos(buttonPos);
            using (var _ = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudWhite))
                ImGui.TextUnformatted(buttonText);

            if (ImGui.IsItemHovered())
            {
                //	Redraw the text in the hovered color
                ImGui.SetCursorPos(buttonPos);
                ImGui.TextUnformatted(buttonText);

                //	Handle the click.
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    callback.Invoke();
                }
            }
        }
        finally
        {
            ImGui.SetCursorPos(storedCursorPos);
            if (iconFont != null) ImGui.PopFont();
            ImGui.PopClipRect();
        }
    }

    public const ImGuiWindowFlags OverlayWindowFlags = ImGuiWindowFlags.NoDecoration |
                                                        ImGuiWindowFlags.NoSavedSettings |
                                                        ImGuiWindowFlags.NoMove |
                                                        ImGuiWindowFlags.NoMouseInputs |
                                                        ImGuiWindowFlags.NoFocusOnAppearing |
                                                        ImGuiWindowFlags.NoBackground |
                                                        ImGuiWindowFlags.NoNav;

    public const ImGuiWindowFlags LayoutWindowFlags = ImGuiWindowFlags.NoSavedSettings |
                                                        ImGuiWindowFlags.NoMove |
                                                        ImGuiWindowFlags.NoMouseInputs |
                                                        ImGuiWindowFlags.NoFocusOnAppearing |
                                                        ImGuiWindowFlags.NoBackground |
                                                        ImGuiWindowFlags.NoNav |
                                                        ImGuiWindowFlags.NoScrollbar;
}
