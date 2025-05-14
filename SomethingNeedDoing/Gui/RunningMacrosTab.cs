using Dalamud.Interface.Windowing;
using ImGuiNET;
using SomethingNeedDoing.Framework.Interfaces;
using SomethingNeedDoing.Scheduler;
using System.Numerics;

namespace SomethingNeedDoing.Gui;

public class RunningMacrosTab : Window
{
    private readonly RunningMacrosPanel _panel;
    private readonly IMacroScheduler _scheduler;
    private readonly MacroHierarchyManager _hierarchyManager;

    public RunningMacrosTab(IMacroScheduler scheduler, MacroHierarchyManager hierarchyManager) 
        : base("Running Macros", ImGuiWindowFlags.NoScrollbar)
    {
        _scheduler = scheduler;
        _hierarchyManager = hierarchyManager;
        _panel = new RunningMacrosPanel(scheduler);
        
        Size = new Vector2(600, 400);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        // Draw the running macros panel at full size
        _panel.DrawDetailed();
        
        // Optional: Draw macro hierarchy tree below
        DrawMacroHierarchy();
    }

    private void DrawMacroHierarchy()
    {
        if (ImGui.CollapsingHeader("Macro Hierarchy", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var macros = _scheduler.GetMacros();
            if (!macros.Any())
            {
                ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), "No running macros");
                return;
            }

            foreach (var macro in macros)
            {
                // Only show parent macros in the hierarchy view
                if (_hierarchyManager.GetParentMacro(macro.Id) == null)
                {
                    DrawMacroNode(macro);
                }
            }
        }
    }

    private void DrawMacroNode(IMacro macro)
    {
        var children = _hierarchyManager.GetChildMacros(macro.Id);
        bool hasChildren = children.Any();
        
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None;
        if (!hasChildren)
            flags |= ImGuiTreeNodeFlags.Leaf;

        bool isOpen = ImGui.TreeNodeEx($"{macro.Name} ({macro.State})", flags);
        
        if (isOpen)
        {
            if (hasChildren)
            {
                foreach (var child in children)
                {
                    DrawMacroNode(child);
                }
            }
            
            ImGui.TreePop();
        }
    }
} 