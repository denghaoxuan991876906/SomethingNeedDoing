using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Framework.Lua;
using SomethingNeedDoing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Numerics;

namespace SomethingNeedDoing.Gui;

public class HelpUI
{
    private readonly LuaDocumentation _luaDocumentation;

    public HelpUI()
    {
        _luaDocumentation = new LuaDocumentation();
    }

    public void Draw()
    {
        // Add some padding at the top
        ImGui.Dummy(new Vector2(0, 5));
        
        using var tabs = ImRaii.TabBar("HelpTabs");
        if (!tabs) return;

        bool luaTabOpen;
        luaTabOpen = ImGui.BeginTabItem("Lua");

        if (luaTabOpen)
        {
            DrawLua();
            ImGui.EndTabItem();
        }

        bool cliTabOpen;
        cliTabOpen = ImGui.BeginTabItem("CLI");

        if (cliTabOpen)
        {
            DrawCli();
            ImGui.EndTabItem();
        }

        bool clicksTabOpen;
        clicksTabOpen = ImGui.BeginTabItem("Clicks");

        if (clicksTabOpen)
        {
            DrawClicks();
            ImGui.EndTabItem();
        }

        bool sendsTabOpen;
        sendsTabOpen = ImGui.BeginTabItem("Sends");

        if (sendsTabOpen)
        {
            DrawVirtualKeys();
            ImGui.EndTabItem();
        }

        bool conditionsTabOpen;
        conditionsTabOpen = ImGui.BeginTabItem("Conditions");

        if (conditionsTabOpen)
        {
            DrawConditions();
            ImGui.EndTabItem();
        }

        bool gitTabOpen;
        gitTabOpen = ImGui.BeginTabItem("Git Macros");

        if (gitTabOpen)
        {
            DrawGitMacros();
            ImGui.EndTabItem();
        }

        bool optionsTabOpen;
        optionsTabOpen = ImGui.BeginTabItem("Options");

        if (optionsTabOpen)
        {
            DrawOptions();
            ImGui.EndTabItem();
        }
    }

