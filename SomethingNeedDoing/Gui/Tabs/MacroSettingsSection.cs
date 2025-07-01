using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Gui.Modals;

namespace SomethingNeedDoing.Gui.Tabs;

public class MacroSettingsSection(IMacroScheduler scheduler, DependencyFactory dependencyFactory, VersionHistoryModal versionHistoryModal, MetadataParser metadataParser, IEnumerable<IDisableable> disableablePlugins)
{
    private string _pluginDependency = string.Empty;
    private string _pluginToDisable = string.Empty;
    private string _gitUrl = string.Empty;
    private DependencyType _dependencyType = DependencyType.Local;
    private readonly List<string> _disableablePluginNames = [.. disableablePlugins.Select(p => p.InternalName)];

    public Action? OnContentUpdated { get; set; } // for refreshing after writing the metadata

    public void Draw(ConfigMacro? selectedMacro)
    {
        using var child = ImRaii.Child("SettingsChild", new(-1, ImGui.GetContentRegionAvail().Y), false);
        if (!child) return;

        if (selectedMacro != null)
        {
            DrawMacroConfig(selectedMacro);
            DrawGeneralInfo(selectedMacro);
            DrawGitInfo(selectedMacro);

            if (selectedMacro.Type is MacroType.Native)
                DrawCraftLoop(selectedMacro);

            DrawTriggers(selectedMacro);
            DrawPluginDependencies(selectedMacro);
            DrawPluginConflicts(selectedMacro);
            DrawDependencies(selectedMacro);
        }
        else
            ImGui.TextColored(ImGuiColors.DalamudGrey, "Select a macro to view and edit its settings");
    }

