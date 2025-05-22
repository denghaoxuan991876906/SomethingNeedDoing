using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Documentation;
// using SomethingNeedDoing.Framework.Lua; // Comment this out to avoid ambiguity
using SomethingNeedDoing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SomethingNeedDoing.Gui;

public class HelpUI
{
    private readonly Documentation.LuaDocumentation _luaDocumentation;

    public HelpUI()
    {
        _luaDocumentation = new Documentation.LuaDocumentation();
    }

    public void Draw()
    {
        // Add some padding at the top
        ImGui.Dummy(new Vector2(0, 5));
        
        using var tabs = ImRaii.TabBar("HelpTabs");
        if (!tabs) return;

        if (ImGui.BeginTabItem("General"))
        {
            DrawGeneralHelp();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Lua"))
        {
            DrawLua();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("CLI"))
        {
            DrawCli();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Clicks"))
        {
            DrawClicks();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Keys & Sends"))
        {
            DrawVirtualKeys();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Conditions"))
        {
            DrawConditions();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Git Macros"))
        {
            DrawGitMacros();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Options"))
        {
            DrawOptions();
            ImGui.EndTabItem();
        }
    }

    private void DrawGeneralHelp()
    {
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Something Need Doing - Macro Helper");
        
        // Introduction section
        ImGui.TextWrapped(
            "Something Need Doing is a powerful macro automation tool for FFXIV that extends the game's native macro " +
            "system with additional features like loops, waits, and Lua scripting.");
        
        ImGui.Separator();
        
        // Key features section
        if (ImGui.CollapsingHeader("Key Features", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);
            
            var features = new[]
            {
                ("Native Macros", "Enhanced version of in-game macros with additional commands and modifiers"),
                ("Lua Scripting", "Full Lua scripting support for complex automation needs"),
                ("UI Automation", "Interact with game UI elements through clicks and keyboard inputs"),
                ("Conditional Logic", "Run commands based on game state using conditions"),
                ("Looping", "Repeat actions multiple times with the loop command"),
                ("Git Integration", "Import and update macros directly from GitHub repositories")
            };
            
            foreach (var (feature, description) in features)
            {
                ImGui.TextColored(ImGuiColors.DalamudOrange, feature);
                ImGui.SameLine();
                ImGui.TextWrapped(description);
                ImGui.Spacing();
            }
            
            ImGui.Unindent(10);
        }
        
        ImGui.Separator();
        
        // Quick start section
        if (ImGui.CollapsingHeader("Quick Start Guide", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);
            
            ImGui.TextWrapped("1. Create a new macro using the 'New Macro' button in the main window");
            ImGui.TextWrapped("2. Enter your commands or Lua script in the editor");
            ImGui.TextWrapped("3. Run the macro with the 'Run' button or with the command:");
            ImGui.TextColored(ImGuiColors.DalamudYellow, $"    /{P.Aliases[0]} run MyMacroName");
            ImGui.TextWrapped("4. For more detailed information, check the other tabs in this help window");
            
            ImGui.Unindent(10);
        }
        
        ImGui.Separator();
        
        // Common use cases
        if (ImGui.CollapsingHeader("Common Use Cases", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent(10);
            
            var useCases = new[]
            {
                ("Crafting Automation", "Create macros that craft multiple items in sequence"),
                ("Gathering Routes", "Automate gathering rotations and teleport sequences"),
                ("UI Navigation", "Navigate through multiple game menus automatically"),
                ("Retainer Management", "Automate sorting, selling, and ventures"),
                ("Custom Triggers", "Set up macros that run automatically on certain game events")
            };
            
            foreach (var (useCase, description) in useCases)
            {
                ImGui.TextColored(ImGuiColors.DalamudOrange, useCase);
                ImGui.SameLine();
                ImGui.TextWrapped(description);
                ImGui.Spacing();
            }
            
            ImGui.Unindent(10);
        }
    }

    private void DrawLua()
    {
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        // Introduction to Lua
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Lua Scripting");
        ImGui.TextWrapped(
            "SomethingNeedDoing supports Lua scripting for advanced automation. " +
            "Lua scripts can do everything native macros can do and much more.");
        
        ImGui.Separator();
        
        // Basic syntax and usage
        if (ImGui.CollapsingHeader("Basic Lua Usage", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.TextWrapped(@"
Lua scripts work by yielding commands back to the macro engine.

For example:

yield(""/ac Muscle memory <wait.3>"")
yield(""/ac Precise touch <wait.2>"")
yield(""/echo done!"")
...and so on.

You can also use regular Lua syntax for complex logic:

for i = 1, 5 do
    yield(""/echo Loop iteration "" .. i)
    if i == 3 then
        yield(""/echo Halfway done!"")
    end
end");
        }
        
        ImGui.Separator();

        // Draw registered Lua modules and functions
        var modules = _luaDocumentation.GetModules();
        foreach (var module in modules)
        {
            if (ImGui.CollapsingHeader(module.Name))
            {
                ImGui.Indent();

                // Module description
                if (!string.IsNullOrEmpty(module.Description))
                {
                    ImGui.TextWrapped(module.Description);
                    ImGui.Separator();
                }

                // Module functions
                foreach (var function in module.Functions)
                {
                    using var functionId = ImRaii.PushId(function.Name);

                    // Function signature
                    var signature = $"{function.Name}({string.Join(", ", function.Parameters.Select(p => p.Name))})";
                    ImGui.TextColored(ImGuiColors.DalamudViolet, signature);

                    // Function description
                    ImGui.TextWrapped(function.Description);

                    // Parameters
                    if (function.Parameters.Count > 0)
                    {
                        ImGui.TextColored(ImGuiColors.DalamudGrey, "Parameters:");
                        ImGui.Indent();
                        foreach (var param in function.Parameters)
                        {
                            ImGui.TextColored(ImGuiColors.DalamudOrange, param.Name);
                            ImGui.SameLine();
                            ImGui.TextWrapped($"- {param.Description}");
                        }
                        ImGui.Unindent();
                    }

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

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Command Line Interface (CLI)");
        ImGui.TextWrapped("The following commands can be used in chat or your macro text. You can use any of the aliases: " +
                         string.Join(", ", P.Aliases) + " or /somethingneeddoing");
        ImGui.Separator();

        // Command reference table
        ImGui.BeginTable("CommandReferenceTable", 3, ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersInner);
        
        ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthFixed, 180 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("Example", ImGuiTableColumnFlags.WidthFixed, 200 * ImGuiHelpers.GlobalScale);
        ImGui.TableHeadersRow();

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

        foreach (var (name, desc, example) in cliData)
        {
            ImGui.TableNextRow();
            
            ImGui.TableSetColumnIndex(0);
            ImGui.TextUnformatted($"{P.Aliases[0]} {name}");
            
            ImGui.TableSetColumnIndex(1);
            ImGui.TextWrapped(desc);
            
            ImGui.TableSetColumnIndex(2);
            if (example != null)
                ImGui.TextColored(ImGuiColors.DalamudOrange, example);
            
        }
        
        ImGui.EndTable();
        
        ImGui.Separator();

        // Advanced examples section
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Advanced Examples:");
        
        ImGui.BeginTable("AdvancedExamplesTable", 2, ImGuiTableFlags.BordersOuter);
        ImGui.TableSetupColumn("Scenario", ImGuiTableColumnFlags.WidthFixed, 250 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthStretch);
        
        var advancedExamples = new[]
        {
            ("Run a macro that crafts 5 items:", $"{P.Aliases[0]} run loop 5 MyCraftingMacro"),
            ("Run a macro and pause it at every loop:", $"{P.Aliases[0]} run MyMacro\n{P.Aliases[0]} pause loop"),
            ("Stop all running macros:", $"{P.Aliases[0]} stop"),
            ("Resume a paused macro:", $"{P.Aliases[0]} resume"),
        };
        
        foreach (var (scenario, command) in advancedExamples)
        {
            ImGui.TableNextRow();
            
            ImGui.TableSetColumnIndex(0);
            ImGui.TextWrapped(scenario);
            
            ImGui.TableSetColumnIndex(1);
            ImGui.TextColored(ImGuiColors.DalamudOrange, command);
        }
        
        ImGui.EndTable();
        
        ImGui.Separator();

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

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Click Commands");
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

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Virtual Key Commands");
        ImGui.TextWrapped("The /send command can be used to send virtual key presses to the game.");
        ImGui.TextWrapped("This can be useful for interacting with UI elements that don't have specific click commands.");
        ImGui.TextWrapped("Active keys will highlight green.");
        ImGui.Separator();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Usage Examples:");
        ImGui.Indent(10);
        ImGui.TextWrapped("/send ESC - Press the Escape key");
        ImGui.TextWrapped("/send RETURN - Press the Enter/Return key");
        ImGui.TextWrapped("/send F12 - Press the F12 key");
        ImGui.Unindent(10);
        ImGui.Separator();

        // Get all valid virtual keys
        var validKeys = Svc.KeyState.GetValidVirtualKeys().ToHashSet();

        // Create a more organized layout with columns
        int columns = 3;
        ImGui.Columns(columns, "VirtualKeysColumns", false);
        
        int counter = 0;
        
        // Display each key with active state
        foreach (var key in Enum.GetValues<VirtualKey>())
        {
            if (!validKeys.Contains(key))
                continue;

            var isActive = Svc.KeyState[key];
            using var colour = ImRaii.PushColor(ImGuiCol.Text,
                isActive ? ImGuiColors.HealerGreen : ImGui.GetStyle().Colors[(int)ImGuiCol.Text]);

            if (ImGui.Selectable($"/send {key}"))
            {
                ImGui.SetClipboardText($"/send {key}");
            }
            
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text($"Click to copy to clipboard");
                ImGui.Text($"Current state: {(isActive ? "Active" : "Inactive")}");
                ImGui.EndTooltip();
            }

            counter++;
            if (counter % (validKeys.Count / columns + 1) == 0)
                ImGui.NextColumn();
        }
        
        ImGui.Columns(1);
    }

    private void DrawConditions()
    {
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Condition Modifiers");
        ImGui.TextWrapped("These conditions can be used with the <condition> modifier in native macros. For example:");
        ImGui.TextColored(ImGuiColors.DalamudOrange, "/ac \"Byregot's Blessing\" <condition.crafting>");
        ImGui.TextWrapped("This command will only execute if you are currently crafting.");
        ImGui.Separator();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Usage Examples:");
        ImGui.Indent(10);
        ImGui.TextWrapped("/ac \"Inner Quiet\" <condition.crafting> - Only use if crafting");
        ImGui.TextWrapped("/ac \"Veneration\" <condition.crafting> <condition.notboundbydutyfinder> - Only use if crafting and not in a duty");
        ImGui.TextWrapped("/ac \"Standard Step\" <condition.incombat> - Only use if in combat");
        ImGui.Unindent(10);
        ImGui.Separator();

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Available Conditions:");
        ImGui.TextWrapped("(Green conditions are currently active)");
        
        // Create a more organized layout with columns
        int columns = 3;
        ImGui.Columns(columns, "ConditionsColumns", false);
        
        int counter = 0;
        int totalConditions = Enum.GetValues<ConditionFlag>().Length;

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
            
            if (ImGui.Selectable($"<condition.{name}>"))
            {
                ImGui.SetClipboardText($"<condition.{name}>");
            }
            
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text($"Current state: {(isActive ? "Active" : "Inactive")}");
                ImGui.Text($"Usage example: /ac \"Some Action\" <condition.{name}>");
                ImGui.EndTooltip();
            }
            
            counter++;
            if (counter % (totalConditions / columns + 1) == 0)
                ImGui.NextColumn();
        }
        
        ImGui.Columns(1);
    }

    private void DrawGitMacros()
    {
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Git Macros");
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

        ImGui.TextColored(ImGuiColors.DalamudViolet, "Creating Your Own Git Macros:");
        ImGui.Indent(10);
        ImGui.TextWrapped("You can create your own Git macros by creating a GitHub repository or Gist. The macro file should contain the macro content in the same format as you would enter in the macro editor.");
        ImGui.TextWrapped("You can add metadata to your macro by adding comments at the beginning of the file:");
        
        ImGui.TextColored(ImGuiColors.DalamudYellow, "// Name: My Awesome Macro");
        ImGui.TextColored(ImGuiColors.DalamudYellow, "// Description: This macro does something awesome");
        ImGui.TextColored(ImGuiColors.DalamudYellow, "// Author: YourName");
        ImGui.TextColored(ImGuiColors.DalamudYellow, "// Version: 1.0");
        
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

    private void DrawOptions()
    {
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Options and Settings");
        ImGui.TextWrapped("These settings control how macros behave in different situations.");
        ImGui.Separator();

        // Create separate collapsing headers for different categories of settings
        
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
        
        // Click automation options
        if (ImGui.CollapsingHeader("Click and UI Automation Options"))
        {
            ImGui.Indent(10);
            
            ImGui.TextWrapped("These settings control how UI automation behaves.");
            
            var stopMacroIfAddonNotFound = C.StopMacroIfAddonNotFound;
            if (ImGui.Checkbox("Stop macro if UI element is not found", ref stopMacroIfAddonNotFound))
            {
                C.StopMacroIfAddonNotFound = stopMacroIfAddonNotFound;
                C.Save();
            }
            
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("When enabled, macros will stop if a UI element is not found");
                ImGui.Text("Otherwise, they will skip the command and continue");
                ImGui.EndTooltip();
            }
            
            ImGui.Unindent(10);
        }
    }
}
