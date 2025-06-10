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

        ImGuiUtils.Section("General Settings", () =>
        {
            var chatChannel = C.ChatType;
            if (ImGuiEx.EnumCombo("ChatType", ref chatChannel))
            {
                C.ChatType = chatChannel;
                C.Save();
            }

            var errorChannel = C.ErrorChatType;
            if (ImGuiEx.EnumCombo("ErrorChatType", ref errorChannel))
            {
                C.ErrorChatType = errorChannel;
                C.Save();
            }
        });

        ImGuiUtils.Section("Crafting", () =>
        {
            var craftSkip = C.CraftSkip;
            if (ImGui.Checkbox("Skip craft actions when not crafting.", ref craftSkip))
            {
                C.CraftSkip = craftSkip;
                C.Save();
            }

            var smartWait = C.SmartWait;
            if (ImGui.Checkbox("Wait for crafting actions to complete instead of using wait modifiers.", ref smartWait))
            {
                C.SmartWait = smartWait;
                C.Save();
            }

            var qualitySkip = C.QualitySkip;
            if (ImGui.Checkbox("Skip quality actions at max quality", ref qualitySkip))
            {
                C.QualitySkip = qualitySkip;
                C.Save();
            }

            var loopTotal = C.LoopTotal;
            if (ImGui.Checkbox("Count the /loop number as the total iterations, rather than the amount to loop.", ref loopTotal))
            {
                C.LoopTotal = loopTotal;
                C.Save();
            }

            var loopEcho = C.LoopEcho;
            if (ImGui.Checkbox("Always echo /loop commands..", ref loopEcho))
            {
                C.LoopEcho = loopEcho;
                C.Save();
            }

            var useCraftLoopTemplate = C.UseCraftLoopTemplate;
            if (ImGui.Checkbox("Use the \"CraftLoop\" template", ref useCraftLoopTemplate))
            {
                C.UseCraftLoopTemplate = useCraftLoopTemplate;
                C.Save();
            }

            var craftLoopFromRecipeNote = C.CraftLoopFromRecipeNote;
            if (ImGui.Checkbox("Start crafting loops from the recipe note window.", ref craftLoopFromRecipeNote))
            {
                C.CraftLoopFromRecipeNote = craftLoopFromRecipeNote;
                C.Save();
            }

            var craftLoopMaxWait = C.CraftLoopMaxWait;
            if (ImGui.SliderInt("Maximum wait value for the \"CraftLoop\" maxwait modifier", ref craftLoopMaxWait, 0, 1000))
            {
                C.CraftLoopMaxWait = craftLoopMaxWait;
                C.Save();
            }

            var craftLoopEcho = C.CraftLoopEcho;
            if (ImGui.Checkbox("Always echo \"CraftLoop\" commands", ref craftLoopEcho))
            {
                C.CraftLoopEcho = craftLoopEcho;
                C.Save();
            }
        });

        ImGuiUtils.Section("Error Handling", () =>
        {
            var maxTimeoutRetries = C.MaxTimeoutRetries;
            if (ImGui.SliderInt("Max Timeout Retries", ref maxTimeoutRetries, 0, 10))
            {
                C.MaxTimeoutRetries = maxTimeoutRetries;
                C.Save();
            }

            var noisyErrors = C.NoisyErrors;
            if (ImGui.Checkbox("Noisy Errors", ref noisyErrors))
            {
                C.NoisyErrors = noisyErrors;
                C.Save();
            }

            if (noisyErrors)
            {
                var beepFrequency = C.BeepFrequency;
                if (ImGui.SliderInt("Beep Frequency", ref beepFrequency, 0, 1000))
                {
                    C.BeepFrequency = beepFrequency;
                    C.Save();
                }

                var beepDuration = C.BeepDuration;
                if (ImGui.SliderInt("Beep Duration", ref beepDuration, 0, 1000))
                {
                    C.BeepDuration = beepDuration;
                    C.Save();
                }

                var beepCount = C.BeepCount;
                if (ImGui.SliderInt("Beep Count", ref beepCount, 0, 10))
                {
                    C.BeepCount = beepCount;
                    C.Save();
                }
            }
        });

        ImGuiUtils.Section("Error Conditions", () =>
        {
            var stopMacroIfActionTimeout = C.StopMacroIfActionTimeout;
            if (ImGui.Checkbox("Stop macro if an action times out.", ref stopMacroIfActionTimeout))
            {
                C.StopMacroIfActionTimeout = stopMacroIfActionTimeout;
                C.Save();
            }

            var stopMacroIfItemNotFound = C.StopMacroIfItemNotFound;
            if (ImGui.Checkbox("Stop macro if an item is not found.", ref stopMacroIfItemNotFound))
            {
                C.StopMacroIfItemNotFound = stopMacroIfItemNotFound;
                C.Save();
            }

            var stopMacroIfCantUseItem = C.StopMacroIfCantUseItem;
            if (ImGui.Checkbox("Stop macro if an item can't be used.", ref stopMacroIfCantUseItem))
            {
                C.StopMacroIfCantUseItem = stopMacroIfCantUseItem;
                C.Save();
            }

            var stopMacroIfTargetNotFound = C.StopMacroIfTargetNotFound;
            if (ImGui.Checkbox("Stop macro if a target is not found.", ref stopMacroIfTargetNotFound))
            {
                C.StopMacroIfTargetNotFound = stopMacroIfTargetNotFound;
                C.Save();
            }

            var stopMacroIfAddonNotFound = C.StopMacroIfAddonNotFound;
            if (ImGui.Checkbox("Stop macro if an addon is not found.", ref stopMacroIfAddonNotFound))
            {
                C.StopMacroIfAddonNotFound = stopMacroIfAddonNotFound;
                C.Save();
            }
        });

        ImGuiUtils.Section("Lua Options", () =>
        {
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
        });

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