    private void DrawLua()
    {
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        // Draw general Lua information
        ImGui.TextWrapped(@"
Lua scripts work by yielding commands back to the macro engine.

For example:

yield(""/ac Muscle memory <wait.3>"")
yield(""/ac Precise touch <wait.2>"")
yield(""/echo done!"")
...and so on.");

        ImGui.Separator();

        // Draw registered Lua modules and functions
        var modules = _luaDocumentation.GetModules();
        foreach (var module in modules)
        {
            if (ImGui.CollapsingHeader(module.Name))
            {
                ImGui.Indent();

                // Module functions
                foreach (var function in module.Functions)
                {
                    using var functionId = ImRaii.PushId(function.Name);

                    // Function signature
                    var signature = $"{function.Name}({string.Join(", ", function.Parameters.Select(p => p.Name))})";
                    ImGui.TextColored(ImGuiColors.DalamudViolet, signature);

                    // Function description
                    ImGui.TextWrapped(function.Description);

                    // Example
                    if (!string.IsNullOrEmpty(function.Example))
                    {
                        ImGui.TextColored(ImGuiColors.DalamudGrey, "Example:");
                        using var exampleColor = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
                        ImGui.TextWrapped(function.Example);
                    }

                    ImGui.Separator();
                }

                ImGui.Unindent();
            }
        }
    }

    private void DrawCli()
    {
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        // CLI command data
        var cliData = new[]
        {
            ("help", "Show the help window.", null),
            ("run", "Run a macro, the name must be unique.", $"{P.Aliases[0]} run MyMacro"),
            ("run loop #", "Run a macro and then loop N times, the name must be unique.", $"{P.Aliases[0]} run loop 5 MyMacro"),
            ("pause", "Pause the currently executing macro.", null),
            ("pause loop", "Pause the currently executing macro at the next loop point.", null),
            ("resume", "Resume the currently paused macro.", null),
            ("stop", "Stop the currently executing macro.", null),
            ("stop loop", "Stop the currently executing macro at the next loop point.", null),
            ("cfg", "Change a configuration value.", $"{P.Aliases[0]} cfg EnableAutoUpdates true"),
        };

        ImGui.TextWrapped("The following commands can be used in chat or your macro text. You can use any of the aliases: " +
                         string.Join(", ", P.Aliases) + " or /somethingneeddoing");
        ImGui.Separator();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Advanced Examples:");
        ImGui.Columns(2);
        
        ImGui.TextWrapped("Run a macro that crafts 5 items:");
        ImGui.NextColumn();
        ImGui.TextColored(ImGuiColors.DalamudOrange, $"{P.Aliases[0]} run loop 5 MyCraftingMacro");
        ImGui.NextColumn();
        
        ImGui.TextWrapped("Run a macro and pause it at every loop:");
        ImGui.NextColumn();
        ImGui.TextColored(ImGuiColors.DalamudOrange, $"{P.Aliases[0]} run MyMacro\n{P.Aliases[0]} pause loop");
        ImGui.NextColumn();
        
        ImGui.TextWrapped("Stop all running macros:");
        ImGui.NextColumn();
        ImGui.TextColored(ImGuiColors.DalamudOrange, $"{P.Aliases[0]} stop");
        ImGui.NextColumn();
        
        ImGui.Columns(1);
        ImGui.Separator();

        foreach (var (name, desc, example) in cliData)
        {
            ImGui.TextUnformatted($"{P.Aliases[0]} {name}");

            using var colour = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
            ImGui.TextWrapped($"- Description: {desc}");

            if (example != null)
                ImGui.TextUnformatted($"- Example: {example}");

            ImGui.Separator();
        }

        // Macro trigger events information
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Macro Trigger Events");
        ImGui.TextWrapped("Macros can be triggered by various game events. You can set these in the macro editor.");
        
        var triggerEvents = new[]
        {
            ("OnCraftingStart", "Triggered when you start crafting an item"),
            ("OnCraftingEnd", "Triggered when crafting process completes"),
            ("OnCombatStart", "Triggered when entering combat"),
            ("OnCombatEnd", "Triggered when leaving combat"),
            ("OnLogin", "Triggered when you log in"),
            ("OnZoneChange", "Triggered when changing zones"),
        };
        
        ImGui.Indent(10);
        foreach (var (trigger, desc) in triggerEvents)
        {
            ImGui.TextColored(ImGuiColors.DalamudOrange, trigger);
            ImGui.SameLine();
            ImGui.TextWrapped(desc);
        }
        ImGui.Unindent(10);
    }

    private void DrawClicks()
    {
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        ImGui.TextWrapped("Click commands can be used to interact with game UI elements. You can use these in your macros.");
        ImGui.TextWrapped("Items in red are properties that themselves have methods (not callable directly).");
        ImGui.Separator();

        // Common scenarios section
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Common Usage Examples:");
        
        var commonExamples = new[]
        {
            ("Confirm a Yes/No dialog", "/click SelectYesno Yes"),
            ("Cancel a dialog", "/click SelectYesno No"),
            ("Select an option from a dropdown", "/click SelectString 2 (selects the 2nd option)"),
            ("Click on a context menu item", "/click ContextMenu Open\n/click ContextMenu 3 (selects the 3rd item)"),
            ("Click a specific item in your inventory", "/click Inventory 5 (clicks the 5th inventory slot)"),
            ("Press a button in a crafting window", "/click SynthesisResult Synthesize"),
            ("Close the current window", "/click Escape")
        };
        
        ImGui.BeginTable("ClickExamplesTable", 2, ImGuiTableFlags.Borders);
        ImGui.TableSetupColumn("Scenario", ImGuiTableColumnFlags.WidthFixed, 250 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();
        
        foreach (var (scenario, command) in commonExamples)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.TextWrapped(scenario);
            ImGui.TableSetColumnIndex(1);
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudOrange);
            ImGui.TextWrapped(command);
            ImGui.PopStyleColor();
        }
        
        ImGui.EndTable();
        ImGui.Separator();

        // Get all available clicks from helper method
        var clickNames = GetAvailableClicks();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Available Click Commands:");
        ImGui.BeginChild("ClicksList", new Vector2(-1, 300), true);
        
        foreach (var name in clickNames)
        {
            var isProperty = name.StartsWith('p');
            var displayName = isProperty ? name[1..] : name;
            var color = isProperty ? ImGuiColors.DalamudRed : ImGui.GetStyle().Colors[(int)ImGuiCol.Text];

            using var textColor = ImRaii.PushColor(ImGuiCol.Text, color);
            if (ImGui.Selectable($"/click {displayName}"))
            {
                ImGui.SetClipboardText($"/click {displayName}");
            }
            
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(isProperty ? 
                    "This is a property with methods. Cannot be called directly." : 
                    "Click to copy to clipboard");
                ImGui.EndTooltip();
            }
        }
        
