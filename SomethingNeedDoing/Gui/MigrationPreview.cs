using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Newtonsoft.Json;
using SomethingNeedDoing.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SomethingNeedDoing.Gui;
public class MigrationPreviewWindow : Window
{
    private readonly WindowSystem _ws;
    private readonly string _oldConfigJson;
    private readonly Dictionary<string, (string OldValue, string NewValue, bool Selected)> changes = [];
    private readonly Dictionary<string, (ConfigMacro Macro, bool Selected)> newMacros = [];
    private readonly Dictionary<string, (ConfigMacro Macro, bool Selected)> removedMacros = [];
    private bool migrationValid = true;
    private string errorMessage = string.Empty;
    private bool selectAllNewMacros = true;
    private bool selectAllRemovedMacros = true;
    private bool selectAllChanges = true;

    public MigrationPreviewWindow(WindowSystem ws, string oldConfigJson) : base($"{FontAwesomeHelper.IconImport} Migration Preview", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings)
    {
        _ws = ws;
        _oldConfigJson = oldConfigJson;
        PreviewMigration();
    }
    public override void OnClose() => _ws.RemoveWindow(this);

    private void PreviewMigration()
    {
        try
        {
            var oldConfig = JsonConvert.DeserializeObject<dynamic>(_oldConfigJson);
            if (oldConfig == null)
            {
                migrationValid = false;
                errorMessage = "Failed to parse old configuration";
                return;
            }

            // Log the old config structure for debugging
            Svc.Log.Debug($"Old config type: {oldConfig.GetType().Name}");

            // Preview general settings
            AddChange("LockWindow", oldConfig.LockWindow?.ToString(), C.LockWindow.ToString());
            AddChange("DisableMonospaced", oldConfig.DisableMonospaced?.ToString(), C.DisableMonospaced.ToString());
            AddChange("ChatType", oldConfig.ChatType?.ToString(), C.ChatType.ToString());
            AddChange("ErrorChatType", oldConfig.ErrorChatType?.ToString(), C.ErrorChatType.ToString());

            // Preview macros
            var oldMacros = new HashSet<string>();
            if (oldConfig.RootFolder?.Children != null)
            {
                Svc.Log.Debug($"Found children in root folder");
                TraverseMacroNodes(oldConfig.RootFolder, "/", oldMacros);
            }
            else
            {
                Svc.Log.Warning("No macros found in old config");
            }

            Svc.Log.Debug($"Current config has {C.Macros.Count} macros");
            foreach (var currentMacro in C.Macros)
            {
                Svc.Log.Debug($"Checking current macro: {currentMacro.Name} in {currentMacro.FolderPath}");
                if (!oldMacros.Contains(currentMacro.Name))
                {
                    Svc.Log.Debug($"Adding {currentMacro.Name} to removed macros");
                    removedMacros[currentMacro.Name] = (currentMacro, true);
                }
            }

            // Log migration summary
            Svc.Log.Info($"Migration preview summary:");
            Svc.Log.Info($"- New macros: {newMacros.Count}");
            Svc.Log.Info($"- Removed macros: {removedMacros.Count}");
            Svc.Log.Info($"- Changed settings: {changes.Count}");

            // Preview other settings
            AddChange("CraftSkip", oldConfig.CraftSkip?.ToString(), C.CraftSkip.ToString());
            AddChange("SmartWait", oldConfig.SmartWait?.ToString(), C.SmartWait.ToString());
            // TODO: the rest

            migrationValid = true;
        }
        catch (Exception ex)
        {
            migrationValid = false;
            errorMessage = $"Error previewing migration: {ex.Message}";
            Svc.Log.Error(ex, "Failed to preview migration");
        }
    }

