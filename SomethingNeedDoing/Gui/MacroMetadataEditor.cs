using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;
using SomethingNeedDoing.Framework.Interfaces;
using System;
using System.Numerics;

namespace SomethingNeedDoing.Gui;

/// <summary>
/// Enhanced MacroMetadataEditor with better UI and IDE-like experience
/// </summary>
public class MacroMetadataEditor
{
    // Add tab state for better organization
    private enum MetadataTab
    {
        General,
        Crafting,
        Triggers
    }
    
    private MetadataTab _currentTab = MetadataTab.General;
    
    public void Draw(IMacro macro)
    {
        if (macro?.Metadata == null)
            return;

        // Tab bar for metadata sections
        DrawMetadataTabs();
        
        // Spacer
        ImGui.Spacing();
        
        // Draw a container with padding and styling
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(10, 10));
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.15f, 0.15f, 0.15f, 0.5f));
        
        // Full width container for metadata content
        ImGui.BeginChild("MetadataContent", new Vector2(-1, 250), true);
        
        // Draw the appropriate tab content
        switch (_currentTab)
        {
            case MetadataTab.General:
                DrawGeneralTab(macro);
                break;
            case MetadataTab.Crafting:
                DrawCraftingTab(macro);
                break;
            case MetadataTab.Triggers:
                DrawTriggersTab(macro);
                break;
        }
        
        ImGui.EndChild();
        
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }
    
    private void DrawMetadataTabs()
    {
        float tabWidth = 100;
        float windowWidth = ImGui.GetContentRegionAvail().X;
        float startPos = (windowWidth - (tabWidth * 3)) / 2; // Center the tabs
        
        ImGui.SetCursorPosX(startPos);
        
        // Create styled tabs
        ImGui.PushStyleColor(ImGuiCol.Button, _currentTab == MetadataTab.General 
            ? ImGuiColors.ParsedPurple 
            : new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
        
        if (ImGui.Button("General", new Vector2(tabWidth, 0)))
            _currentTab = MetadataTab.General;
            
        ImGui.PopStyleColor();
        
        ImGui.SameLine();
        
        ImGui.PushStyleColor(ImGuiCol.Button, _currentTab == MetadataTab.Crafting 
            ? ImGuiColors.ParsedPurple 
            : new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            
        if (ImGui.Button("Crafting", new Vector2(tabWidth, 0)))
            _currentTab = MetadataTab.Crafting;
            
        ImGui.PopStyleColor();
        
        ImGui.SameLine();
        
        ImGui.PushStyleColor(ImGuiCol.Button, _currentTab == MetadataTab.Triggers 
            ? ImGuiColors.ParsedPurple 
            : new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            
        if (ImGui.Button("Triggers", new Vector2(tabWidth, 0)))
            _currentTab = MetadataTab.Triggers;
            
        ImGui.PopStyleColor();
    }
    
    private void DrawGeneralTab(IMacro macro)
    {
        // Create a nicer form layout with headers - without using custom fonts
        ImGui.TextColored(ImGuiColors.DalamudViolet, "General Information");
        
        ImGui.Separator();
        ImGui.Spacing();
        
        // Draw with better spacing and layout
        float labelWidth = 100;
        float spacing = 10;
        float inputWidth = ImGui.GetContentRegionAvail().X - labelWidth - spacing - 20;
        
        // Author field with nicer styling
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Author:");
        ImGui.SameLine(labelWidth + spacing);

        var author = macro.Metadata.Author ?? string.Empty;
        ImGui.SetNextItemWidth(inputWidth);
        
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
        bool authorChanged = ImGui.InputText("##Author", ref author, 100);
        ImGui.PopStyleColor();
        
        if (authorChanged)
        {
            macro.Metadata.Author = author;
            C.Save();
        }

        ImGui.Spacing();
        
        // Version field
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Version:");
        ImGui.SameLine(labelWidth + spacing);

        var version = macro.Metadata.Version ?? string.Empty;
        ImGui.SetNextItemWidth(inputWidth);
        
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
        bool versionChanged = ImGui.InputText("##Version", ref version, 50);
        ImGui.PopStyleColor();
        
        if (versionChanged)
        {
            macro.Metadata.Version = version;
            C.Save();
        }

        ImGui.Spacing();
        
        // Description field
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Description:");
        
        // Full width multiline input for description
        ImGui.Spacing();
        var description = macro.Metadata.Description ?? string.Empty;
        
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
        bool descChanged = ImGui.InputTextMultiline("##Description", ref description, 1000, new Vector2(-1, 100));
        ImGui.PopStyleColor();
        
        if (descChanged)
        {
            macro.Metadata.Description = description;
            C.Save();
        }
    }
    
    private void DrawCraftingTab(IMacro macro)
    {
        // Header for crafting section - without custom fonts
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Crafting Settings");
        
        ImGui.Separator();
        ImGui.Spacing();
        
        float labelWidth = 120;
        float spacing = 10;
        
        // Crafting Loop checkbox with better styling
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Enable Crafting Loop:");
        ImGui.SameLine(labelWidth + spacing);

        var craftingLoop = macro.Metadata.CraftingLoop;
        
        // Better looking checkbox
        if (ImGuiComponents.ToggleButton("##CraftingLoop", ref craftingLoop))
        {
            macro.Metadata.CraftingLoop = craftingLoop;
            C.Save();
        }
        
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.HealerGreen, craftingLoop ? "Enabled" : "Disabled");

        // Show loop settings only if crafting loop is enabled
        if (craftingLoop)
        {
            ImGui.Spacing();
            ImGui.Spacing();
            
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Loop Count:");
            ImGui.SameLine(labelWidth + spacing);

            ImGui.SetNextItemWidth(100);
            var craftLoopCount = macro.Metadata.CraftLoopCount;
            
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            if (ImGui.InputInt("##LoopCount", ref craftLoopCount))
            {
                if (craftLoopCount < -1)
                    craftLoopCount = -1;

                macro.Metadata.CraftLoopCount = craftLoopCount;
                C.Save();
            }
            ImGui.PopStyleColor();

            ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.DalamudGrey, "(-1 = infinite)");
            
            // Add helpful tip
            ImGui.Spacing();
            ImGui.TextWrapped("When crafting loop is enabled, the macro will automatically repeat for the specified number of crafts.");
        }
    }
    
    private void DrawTriggersTab(IMacro macro)
    {
        // Header for triggers section - without custom fonts
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Trigger Events");
        
        ImGui.Separator();
        ImGui.Spacing();
        
        // Trigger on Auto Retainer
        bool arPostProcess = macro.Metadata.TriggerEvents.Contains(TriggerEvent.OnAutoRetainerCharacterPostProcess);
        
        if (ImGuiComponents.ToggleButton("##ARPostProcess", ref arPostProcess))
        {
            if (arPostProcess)
                macro.Metadata.TriggerEvents.Add(TriggerEvent.OnAutoRetainerCharacterPostProcess);
            else
                macro.Metadata.TriggerEvents.Remove(TriggerEvent.OnAutoRetainerCharacterPostProcess);
                
            C.Save();
        }
        
        ImGui.SameLine();
        ImGui.Text("Run after Auto Retainer completes");
        
        ImGui.Spacing();
        ImGui.Spacing();
        
        // Other triggers could be added here in the future
        
        // Help text
        ImGui.Spacing();
        ImGui.TextColored(ImGuiColors.DalamudGrey, "Triggers allow macros to run automatically in response to events.");
    }
}
