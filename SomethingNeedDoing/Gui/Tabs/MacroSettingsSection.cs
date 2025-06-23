using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Core.Github;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Gui.Modals;

namespace SomethingNeedDoing.Gui.Tabs;

public class MacroSettingsSection(IMacroScheduler scheduler, DependencyFactory dependencyFactory, VersionHistoryModal versionHistoryModal, GitMacroMetadataParser metadataParser, IEnumerable<IDisableable> disableablePlugins)
{
    private string _pluginDependency = string.Empty;
    private string _pluginToDisable = string.Empty;
    private string _gitUrl = string.Empty;
    private string _branch = "main";
    private string _path = string.Empty;
    private DependencyType _dependencyType = DependencyType.Local;
    private readonly List<string> _disableablePluginNames = [.. disableablePlugins.Select(p => p.InternalName)];

    public void Draw(ConfigMacro? selectedMacro)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Header, new Vector4(0.2f, 0.2f, 0.3f, 0.7f))
            .Push(ImGuiCol.HeaderHovered, new Vector4(0.25f, 0.25f, 0.35f, 0.8f));

        if (ImGui.CollapsingHeader("MACRO SETTINGS", ImGuiTreeNodeFlags.DefaultOpen))
        {
            using var child = ImRaii.Child("SettingsChild", new(-1, ImGui.GetContentRegionAvail().Y), false);
            if (!child) return;

            if (selectedMacro != null)
            {
                ImGui.Spacing();

                if (ImGui.CollapsingHeader("General Information", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Spacing();

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
                        if (metadataParser.WriteMetadata(selectedMacro))
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
                }

                ImGui.Spacing();

                if (ImGui.CollapsingHeader("Git Settings", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Spacing();

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
                }

                ImGui.Spacing();

                if (selectedMacro.Type is MacroType.Native)
                {
                    if (ImGui.CollapsingHeader("Crafting Settings", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.Spacing();

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
                    }
                }

                ImGui.Spacing();

                if (ImGui.CollapsingHeader("Trigger Events", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Spacing();

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
                }

                ImGui.Spacing();

                if (ImGui.CollapsingHeader("Plugin Dependencies", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Spacing();

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
                }

                ImGui.Spacing();

                if (ImGui.CollapsingHeader("Plugins to Disable", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Spacing();

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
                }

                ImGui.Spacing();

                if (ImGui.CollapsingHeader("Macro Dependencies", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Spacing();

                    if (selectedMacro.Metadata.Dependencies.Count == 0)
                        ImGui.TextColored(ImGuiColors.DalamudGrey, "No macro dependencies configured");
                    else
                    {
                        for (var i = 0; i < selectedMacro.Metadata.Dependencies.Count; i++)
                        {
                            var dependency = selectedMacro.Metadata.Dependencies[i];
                            using var __ = ImRaii.PushId(i);

                            ImGui.AlignTextToFramePadding();
                            ImGui.Text($"Dependency {i + 1}:");
                            ImGui.SameLine(100);

                            var macroId = dependency.Id;
                            var isGit = macroId.StartsWith("git://");
                            var displayName = isGit ? macroId[6..] : macroId;

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
                        var localMacros = C.Macros
                            .Where(m => m.Id != selectedMacro.Id)
                            .OrderBy(m => m.Name)
                            .ToList();

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
                        if (ImGui.InputText("##GitUrl", ref _gitUrl, 1000))
                        {
                            // Auto-detect branch and path from URL if it's a blob/raw URL
                            if (_gitUrl.Contains("/blob/") || _gitUrl.Contains("/raw/"))
                            {
                                var uri = new Uri(_gitUrl);
                                var pathParts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                                if (pathParts.Length >= 4)
                                {
                                    _branch = pathParts[3];
                                    _path = string.Join("/", pathParts.Skip(4));
                                }
                            }
                        }
                        ImGuiEx.Tooltip("Enter a GitHub URL (e.g., https://github.com/owner/repo or https://github.com/owner/repo/blob/branch/path)");

                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        if (ImGui.InputText("##Branch", ref _branch, 100))
                        {
                            // Validate branch name
                        }
                        ImGuiEx.Tooltip("Branch name (default: main)");

                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        if (ImGui.InputText("##Path", ref _path, 100))
                        {
                            // Validate path
                        }
                        ImGuiEx.Tooltip("Path to the file in the repository (optional)");

                        if (ImGui.Button("Add Dependency"))
                        {
                            if (!string.IsNullOrWhiteSpace(_gitUrl))
                            {
                                selectedMacro.Metadata.Dependencies.Add(dependencyFactory.CreateDependency(_gitUrl));
                                C.Save();
                                _gitUrl = string.Empty;
                                _branch = "main";
                                _path = string.Empty;
                            }
                        }
                    }
                }
            }
            else
                ImGui.TextColored(ImGuiColors.DalamudGrey, "Select a macro to view and edit its settings");
        }
    }
}