    private void TraverseMacroNodes(dynamic node, string currentPath, HashSet<string> oldMacros)
    {
        try
        {
            if (node == null) return;

            // If it has contents, it's a macro node and not a folder node
            if (node.Contents != null)
            {
                var name = node.Name?.ToString() ?? "Unknown";
                var language = node.Language?.ToString() ?? "0";
                var content = node.Contents.ToString();
                var craftingLoop = node.CraftingLoop ?? false;
                var craftLoopCount = node.CraftLoopCount ?? 0;
                var isPostProcess = node.IsPostProcess ?? false;

                Svc.Log.Debug($"Processing old macro: {name} (Type: {language}, Folder: {currentPath})");

                oldMacros.Add(name);
                var macro = new ConfigMacro
                {
                    Name = name,
                    Type = language == "1" ? MacroType.Lua : MacroType.Native,
                    Content = content,
                    FolderPath = currentPath,
                    Metadata = new MacroMetadata
                    {
                        LastModified = DateTime.Now,
                        CraftingLoop = craftingLoop,
                        CraftLoopCount = craftLoopCount,
                        TriggerEvents = isPostProcess ? [TriggerEvent.OnAutoRetainerCharacterPostProcess] : [],
                    }
                };
                newMacros[name] = (macro, true);
            }
            // Check if this is a folder node by looking for Children property
            else if (node.Children != null)
            {
                var folderName = node.Name?.ToString() ?? "Unknown";
                var newPath = Path.Combine(currentPath, folderName);

                foreach (dynamic child in node.Children)
                    TraverseMacroNodes(child, newPath, oldMacros);
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Error processing node in folder {currentPath}");
        }
    }

    private void AddChange(string key, string? oldValue, string newValue)
    {
        if (oldValue != newValue)
        {
            Svc.Log.Debug($"Adding change: {key} = {oldValue} -> {newValue}");
            changes[key] = (oldValue ?? "null", newValue, true);
        }
    }

    public override void Draw()
    {
        if (!migrationValid)
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), "Migration Preview Failed");
            ImGui.TextWrapped(errorMessage);
            if (ImGui.Button("Close"))
                IsOpen = false;
            return;
        }

        // General Settings Section
        if (ImGui.CollapsingHeader($"{FontAwesomeHelper.IconSettings} General Settings"))
        {
            if (ImGui.Checkbox("Select All Changes", ref selectAllChanges))
            {
                var keys = changes.Keys.ToList();
                foreach (var key in keys)
                {
                    var (oldValue, newValue, _) = changes[key];
                    changes[key] = (oldValue, newValue, selectAllChanges);
                }
            }

            foreach (var (key, (oldValue, newValue, selected)) in changes.Where(c => !c.Key.StartsWith("Macro")))
            {
                var newSelected = selected;
                if (ImGui.Checkbox($"##{key}", ref newSelected))
                {
                    changes[key] = (oldValue, newValue, newSelected);
                }
                ImGui.SameLine();
                ImGui.Text($"{key}:");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(1, 0, 0, 1), oldValue);
                ImGui.SameLine();
                ImGui.Text("→");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(0, 1, 0, 1), newValue);
            }
        }

        // New Macros Section
        if (ImGui.CollapsingHeader($"{FontAwesomeHelper.IconNew} New Macros ({newMacros.Count})"))
        {
            if (ImGui.Checkbox("Select All New Macros", ref selectAllNewMacros))
            {
                var keys = newMacros.Keys.ToList();
                foreach (var key in keys)
                {
                    var (macro, _) = newMacros[key];
                    newMacros[key] = (macro, selectAllNewMacros);
                }
            }

            foreach (var (name, (macro, selected)) in newMacros)
            {
                var newSelected = selected;
                if (ImGui.Checkbox($"##new{name}", ref newSelected))
                    newMacros[name] = (macro, newSelected);
                ImGui.SameLine();
                ImGui.Text($"{name} ({macro.Type})");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(0, 1, 0, 1), "New");
            }
        }

        // Removed Macros Section
        if (ImGui.CollapsingHeader($"{FontAwesomeHelper.IconDelete} Removed Macros ({removedMacros.Count})"))
        {
            if (ImGui.Checkbox("Select All Removed Macros", ref selectAllRemovedMacros))
            {
                var keys = removedMacros.Keys.ToList();
                foreach (var key in keys)
                {
                    var (macro, _) = removedMacros[key];
                    removedMacros[key] = (macro, selectAllRemovedMacros);
                }
            }

            foreach (var (name, (macro, selected)) in removedMacros)
            {
                var newSelected = selected;
                if (ImGui.Checkbox($"##removed{name}", ref newSelected))
                {
                    removedMacros[name] = (macro, newSelected);
                }
                ImGui.SameLine();
                ImGui.Text($"{name} ({macro.Type})");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Removed");
            }
        }

        ImGui.Separator();
        if (ImGui.Button($"{FontAwesomeHelper.IconPlay} Apply Selected Changes", new Vector2(180, 0)))
        {
            ApplySelectedChanges();
            IsOpen = false;
        }
        ImGui.SameLine();
        if (ImGui.Button($"{FontAwesomeHelper.IconClear} Cancel", new Vector2(100, 0)))
            IsOpen = false;
    }

    private void ApplySelectedChanges()
    {
        try
        {
            // General settings
            foreach (var (key, (oldValue, newValue, selected)) in changes.Where(c => c.Value.Selected))
            {
                if (C.GetType().GetProperty(key) is { } property)
                {
                    try
                    {
                        var convertedValue = property.PropertyType.IsEnum
                            ? Enum.Parse(property.PropertyType, newValue)
                            : Convert.ChangeType(newValue, property.PropertyType);
                        property.SetValue(C, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        Svc.Log.Error(ex, $"Failed to convert value for property {key}: {newValue} to type {property.PropertyType}");
                    }
                }
            }

            // Apply selected new macros
            foreach (var (_, (macro, selected)) in newMacros.Where(m => m.Value.Selected))
                C.Macros.Add(macro);

            // Remove selected macros
            foreach (var (name, (macro, selected)) in removedMacros.Where(m => m.Value.Selected))
                C.Macros.RemoveAll(m => m.Name == name);

            C.Save();
            Svc.Chat.Print("Selected changes applied successfully!");
        }
        catch (Exception ex)
        {
            Svc.Chat.PrintError($"Failed to apply selected changes: {ex.Message}");
        }
    }
}
