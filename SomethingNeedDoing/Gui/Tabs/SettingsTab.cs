using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Gui.Modals;

namespace SomethingNeedDoing.Gui.Tabs;
public static class SettingsTab
{
    public static void DrawTab()
    {
        using var _ = ImRaii.Child("SettingsTab", Vector2.Create(-1), false);

        if (ImGui.CollapsingHeader("General Settings", ImGuiTreeNodeFlags.DefaultOpen))
        {
            using var __ = ImRaii.PushIndent();
            var useCraftLoopTemplate = C.UseCraftLoopTemplate;
            if (ImGui.Checkbox("Enable automatic updates for Git macros", ref useCraftLoopTemplate))
            {
                C.UseCraftLoopTemplate = useCraftLoopTemplate;
                C.Save();
            }

            var craftSkip = C.CraftSkip;
            if (ImGui.Checkbox("Enable unsafe actions during crafting", ref craftSkip))
            {
                C.CraftSkip = craftSkip;
                C.Save();
            }
        }

        // Native macro options
        if (ImGui.CollapsingHeader("Native Macro Options"))
        {
            using var __ = ImRaii.PushIndent();
            var qualitySkip = C.QualitySkip;
            if (ImGui.Checkbox("Skip quality actions at max quality", ref qualitySkip))
            {
                C.QualitySkip = qualitySkip;
                C.Save();
            }
            ImGuiEx.Tooltip("When enabled, quality-increasing actions will be skipped at max quality");

            var loopTotal = C.LoopTotal;
            if (ImGui.Checkbox("Loop command specifies total iterations", ref loopTotal))
            {
                C.LoopTotal = loopTotal;
                C.Save();
            }
            ImGuiEx.Tooltip("When enabled, /loop 5 means 'loop a total of 5 times' instead of 'loop 5 more times'");
        }

        // UI Automation options
        if (ImGui.CollapsingHeader("UI Automation Options"))
        {
            using var __ = ImRaii.PushIndent();
            var stopMacroIfAddonNotFound = C.StopMacroIfAddonNotFound;
            if (ImGui.Checkbox("Stop macro if UI element is not found", ref stopMacroIfAddonNotFound))
            {
                C.StopMacroIfAddonNotFound = stopMacroIfAddonNotFound;
                C.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("When enabled, macros will stop if a UI element is not found");
            }
        }

        // Lua Script Options
        if (ImGui.CollapsingHeader("Lua Script Options"))
        {
            using var __ = ImRaii.PushIndent();
            ImGui.TextWrapped("Lua require paths (where to look for Lua modules):");

            var paths = C.LuaRequirePaths.ToArray();
            for (var index = 0; index < paths.Length; index++)
            {
                var path = paths[index];

                if (ImGui.InputText($"Path #{index}", ref path, 200))
                {
                    var newPaths = paths.ToList();
                    newPaths[index] = path;
                    C.LuaRequirePaths = [.. newPaths.Where(p => !string.IsNullOrWhiteSpace(p))];
                    C.Save();
                }
            }

            if (ImGui.Button("Add Path"))
            {
                var newPaths = paths.ToList();
                newPaths.Add(string.Empty);
                C.LuaRequirePaths = [.. newPaths];
                C.Save();
            }
        }

        ImGui.Separator();
        ImGui.Spacing();

        // Import/Export Section
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Legacy Macro Import");
        ImGui.Spacing();

        // Instructions for import
        ImGui.TextWrapped($"Import macros from the old version of {P.Name}. These are not guaranteed to work any more but can be imported as a reference.\n" +
            "You can copy an old config to clipboard and click the import button, or it will automatically attempt to find the old config file.");
        ImGui.Spacing();

        // Import button with better label
        if (ImGuiUtils.IconButton(FontAwesomeHelper.IconImport, "Import"))
            MigrationModal.Open(ImGui.GetClipboardText());
    }
}
