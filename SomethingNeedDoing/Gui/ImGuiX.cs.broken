using Dalamud.Interface;using ImGuiNET;using System.Numerics;using SomethingNeedDoing.Utils;using ECommons;using Dalamud.Interface.Utility;using System;

namespace SomethingNeedDoing.Gui;

internal static class ImGuiX
{
    /// <summary>
    /// An icon button.
    /// </summary>
    /// <param name="icon">Icon value.</param>
    /// <param name="tooltip">Simple tooltip.</param>
    /// <returns>Result from ImGui.Button.</returns>
    public static bool IconButton(FontAwesomeIcon icon, string tooltip)
    {
        bool result;
        
        ImGui.PushFont(UiBuilder.IconFont);
        result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-{tooltip}");
        ImGui.PopFont();
        
        // Show tooltip if hovered
        if (tooltip != null)
            TextTooltip(tooltip);

        return result;
    }

    /// <summary>
    /// Show a simple text tooltip if hovered.
    /// </summary>
    /// <param name="text">Text to display.</param>
    public static void TextTooltip(string text)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(text);
            ImGui.EndTooltip();
        }
    }

    /// <summary>
    /// Get the current RGBA color for the given widget.
    /// </summary>
    /// <param name="col">The type of color to fetch.</param>
    /// <returns>A RGBA vec4.</returns>
    public static Vector4 GetStyleColorVec4(ImGuiCol col)
    {
        unsafe
        {
            return *ImGui.GetStyleColorVec4(col);
        }
    }

    /// <summary>
    /// Creates a button with both an icon and text.
    /// </summary>
    /// <param name="icon">The icon to display on the button.</param>
    /// <param name="text">The text to display next to the icon.</param>
    /// <param name="size">Optional size for the button.</param>
    /// <returns>True if the button was clicked.</returns>
    public static bool IconTextButton(FontAwesomeIcon icon, string text, Vector2? size = null)
    {
        string iconStr;
        
        ImGui.PushFont(UiBuilder.IconFont);
        iconStr = icon.ToIconString();
        ImGui.PopFont();
        
        return size.HasValue 
            ? ImGui.Button($"{iconStr} {text}", size.Value) 
            : ImGui.Button($"{iconStr} {text}");
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
        // Use the actual icon
        string iconStr = GetIconString(icon);
        return ImGui.MenuItem($"{iconStr} {label}", string.Empty, selected, enabled);
    }
    
    /// <summary>
    /// Gets the string representation of an icon with proper font handling.
    /// </summary>
    /// <param name="icon">The icon to get the string for.</param>
    /// <returns>The string representation of the icon.</returns>
    public static string GetIconString(FontAwesomeIcon icon)
    {
        string result;
        ImGui.PushFont(UiBuilder.IconFont);
        result = icon.ToIconString();
        ImGui.PopFont();
        return result;
    }
} 