    private void DrawMacroConfig(ConfigMacro selectedMacro)
    {
        if (selectedMacro.Metadata.Configs.Count == 0) return;
        ImGuiUtils.Section("Macro Configuration", () =>
        {
            foreach (var kvp in selectedMacro.Metadata.Configs)
            {
                var configName = kvp.Key;
                var configValue = kvp.Value;

                using var _ = ImRaii.PushId(configName);

                ImGui.AlignTextToFramePadding();
                ImGuiEx.Text(ImGuiColors.DalamudOrange, configName);
                ImGui.SameLine();
                ImGuiEx.Text(ImGuiColors.DalamudGrey, configValue.Description);

                ImGui.SameLine(ImGui.GetContentRegionAvail().X - 80);
                if (ImGui.Button("Reset", new Vector2(70, 0)))
                {
                    configValue.Value = configValue.DefaultValue;
                    C.Save();
                }
                ImGuiEx.Tooltip($"Reset to default value: {configValue.DefaultValue}");
                ImGui.Spacing();

                // Value editor based on type
                var valueChanged = false;
                switch (configValue.Type.ToLower())
                {
                    case "int":
                        var intValue = Convert.ToInt32(configValue.Value);
                        var intDefault = Convert.ToInt32(configValue.DefaultValue);
                        var intMin = configValue.MinValue != null ? Convert.ToInt32(configValue.MinValue) : int.MinValue;
                        var intMax = configValue.MaxValue != null ? Convert.ToInt32(configValue.MaxValue) : int.MaxValue;

                        ImGui.SetNextItemWidth(200);
                        if (ImGui.InputInt($"##{configName}Value", ref intValue))
                        {
                            if (intValue < intMin) intValue = intMin;
                            if (intValue > intMax) intValue = intMax;
                            configValue.Value = intValue;
                            valueChanged = true;
                        }

                        if (configValue.MinValue != null || configValue.MaxValue != null)
                        {
                            ImGui.SameLine();
                            ImGui.TextColored(ImGuiColors.DalamudGrey, $"Range: {intMin} - {intMax}");
                        }
                        break;

                    case "float":
                    case "double":
                        var floatValue = Convert.ToSingle(configValue.Value);
                        var floatDefault = Convert.ToSingle(configValue.DefaultValue);
                        var floatMin = configValue.MinValue != null ? Convert.ToSingle(configValue.MinValue) : float.MinValue;
                        var floatMax = configValue.MaxValue != null ? Convert.ToSingle(configValue.MaxValue) : float.MaxValue;

                        ImGui.SetNextItemWidth(200);
                        if (ImGui.InputFloat($"##{configName}Value", ref floatValue))
                        {
                            if (floatValue < floatMin) floatValue = floatMin;
                            if (floatValue > floatMax) floatValue = floatMax;
                            configValue.Value = floatValue;
                            valueChanged = true;
                        }

                        if (configValue.MinValue != null || configValue.MaxValue != null)
                        {
                            ImGui.SameLine();
                            ImGui.TextColored(ImGuiColors.DalamudGrey, $"Range: {floatMin:F2} - {floatMax:F2}");
                        }
                        break;

                    case "bool":
                    case "boolean":
                        var boolValue = Convert.ToBoolean(configValue.Value);
                        ImGui.SetNextItemWidth(200);
                        if (ImGui.Checkbox($"##{configName}Value", ref boolValue))
                        {
                            configValue.Value = boolValue;
                            valueChanged = true;
                        }
                        break;

                    case "string":
                    default:
                        var stringValue = configValue.Value.ToString() ?? string.Empty;
                        ImGui.SetNextItemWidth(300);

                        var isValid = true;
                        var validationMessage = string.Empty;
                        if (!string.IsNullOrEmpty(configValue.ValidationPattern))
                        {
                            try
                            {
                                var regex = new System.Text.RegularExpressions.Regex(configValue.ValidationPattern);
                                isValid = regex.IsMatch(stringValue);
                                if (!isValid)
                                    validationMessage = configValue.ValidationMessage ?? "Value does not match pattern";
                            }
                            catch (Exception ex)
                            {
                                isValid = false;
                                validationMessage = $"Invalid validation pattern: {ex.Message}";
                            }
                        }

                        using (ImRaii.PushColor(ImGuiCol.Text, isValid ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed, !string.IsNullOrEmpty(configValue.ValidationPattern)))
                        {
                            if (ImGui.InputText($"##{configName}Value", ref stringValue, 1000))
                            {
                                configValue.Value = stringValue;
                                valueChanged = true;
                            }
                        }

                        if (!string.IsNullOrEmpty(configValue.ValidationPattern))
                        {
                            ImGui.SameLine();
                            ImGuiEx.Icon(isValid ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed, isValid ? FontAwesomeIcon.Check : FontAwesomeIcon.ExclamationTriangle);

                            if (!isValid && !string.IsNullOrEmpty(validationMessage))
                                ImGuiEx.Tooltip(validationMessage);
                            else if (isValid)
                                ImGuiEx.Tooltip("Value matches validation pattern");
                        }
                        break;
                }

                if (valueChanged)
                    C.Save();

                if (configValue.Required)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(ImGuiColors.DalamudRed, "*");
                    ImGuiEx.Tooltip("This configuration is required");
                }

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
            }
        });
    }

    private void DrawGeneralInfo(ConfigMacro selectedMacro)
    {
        ImGuiUtils.Section("General Information", () =>
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Author:");
            ImGui.SameLine(100);

            var author = selectedMacro.Metadata.Author ?? string.Empty;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputText("##Author", ref author, 100))
            {
                selectedMacro.Metadata.Author = author;
                C.Save();
            }

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Version:");
            ImGui.SameLine(100);

            var version = selectedMacro.Metadata.Version ?? string.Empty;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputText("##Version", ref version, 50))
            {
                selectedMacro.Metadata.Version = version;
                C.Save();
            }

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Description:");

            var description = selectedMacro.Metadata.Description ?? string.Empty;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputTextMultiline("##Description", ref description, 1000, new Vector2(-1, 100)))
            {
                selectedMacro.Metadata.Description = description;
                C.Save();
            }

            if (ImGui.Button("Write Metadata to Content"))
            {
                if (metadataParser.WriteMetadata(selectedMacro, OnContentUpdated))
                    Svc.Log.Debug($"Wrote metadata to macro {selectedMacro.Name}");
                else
                    Svc.Log.Error($"Failed to write metadata to macro {selectedMacro.Name}");
            }
            ImGuiEx.Tooltip("Writes the current metadata (author, version, description, dependencies, triggers) to the macro content. If metadata already exists, it will be updated.");

            ImGui.SameLine();

            if (ImGui.Button("Read Metadata from Content"))
            {
                selectedMacro.Metadata = metadataParser.ParseMetadata(selectedMacro.Content);
                C.Save();
            }
            ImGuiEx.Tooltip("Reads metadata (author, version, description, dependencies, triggers) from the macro content and updates the settings.");
        });
    }

    private void DrawGitInfo(ConfigMacro selectedMacro)
    {
        ImGuiUtils.Section("Git Information", () =>
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Repository URL:");
            ImGui.SameLine(100);

            var repoUrl = selectedMacro.GitInfo.RepositoryUrl;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputText("##RepoUrl", ref repoUrl, 500))
            {
                selectedMacro.GitInfo.RepositoryUrl = repoUrl;
                C.Save();
            }

            if (selectedMacro.IsGitMacro)
            {
                ImGui.AlignTextToFramePadding();
                var autoUpdate = selectedMacro.GitInfo.AutoUpdate;
                if (ImGui.Checkbox("Auto Update", ref autoUpdate))
                {
                    selectedMacro.GitInfo.AutoUpdate = autoUpdate;
                    C.Save();
                }

                var group = new ImGuiEx.EzButtonGroup();
                group.AddIconWithText(FontAwesomeIcon.History, "Version History", () => versionHistoryModal.Open(selectedMacro));
                group.AddIconWithText(FontAwesomeIcon.Sync, "Reset Git Info",
                    () => { selectedMacro.GitInfo = new GitInfo(); C.Save(); }, "Wipes all git information and reverts this macro back to a standard local macro.",
                    new() { ButtonColor = EzColor.Red });
                group.Draw();
            }
        });
    }

    private void DrawCraftLoop(ConfigMacro selectedMacro)
    {
        ImGuiUtils.Section("CraftLoop Settings", () =>
        {
            var craftingLoop = selectedMacro.Metadata.CraftingLoop;
            if (ImGui.Checkbox("Enable Crafting Loop", ref craftingLoop))
            {
                selectedMacro.Metadata.CraftingLoop = craftingLoop;
                C.Save();
            }

            if (craftingLoop)
            {
                ImGui.Indent(20);

                var loopCount = selectedMacro.Metadata.CraftLoopCount;
                ImGui.SetNextItemWidth(100);
                if (ImGui.InputInt("Loop Count", ref loopCount))
                {
                    if (loopCount < -1)
                        loopCount = -1;

                    selectedMacro.Metadata.CraftLoopCount = loopCount;
                    C.Save();
                }

                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey, "(-1 = infinite)");

                ImGui.Unindent(20);
            }
        });
    }

    private void DrawTriggers(ConfigMacro selectedMacro)
    {
        ImGuiUtils.Section("Trigger Events", () =>
        {
            var events = new List<TriggerEvent>(selectedMacro.Metadata.TriggerEvents);
            if (ImGuiUtils.EnumCheckboxes(ref events, [TriggerEvent.None]))
                selectedMacro.SetTriggerEvents(scheduler, events);

            // Show addon event configuration only when OnAddonEvent is selected
            if (events.Contains(TriggerEvent.OnAddonEvent))
            {
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Text("Addon Event Configuration");
                ImGui.Spacing();

                var addonConfig = selectedMacro.Metadata.AddonEventConfig ?? new AddonEventConfig();
                var addonName = addonConfig.AddonName;
                var eventType = addonConfig.EventType;

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Addon Name:");
                ImGui.SameLine(100);

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                if (ImGui.InputText("##AddonName", ref addonName, 100))
                {
                    addonConfig.AddonName = addonName;
                    selectedMacro.Metadata.AddonEventConfig = addonConfig;
                    C.Save();
                }

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Event Type:");
                ImGui.SameLine(100);

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                if (ImGuiEx.EnumCombo("##EventType", ref eventType))
                {
                    addonConfig.EventType = eventType;
                    selectedMacro.Metadata.AddonEventConfig = addonConfig;
                    C.Save();
                }

                if (ImGui.Button("Clear Addon Event Config"))
                {
                    selectedMacro.Metadata.AddonEventConfig = null;
                    C.Save();
                }
            }
        });
    }

    private void DrawPluginDependencies(ConfigMacro selectedMacro)
    {
        ImGuiUtils.Section("Plugin Dependencies", () =>
        {
            var installedPlugins = Svc.PluginInterface.InstalledPlugins
                .Where(p => p.IsLoaded)
                .Select(p => p.InternalName)
                .OrderBy(p => p)
                .ToList();

            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGuiEx.Combo("##PluginSelector", ref _pluginDependency, installedPlugins))
            {
                if (!selectedMacro.Metadata.PluginDependecies.Contains(_pluginDependency))
                {
                    var newDeps = selectedMacro.Metadata.PluginDependecies.ToList();
                    newDeps.Add(_pluginDependency);
                    selectedMacro.Metadata.PluginDependecies = [.. newDeps];
                    C.Save();
                }
            }

            ImGui.Spacing();

            if (selectedMacro.Metadata.PluginDependecies.Length == 0)
                ImGui.TextColored(ImGuiColors.DalamudGrey, "No plugin dependencies configured");
            else
            {
                foreach (var plugin in selectedMacro.Metadata.PluginDependecies)
                {
                    using var __ = ImRaii.PushId(plugin);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(plugin);
                    ImGui.SameLine(ImGui.GetContentRegionAvail().X - 30);

                    if (ImGuiUtils.IconButton(FontAwesomeIcon.Trash, "Remove dependency"))
                    {
                        var newDeps = selectedMacro.Metadata.PluginDependecies.ToList();
                        newDeps.Remove(plugin);
                        selectedMacro.Metadata.PluginDependecies = [.. newDeps];
                        C.Save();
                    }
                }
            }
        });
    }

    private void DrawPluginConflicts(ConfigMacro selectedMacro)
    {
        ImGuiUtils.Section("Plugin Conflicts", () =>
        {
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGuiEx.Combo("##DisableablePluginSelector", ref _pluginToDisable, _disableablePluginNames))
            {
                if (!selectedMacro.Metadata.PluginsToDisable.Contains(_pluginToDisable))
                {
                    var newDeps = selectedMacro.Metadata.PluginsToDisable.ToList();
                    newDeps.Add(_pluginToDisable);
                    selectedMacro.Metadata.PluginsToDisable = [.. newDeps];
                    C.Save();
                }
            }

            ImGui.Spacing();

            if (selectedMacro.Metadata.PluginsToDisable.Length == 0)
                ImGui.TextColored(ImGuiColors.DalamudGrey, "No plugins configured to disable");
            else
            {
                foreach (var plugin in selectedMacro.Metadata.PluginsToDisable)
                {
                    using var __ = ImRaii.PushId(plugin);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(plugin);
                    ImGui.SameLine(ImGui.GetContentRegionAvail().X - 30);

                    if (ImGuiUtils.IconButton(FontAwesomeIcon.Trash, "Remove plugin"))
                    {
                        var newDeps = selectedMacro.Metadata.PluginsToDisable.ToList();
                        newDeps.Remove(plugin);
                        selectedMacro.Metadata.PluginsToDisable = [.. newDeps];
                        C.Save();
                    }
                }
            }
        });
    }

    private void DrawDependencies(ConfigMacro selectedMacro)
    {
        ImGuiUtils.Section("Macro Dependencies", () =>
        {
            if (selectedMacro.Metadata.Dependencies.Count == 0)
                ImGui.TextColored(ImGuiColors.DalamudGrey, "No macro dependencies configured");
            else
            {
                for (var i = 0; i < selectedMacro.Metadata.Dependencies.Count; i++)
                {
                    var dependency = selectedMacro.Metadata.Dependencies[i];
                    using var __ = ImRaii.PushId(i);

                    var macroId = dependency.Id;
                    var isGit = macroId.StartsWith("git://");
                    var displayName = $"[{macroId[..7]}] {dependency.Name}";

                    ImGuiEx.IconWithText(isGit ? ImGuiColors.ParsedBlue : ImGuiColors.DalamudWhite, isGit ? FontAwesomeIcon.CloudDownloadAlt : FontAwesomeIcon.FileAlt, displayName);

                    ImGui.SameLine(ImGui.GetContentRegionAvail().X - 30);
                    if (ImGuiUtils.IconButton(FontAwesomeIcon.Trash, "Remove dependency"))
                    {
                        selectedMacro.Metadata.Dependencies.RemoveAt(i--);
                        C.Save();
                    }
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.Text("Add New Dependency");
            ImGui.Spacing();

            ImGuiEx.EnumRadio(ref _dependencyType, true);

            ImGui.Spacing();

            if (_dependencyType == DependencyType.Local)
            {
                var localMacros = C.Macros.Where(m => m.Id != selectedMacro.Id).OrderBy(m => m.Name).ToList();
                var selectedMacroId = string.Empty;
                var macroNames = localMacros.ToDictionary(m => m.Id, m => $"{m.Name} [{m.FolderPath}]");

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                if (ImGuiEx.Combo("##LocalMacroSelector", ref selectedMacroId, localMacros.Select(m => m.Id), names: macroNames))
                {
                    selectedMacro.Metadata.Dependencies.Add(dependencyFactory.CreateDependency(selectedMacroId));
                    C.Save();
                }
            }
            else
            {
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                ImGui.InputText("##GitUrl", ref _gitUrl, 1000);
                ImGuiEx.Tooltip("Enter a GitHub URL (e.g., https://github.com/owner/repo or https://github.com/owner/repo/blob/branch/path)");

                if (ImGui.Button("Add Dependency"))
                {
                    if (!string.IsNullOrWhiteSpace(_gitUrl))
                    {
                        selectedMacro.Metadata.Dependencies.Add(dependencyFactory.CreateDependency(_gitUrl));
                        C.Save();
                        _gitUrl = string.Empty;
                    }
                }
            }
        });
    }
}
