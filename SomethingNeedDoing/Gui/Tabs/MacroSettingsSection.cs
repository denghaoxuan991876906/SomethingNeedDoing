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
    private DependencyType _dependencyType = DependencyType.Local;
    private readonly List<string> _disableablePluginNames = [.. disableablePlugins.Select(p => p.InternalName)];

    public void Draw(ConfigMacro? selectedMacro)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Header, new Vector4(0.2f, 0.2f, 0.3f, 0.7f))
            .Push(ImGuiCol.HeaderHovered, new Vector4(0.25f, 0.25f, 0.35f, 0.8f));

        if (ImGui.CollapsingHeader("宏设置", ImGuiTreeNodeFlags.DefaultOpen))
        {
            using var child = ImRaii.Child("SettingsChild", new(-1, ImGui.GetContentRegionAvail().Y), false);
            if (!child) return;

            if (selectedMacro != null)
            {
                ImGui.Spacing();

                if (ImGui.CollapsingHeader("常规信息", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Spacing();

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("作者:");
                    ImGui.SameLine(100);

                    var author = selectedMacro.Metadata.Author ?? string.Empty;
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    if (ImGui.InputText("##Author", ref author, 100))
                    {
                        selectedMacro.Metadata.Author = author;
                        C.Save();
                    }

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("版本:");
                    ImGui.SameLine(100);

                    var version = selectedMacro.Metadata.Version ?? string.Empty;
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    if (ImGui.InputText("##Version", ref version, 50))
                    {
                        selectedMacro.Metadata.Version = version;
                        C.Save();
                    }

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("描述:");

                    var description = selectedMacro.Metadata.Description ?? string.Empty;
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    if (ImGui.InputTextMultiline("##Description", ref description, 1000, new Vector2(-1, 100)))
                    {
                        selectedMacro.Metadata.Description = description;
                        C.Save();
                    }

                    if (ImGui.Button("将元数据写入内容"))
                    {
                        if (metadataParser.WriteMetadata(selectedMacro))
                            Svc.Log.Debug($"已写入宏 {selectedMacro.Name} 的元数据");
                        else
                            Svc.Log.Error($"写入宏 {selectedMacro.Name} 的元数据失败");
                    }
                    ImGuiEx.Tooltip("将当前元数据（作者、版本、描述、依赖、触发器等）写入宏内容。如果元数据已存在，将更新。");

                    ImGui.SameLine();

                    if (ImGui.Button("从内容读取元数据"))
                    {
                        selectedMacro.Metadata = metadataParser.ParseMetadata(selectedMacro.Content);
                        C.Save();
                    }
                    ImGuiEx.Tooltip("从宏内容中读取元数据（作者、版本、描述、依赖、触发器等）并更新设置。");
                }

                ImGui.Spacing();

                if (ImGui.CollapsingHeader("Git设置", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Spacing();

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("仓库URL:");
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
                        if (ImGui.Checkbox("自动更新", ref autoUpdate))
                        {
                            selectedMacro.GitInfo.AutoUpdate = autoUpdate;
                            C.Save();
                        }

                        var group = new ImGuiEx.EzButtonGroup();
                        group.AddIconWithText(FontAwesomeIcon.History, "版本历史", () => versionHistoryModal.Open(selectedMacro));
                        group.AddIconWithText(FontAwesomeIcon.Sync, "重置Git信息",
                            () => { selectedMacro.GitInfo = new GitInfo(); C.Save(); }, "清除所有Git信息，将此宏恢复为标准本地宏。",
                            new() { ButtonColor = EzColor.Red });
                        group.Draw();
                    }
                }

                ImGui.Spacing();

                if (selectedMacro.Type is MacroType.Native)
                {
                    if (ImGui.CollapsingHeader("制作设置", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.Spacing();

                        var craftingLoop = selectedMacro.Metadata.CraftingLoop;
                        if (ImGui.Checkbox("启用制作循环", ref craftingLoop))
                        {
                            selectedMacro.Metadata.CraftingLoop = craftingLoop;
                            C.Save();
                        }

                        if (craftingLoop)
                        {
                            ImGui.Indent(20);

                            var loopCount = selectedMacro.Metadata.CraftLoopCount;
                            ImGui.SetNextItemWidth(100);
                            if (ImGui.InputInt("循环次数", ref loopCount))
                            {
                                if (loopCount < -1)
                                    loopCount = -1;

                                selectedMacro.Metadata.CraftLoopCount = loopCount;
                                C.Save();
                            }

                            ImGui.SameLine();
                            ImGui.TextColored(ImGuiColors.DalamudGrey, "(-1 = 无限)");

                            ImGui.Unindent(20);
                        }
                    }
                }

                ImGui.Spacing();

                if (ImGui.CollapsingHeader("触发事件", ImGuiTreeNodeFlags.DefaultOpen))
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

                        ImGui.Text("插件事件配置");
                        ImGui.Spacing();

                        var addonConfig = selectedMacro.Metadata.AddonEventConfig ?? new AddonEventConfig();
                        var addonName = addonConfig.AddonName;
                        var eventType = addonConfig.EventType;

                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("插件名称:");
                        ImGui.SameLine(100);

                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        if (ImGui.InputText("##AddonName", ref addonName, 100))
                        {
                            addonConfig.AddonName = addonName;
                            selectedMacro.Metadata.AddonEventConfig = addonConfig;
                            C.Save();
                        }

                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("事件类型:");
                        ImGui.SameLine(100);

                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        if (ImGuiEx.EnumCombo("##EventType", ref eventType))
                        {
                            addonConfig.EventType = eventType;
                            selectedMacro.Metadata.AddonEventConfig = addonConfig;
                            C.Save();
                        }

                        if (ImGui.Button("清除插件事件配置"))
                        {
                            selectedMacro.Metadata.AddonEventConfig = null;
                            C.Save();
                        }
                    }
                }

                ImGui.Spacing();

                if (ImGui.CollapsingHeader("插件依赖", ImGuiTreeNodeFlags.DefaultOpen))
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
                        ImGui.TextColored(ImGuiColors.DalamudGrey, "未配置插件依赖");
                    else
                    {
                        foreach (var plugin in selectedMacro.Metadata.PluginDependecies)
                        {
                            using var __ = ImRaii.PushId(plugin);

                            ImGui.AlignTextToFramePadding();
                            ImGui.Text(plugin);
                            ImGui.SameLine(ImGui.GetContentRegionAvail().X - 30);

                            if (ImGuiUtils.IconButton(FontAwesomeIcon.Trash, "移除依赖"))
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

                if (ImGui.CollapsingHeader("要禁用的插件", ImGuiTreeNodeFlags.DefaultOpen))
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
                        ImGui.TextColored(ImGuiColors.DalamudGrey, "未配置要禁用的插件");
                    else
                    {
                        foreach (var plugin in selectedMacro.Metadata.PluginsToDisable)
                        {
                            using var __ = ImRaii.PushId(plugin);

                            ImGui.AlignTextToFramePadding();
                            ImGui.Text(plugin);
                            ImGui.SameLine(ImGui.GetContentRegionAvail().X - 30);

                            if (ImGuiUtils.IconButton(FontAwesomeIcon.Trash, "移除插件"))
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

                if (ImGui.CollapsingHeader("宏依赖", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Spacing();

                    if (selectedMacro.Metadata.Dependencies.Count == 0)
                        ImGui.TextColored(ImGuiColors.DalamudGrey, "未配置宏依赖");
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
                            if (ImGuiUtils.IconButton(FontAwesomeIcon.Trash, "移除依赖"))
                            {
                                selectedMacro.Metadata.Dependencies.RemoveAt(i--);
                                C.Save();
                            }
                        }
                    }

                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.Text("添加新依赖");
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
                        ImGuiEx.Tooltip("输入一个GitHub URL（类似：https://github.com/owner/repo 或 https://github.com/owner/repo/blob/branch/path）");

                        if (ImGui.Button("添加依赖"))
                        {
                            if (!string.IsNullOrWhiteSpace(_gitUrl))
                            {
                                selectedMacro.Metadata.Dependencies.Add(dependencyFactory.CreateDependency(_gitUrl));
                                C.Save();
                                _gitUrl = string.Empty;
                            }
                        }
                    }
                }
            }
            else
                ImGui.TextColored(ImGuiColors.DalamudGrey, "选择一个宏以查看和编辑其设置");
        }
    }
}
