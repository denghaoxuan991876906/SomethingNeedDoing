using Dalamud.Game.Addon.Lifecycle;
using SomethingNeedDoing.Core.Interfaces;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Parser for macro metadata using YAML format.
/// </summary>
public class MetadataParser
{
    public static readonly Regex MetadataBlockRegex = new(
        @"(?:--\[=====\[|/\*).*?\[\[SND\s*Metadata\]\](.*?)\[\[End\s*Metadata\]\].*?(?:\]=====\]|\*/)",
        RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    /// <summary>
    /// Parses metadata from macro content.
    /// </summary>
    /// <param name="content">The macro content to parse.</param>
    /// <returns>The parsed metadata.</returns>
    public MacroMetadata ParseMetadata(string content)
    {
        var match = MetadataBlockRegex.Match(content);
        if (!match.Success) return new MacroMetadata();

        var metadataContent = match.Groups[1].Value.Trim();
        if (string.IsNullOrEmpty(metadataContent)) return new MacroMetadata();

        var metadata = new MacroMetadata();

        try
        {
            if (_deserializer.Deserialize<Dictionary<string, object>>(metadataContent) is not { } yaml) return metadata;

            if (yaml.TryGetValue("author", out var author))
                metadata.Author = author?.ToString() ?? string.Empty;

            if (yaml.TryGetValue("version", out var version))
                metadata.Version = version?.ToString() ?? "1.0.0";

            if (yaml.TryGetValue("description", out var description))
                metadata.Description = description?.ToString() ?? string.Empty;

            if (yaml.TryGetValue("triggers", out var triggers))
            {
                if (triggers is List<object> triggerList)
                {
                    metadata.TriggerEvents = [.. triggerList
                        .Select(t => t.ToString())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .Select(t => Enum.TryParse<TriggerEvent>(t, true, out var trigger) ? trigger : TriggerEvent.None)
                        .Where(t => t != TriggerEvent.None)];
                }
            }

            if (yaml.TryGetValue("plugin_dependencies", out var pluginDeps))
                if (pluginDeps is List<object> pluginList)
                    metadata.PluginDependecies = [.. pluginList.Select(p => p.ToString() ?? string.Empty).Where(p => !string.IsNullOrEmpty(p))];

            if (yaml.TryGetValue("plugins_to_disable", out var pluginsToDisable))
                if (pluginsToDisable is List<object> disableList)
                    metadata.PluginsToDisable = [.. disableList.Select(p => p.ToString() ?? string.Empty).Where(p => !string.IsNullOrEmpty(p))];

            if (yaml.TryGetValue("crafting_loop", out var craftingLoop))
                metadata.CraftingLoop = craftingLoop?.ToString()?.ToLower() == "true";

            if (yaml.TryGetValue("craft_loop_count", out var craftLoopCount))
                if (int.TryParse(craftLoopCount?.ToString(), out var count))
                    metadata.CraftLoopCount = count;

            if (yaml.TryGetValue("addon_event", out var addonEvent) && addonEvent is Dictionary<object, object> addonDict)
            {
                metadata.AddonEventConfig = new AddonEventConfig();

                if (addonDict.TryGetValue("addon_name", out var addonName))
                    metadata.AddonEventConfig.AddonName = addonName?.ToString() ?? string.Empty;

                if (addonDict.TryGetValue("event_type", out var eventType))
                    if (Enum.TryParse<AddonEvent>(eventType?.ToString(), true, out var parsedEventType))
                        metadata.AddonEventConfig.EventType = parsedEventType;
            }

            if (yaml.TryGetValue("dependencies", out var dependencies))
                metadata.Dependencies = ParseDependencies(dependencies);

            if (yaml.TryGetValue("configs", out var configs) && configs is Dictionary<object, object> configDict)
                metadata.Configs = ParseConfigs(configDict);
        }
        catch (Exception ex)
        {
            FrameworkLogger.Error(ex, "Failed to parse metadata YAML");
        }

        return metadata;
    }

    /// <summary>
    /// Writes metadata to macro content.
    /// </summary>
    /// <param name="macro">The macro to write metadata to.</param>
    /// <param name="onContentUpdated">Optional callback to call when content is updated.</param>
    /// <returns>True if the metadata was written successfully.</returns>
    public bool WriteMetadata(ConfigMacro macro, Action? onContentUpdated = null)
    {
        if (macro == null) return false;

        var metadataDict = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(macro.Metadata.Author))
            metadataDict["author"] = macro.Metadata.Author;

        if (!string.IsNullOrEmpty(macro.Metadata.Version))
            metadataDict["version"] = macro.Metadata.Version;

        if (!string.IsNullOrEmpty(macro.Metadata.Description))
            metadataDict["description"] = macro.Metadata.Description;

        if (macro.Metadata.TriggerEvents.Any())
            metadataDict["triggers"] = macro.Metadata.TriggerEvents.Select(t => t.ToString().ToLower()).ToList();

        if (macro.Metadata.PluginDependecies.Any())
            metadataDict["plugin_dependencies"] = macro.Metadata.PluginDependecies.ToList();

        if (macro.Metadata.PluginsToDisable.Any())
            metadataDict["plugins_to_disable"] = macro.Metadata.PluginsToDisable.ToList();

        if (macro.Metadata.CraftingLoop)
            metadataDict["crafting_loop"] = true;

        if (macro.Metadata.CraftLoopCount > 0)
            metadataDict["craft_loop_count"] = macro.Metadata.CraftLoopCount;

        if (macro.Metadata.AddonEventConfig != null)
        {
            metadataDict["addon_event"] = new Dictionary<string, object>
            {
                ["addon_name"] = macro.Metadata.AddonEventConfig.AddonName,
                ["event_type"] = macro.Metadata.AddonEventConfig.EventType.ToString().ToLower()
            };
        }

        if (macro.Metadata.Dependencies.Any())
        {
            metadataDict["dependencies"] = macro.Metadata.Dependencies.Select(dep => new Dictionary<string, object>
            {
                ["source"] = dep.Source,
                ["name"] = dep.Name,
                ["type"] = dep switch
                {
                    GitDependency => "git",
                    LocalMacroDependency => "macro",
                    LocalDependency => "file",
                    _ => "unknown"
                }
            }).ToList();
        }

        if (macro.Metadata.Configs.Any())
        {
            metadataDict["configs"] = macro.Metadata.Configs.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    var configDict = new Dictionary<string, object>();

                    if (!string.IsNullOrEmpty(kvp.Value.DefaultValue?.ToString()))
                        configDict["default"] = kvp.Value.DefaultValue;

                    if (!string.IsNullOrEmpty(kvp.Value.Description))
                        configDict["description"] = kvp.Value.Description;

                    if (!string.IsNullOrEmpty(kvp.Value.Type))
                        configDict["type"] = kvp.Value.Type;

                    if (kvp.Value.MinValue != null)
                        configDict["min"] = kvp.Value.MinValue;

                    if (kvp.Value.MaxValue != null)
                        configDict["max"] = kvp.Value.MaxValue;

                    if (kvp.Value.Required)
                        configDict["required"] = kvp.Value.Required;

                    if (!string.IsNullOrEmpty(kvp.Value.ValidationPattern))
                        configDict["validation_pattern"] = kvp.Value.ValidationPattern;

                    if (!string.IsNullOrEmpty(kvp.Value.ValidationMessage))
                        configDict["validation_message"] = kvp.Value.ValidationMessage;

                    return configDict;
                }
            );
        }

        var yaml = _serializer.Serialize(metadataDict);
        var (startComment, endComment) = GetCommentSyntax(macro.Type);
        var metadataBlock = $"{startComment}\n[[SND Metadata]]\n{yaml}\n[[End Metadata]]\n{endComment}";

        var match = MetadataBlockRegex.Match(macro.Content);
        if (match.Success)
        {
            // Replace existing block
            var afterMetadata = macro.Content[(match.Index + match.Length)..]; // for ensuring there's a new line after the end
            macro.Content = macro.Content[..match.Index] + metadataBlock + (afterMetadata.StartsWith('\n') || afterMetadata.StartsWith('\r') ? "" : "\n") + afterMetadata;
        }
        else
            // Add new block
            macro.Content = metadataBlock + (macro.Content.StartsWith('\n') ? "" : "\n") + macro.Content;

        C.Save();
        onContentUpdated?.Invoke(); // there's probably a better way
        return true;
    }

    /// <summary>
    /// Gets the appropriate comment syntax for the given macro type.
    /// </summary>
    /// <param name="macroType">The type of macro.</param>
    /// <returns>Tuple of (startComment, endComment) strings.</returns>
    private static (string startComment, string endComment) GetCommentSyntax(MacroType macroType)
    {
        return macroType switch
        {
            MacroType.Lua => ("--[=====[", "--]=====]"),
            _ => ("", "")
        };
    }

    private List<IMacroDependency> ParseDependencies(object dependencies)
    {
        var result = new List<IMacroDependency>();

        if (dependencies is List<object> depList)
        {
            foreach (var dep in depList)
            {
                if (dep is Dictionary<object, object> depDict)
                {
                    var repo = depDict.TryGetValue("repo", out var r) ? r?.ToString() : string.Empty;
                    var path = depDict.TryGetValue("path", out var p) ? p?.ToString() : string.Empty;
                    var name = depDict.TryGetValue("name", out var n) ? n?.ToString() : string.Empty;
                    var branch = depDict.TryGetValue("branch", out var b) ? b?.ToString() : "main";
                    var source = depDict.TryGetValue("source", out var s) ? s?.ToString() : string.Empty;
                    var type = depDict.TryGetValue("type", out var t) ? t?.ToString() : string.Empty;

                    if (!string.IsNullOrEmpty(repo) && !string.IsNullOrEmpty(path))
                    {
                        result.Add(new GitDependency
                        {
                            GitInfo = new GitInfo
                            {
                                RepositoryUrl = repo,
                                FilePath = path,
                                Branch = branch ?? string.Empty
                            },
                            Source = repo,
                            Name = string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(path) : name
                        });
                    }
                    else if (!string.IsNullOrEmpty(source))
                    {
                        if (type == "macro" && Guid.TryParse(source, out _))
                        {
                            var macro = C.GetMacro(source);
                            if (macro != null)
                            {
                                result.Add(new LocalMacroDependency
                                {
                                    Source = source,
                                    Name = string.IsNullOrEmpty(name) ? macro.Name : name
                                });
                            }
                        }
                        else if (File.Exists(source.NormalizeFilePath()))
                        {
                            var normalizedPath = source.NormalizeFilePath();
                            result.Add(new LocalDependency
                            {
                                Source = normalizedPath,
                                Name = string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(normalizedPath) : name
                            });
                        }
                    }
                }
            }
        }

        return result;
    }

    private Dictionary<string, MacroConfigItem> ParseConfigs(Dictionary<object, object> configDict)
    {
        var result = new Dictionary<string, MacroConfigItem>();

        foreach (var kvp in configDict)
        {
            var configName = kvp.Key.ToString();
            if (string.IsNullOrEmpty(configName)) continue;

            if (kvp.Value is Dictionary<object, object> configData)
            {
                var configItem = new MacroConfigItem();

                if (configData.TryGetValue("default", out var defaultValue))
                    configItem.DefaultValue = defaultValue ?? string.Empty;

                if (configData.TryGetValue("description", out var description))
                    configItem.Description = description?.ToString() ?? string.Empty;

                if (configData.TryGetValue("type", out var type))
                    configItem.Type = type?.ToString() ?? "string";

                if (configData.TryGetValue("min", out var min))
                    configItem.MinValue = min;

                if (configData.TryGetValue("max", out var max))
                    configItem.MaxValue = max;

                if (configData.TryGetValue("required", out var required))
                    configItem.Required = required?.ToString()?.ToLower() == "true";

                if (configData.TryGetValue("validation_pattern", out var pattern))
                    configItem.ValidationPattern = pattern?.ToString();

                if (configData.TryGetValue("validation_message", out var message))
                    configItem.ValidationMessage = message?.ToString();

                configItem.Value = configItem.DefaultValue;
                result[configName] = configItem;
            }
        }

        return result;
    }
}
