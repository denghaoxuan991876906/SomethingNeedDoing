using Dalamud.Interface;

namespace SomethingNeedDoing.Gui;

internal static class ImGuiX
{
    /// <summary>
    /// Creates a button with both an icon and text.
    /// </summary>
    /// <param name="icon">The icon to display on the button.</param>
    /// <param name="text">The text to display next to the icon.</param>
    /// <param name="size">Optional size for the button.</param>
    /// <returns>True if the button was clicked.</returns>
    public static bool IconTextButton(FontAwesomeIcon icon, string text, Vector2? size = null)
    {
        // The most basic and reliable approach
        var id = $"##Button_{icon}_{text}";

        // Start a group so all components are treated as one item
        ImGui.BeginGroup();

        // Create a button with just an ID
        var result = size.HasValue
            ? ImGui.Button(id, size.Value)
            : ImGui.Button(id);

        // Get position for drawing the icon and text
        var buttonX = ImGui.GetItemRectMin().X;
        var buttonY = ImGui.GetItemRectMin().Y;
        var buttonWidth = ImGui.GetItemRectSize().X;
        var buttonHeight = ImGui.GetItemRectSize().Y;

        // Center content vertically
        var offsetY = (buttonHeight - ImGui.GetTextLineHeight()) * 0.5f;

        // Draw at a fixed position, not affected by cursor
        ImGui.SetCursorScreenPos(new Vector2(buttonX + 10, buttonY + offsetY));

        // Draw icon with icon font
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text(icon.ToIconString());
        ImGui.PopFont();

        // Get the width of the icon
        ImGui.PushFont(UiBuilder.IconFont);
        var iconWidth = ImGui.CalcTextSize(icon.ToIconString()).X;
        ImGui.PopFont();

        // Draw the text with proper spacing
        ImGui.SetCursorScreenPos(new Vector2(buttonX + 10 + iconWidth + 5, buttonY + offsetY));
        ImGui.Text(text);

        ImGui.EndGroup();

        return result;
    }

    /// <summary>
    /// Display an icon without pushing/popping fonts
    /// </summary>
    /// <param name="icon">The FontAwesomeIcon to display</param>
    public static void Icon(FontAwesomeIcon icon)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text(icon.ToIconString());
        ImGui.PopFont();
    }

    /// <summary>
    /// Creates a menu item with an icon and text.
    /// </summary>
    /// <param name="icon">The icon to use.</param>
    /// <param name="label">The text to display next to the icon.</param>
    /// <param name="selected">Whether the item is selected.</param>
    /// <param name="enabled">Whether the item is enabled.</param>
    /// <returns>True if the menu item was clicked.</returns>
    public static bool IconMenuItem(FontAwesomeIcon icon, string label, bool selected = false, bool enabled = true)
    {
        // Create a unique ID for this menu item
        var menuId = $"##Menu_{icon}_{label}";

        // Use simple approach: create a MenuItem with just an ID, then overlay text
        var result = ImGui.MenuItem(menuId, string.Empty, selected, enabled);

        // Only draw the icon and text if we should be rendering them
        // (this prevents drawing when the menu is closed)
        if (ImGui.IsItemVisible())
        {
            // Get position for drawing
            var itemX = ImGui.GetItemRectMin().X;
            var itemY = ImGui.GetItemRectMin().Y;
            var itemHeight = ImGui.GetItemRectSize().Y;

            // Backup cursor position
            var cursorPos = ImGui.GetCursorPos();

            // Center content vertically
            var offsetY = (itemHeight - ImGui.GetTextLineHeight()) * 0.5f;

            // Draw icon with icon font - position at start of item
            ImGui.SetCursorScreenPos(new Vector2(itemX + 5, itemY + offsetY));
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.Text(icon.ToIconString());
            var iconWidth = ImGui.CalcTextSize(icon.ToIconString()).X;
            ImGui.PopFont();

            // Draw the label after the icon
            ImGui.SetCursorScreenPos(new Vector2(itemX + 10 + iconWidth, itemY + offsetY));
            ImGui.Text(label);

            // Restore cursor position
            ImGui.SetCursorPos(cursorPos);
        }

        return result;
    }
}