        ImGui.EndChild();
    }

    private List<string> GetAvailableClicks()
    {
        // This is a simplified list of common click targets
        // In the actual implementation, these would be discovered from the game
        return [
            "SelectYesno Select",
            "SelectYesno Yes",
            "SelectYesno No",
            "SelectYesno Cancel",
            "SelectString Select",
            "SelectString Cancel",
            "pContextMenu Open",
            "pContextMenu Close",
            "pContextMenu Select",
            "Inventory",
            "CharacterStatus",
            "Chat",
            "SynthesisResult Synthesize",
            "SynthesisResult Cancel",
            "Journal",
            "Escape",
            "Teleport",
            "MarketBoard",
            "RetainerList",
            "pRetainer Inventory",
            "pRetainer Sell",
            "pRetainer Tasks",
            "Talk",
        ];
    }

    private void DrawVirtualKeys()
    {
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        ImGui.TextWrapped("Active keys will highlight green.");
        ImGui.Separator();

        // Get all valid virtual keys
        var validKeys = Svc.KeyState.GetValidVirtualKeys().ToHashSet();

        // Display each key with active state
        foreach (var key in Enum.GetValues<VirtualKey>())
        {
            if (!validKeys.Contains(key))
                continue;

            var isActive = Svc.KeyState[key];
            using var colour = ImRaii.PushColor(ImGuiCol.Text,
                ImGuiColors.HealerGreen, isActive);

            ImGui.TextUnformatted($"/send {key}");
        }
    }

    private void DrawConditions()
    {
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        ImGui.TextWrapped("These conditions can be used with the <condition> modifier in native macros. For example:");
        ImGui.TextColored(ImGuiColors.DalamudViolet, "/ac \"Byregot's Blessing\" <condition.crafting>");
        ImGui.TextWrapped("This command will only execute if you are currently crafting.");
        ImGui.Separator();

        // Display all game condition flags
        foreach (var condition in Enum.GetValues<ConditionFlag>())
        {
            var name = condition.ToString().ToLower();
            
            // Skip obsolete conditions
            if (name.Contains("obsolete", StringComparison.OrdinalIgnoreCase) || 
                name.Contains("dummy", StringComparison.OrdinalIgnoreCase))
                continue;
                
            var isActive = Svc.Condition[condition];
            using var colour = ImRaii.PushColor(ImGuiCol.Text, isActive ? ImGuiColors.HealerGreen : ImGui.GetStyle().Colors[(int)ImGuiCol.Text]);
            
            ImGui.Selectable($"<condition.{name}>");
            
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text($"Current state: {(isActive ? "Active" : "Inactive")}");
                ImGui.Text($"Usage example: /ac \"Some Action\" <condition.{name}>");
                ImGui.EndTooltip();
            }
        }
    }

    private void DrawOptions()
    {
        ImGui.TextWrapped("These are general options and settings for macros:");
        ImGui.Separator();

        // General settings section
        if (ImGui.CollapsingHeader("General Settings", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);

            var craftingSkip = C.CraftSkip;
            if (ImGui.Checkbox("Enable unsafe actions during crafting", ref craftingSkip))
            {
                C.CraftSkip = craftingSkip;
                C.Save();
            }
            
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("When enabled, allows potentially destructive actions during crafting");
                ImGui.Text("This might cause issues if actions are used at wrong times");
                ImGui.EndTooltip();
            }

            var useCraftLoopTemplate = C.UseCraftLoopTemplate;
            if (ImGui.Checkbox("Enable automatic updates for Git macros", ref useCraftLoopTemplate))
            {
                C.UseCraftLoopTemplate = useCraftLoopTemplate;
                C.Save();
            }
            
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("When enabled, Git macros will automatically check for updates");
                ImGui.Text("Updates are applied when macros are loaded");
                ImGui.EndTooltip();
            }

            ImGui.Unindent(10);
        }

        // Native macro options section
        if (ImGui.CollapsingHeader("Native Macro Options", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);

            var craftSkip = C.CraftSkip;
            if (ImGui.Checkbox("Skip craft commands when not crafting", ref craftSkip))
            {
                C.CraftSkip = craftSkip;
                C.Save();
            }
            
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("When enabled, crafting commands will be skipped if not in crafting mode");
                ImGui.Text("Example: /ac \"Muscle Memory\" will be skipped when not crafting");
                ImGui.EndTooltip();
            }

            var qualitySkip = C.QualitySkip;
            if (ImGui.Checkbox("Skip quality actions at max quality", ref qualitySkip))
            {
                C.QualitySkip = qualitySkip;
                C.Save();
            }
            
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("When enabled, quality-increasing actions will be skipped at max quality");
                ImGui.Text("Example: /ac \"Byregot's Blessing\" will be skipped when at max quality");
                ImGui.EndTooltip();
            }

            var loopTotal = C.LoopTotal;
            if (ImGui.Checkbox("Loop command specifies total iterations", ref loopTotal))
            {
                C.LoopTotal = loopTotal;
                C.Save();
            }
            
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("When enabled, /loop 5 means 'loop a total of 5 times'");
                ImGui.Text("When disabled, it means 'after executing once, loop 5 more times'");
                ImGui.EndTooltip();
            }

            ImGui.Unindent(10);
        }

        // Lua script options section
        if (ImGui.CollapsingHeader("Lua Script Options", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);

            ImGui.TextWrapped("Lua require paths (where to look for Lua modules):");
            ImGui.Separator();

            var paths = C.LuaRequirePaths.ToArray();
            for (var index = 0; index < paths.Length; index++)
            {
                var path = paths[index];
                
                if (ImGui.InputText($"Path #{index}", ref path, 200))
                {
                    var newPaths = paths.ToList();
                    newPaths[index] = path;
                    C.LuaRequirePaths = newPaths.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                    C.Save();
                }
            }

            if (ImGui.Button("Add Path"))
            {
                var newPaths = paths.ToList();
                newPaths.Add(string.Empty);
                C.LuaRequirePaths = newPaths.ToArray();
                C.Save();
            }

            ImGui.Unindent(10);
        }
    }

    private void DrawGitMacros()
    {
        ImGui.TextWrapped("Git macros allow you to use macros directly from GitHub repositories. These macros can be automatically updated when changes are made to the source repository.");
        ImGui.Separator();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "How to Add a Git Macro:");
        ImGui.Indent(10);

        ImGui.TextWrapped("1. Copy a GitHub URL to your clipboard. Supported formats:");
        ImGui.Indent(20);
        ImGui.TextColored(ImGuiColors.DalamudYellow, "• GitHub file URL: https://github.com/username/repo/blob/main/path/to/macro.txt");
        ImGui.TextColored(ImGuiColors.DalamudYellow, "• GitHub Gist URL: https://gist.github.com/username/gistid");
        ImGui.TextColored(ImGuiColors.DalamudYellow, "• GitLab file URL: https://gitlab.com/username/repo/blob/main/path/to/macro.txt");
        ImGui.Unindent(20);

        ImGui.TextWrapped("2. Click the 'New Macro' button in the main interface");
        ImGui.TextWrapped("3. The URL will be detected automatically and the macro will be imported from GitHub");
        ImGui.Unindent(10);
        ImGui.Separator();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Git Macro Features:");
        ImGui.Indent(10);

        ImGui.TextWrapped("• Automatic Updates: Git macros can check for updates and automatically download new versions");
        ImGui.TextWrapped("• Version History: View and restore previous versions of the macro");
        ImGui.TextWrapped("• Metadata: Git macros can include author information, version details, and documentation");
        ImGui.Unindent(10);
        ImGui.Separator();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Examples:");
        ImGui.TextWrapped("Here are some examples of community macros you can import:");
        ImGui.Indent(10);

        var exampleMacros = new[]
        {
            ("Basic Crafting Rotation", "https://github.com/example/ffxiv-macros/blob/main/crafting/basic.txt"),
            ("Expert Recipe", "https://github.com/example/ffxiv-macros/blob/main/crafting/expert.txt"),
            ("Simple Gathering Macro", "https://gist.github.com/example/123456789abcdef"),
        };

        foreach (var (name, url) in exampleMacros)
        {
            ImGui.TextColored(ImGuiColors.DalamudOrange, name);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Click to copy URL");
                ImGui.EndTooltip();
            }
            
            if (ImGui.IsItemClicked())
            {
                ImGui.SetClipboardText(url);
                ImGui.SetTooltip("Copied to clipboard!");
            }
            
            ImGui.SameLine();
            ImGui.TextWrapped(url);
        }
        
        ImGui.Unindent(10);
    }
}
