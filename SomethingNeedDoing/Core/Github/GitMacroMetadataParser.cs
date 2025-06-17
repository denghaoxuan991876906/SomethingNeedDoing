using SomethingNeedDoing.Core.Interfaces;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace SomethingNeedDoing.Core.Github;

/// <summary>
/// Parser for Git macro metadata.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GitMacroMetadataParser"/> class.
/// </remarks>
/// <param name="gitService">The Git service.</param>
public class GitMacroMetadataParser(IGitService gitService)
{
    private static readonly Regex MetadataBlockRegex = new(
        @"^--\[\[SND\s*Metadata\s*\]\]\s*\n(.*?)\n--\[\[End\s*Metadata\s*\]\]",
        RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private readonly IGitService _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));

    /// <summary>
    /// Writes metadata to macro content.
    /// </summary>
    /// <param name="macro">The macro to write metadata to.</param>
    /// <returns>True if the metadata was written successfully.</returns>
    public bool WriteMetadata(ConfigMacro macro)
    {
        if (macro == null)
            return false;

        var metadataBlock = new StringBuilder();
        metadataBlock.AppendLine("--[[SND Metadata]]");

        if (!string.IsNullOrEmpty(macro.Metadata.Author))
            metadataBlock.AppendLine($"author: {macro.Metadata.Author}");

        if (!string.IsNullOrEmpty(macro.Metadata.Version))
            metadataBlock.AppendLine($"version: {macro.Metadata.Version}");

        if (!string.IsNullOrEmpty(macro.Metadata.Description))
            metadataBlock.AppendLine($"description: {macro.Metadata.Description}");

        if (macro.Metadata.Dependencies.Any())
        {
            metadataBlock.AppendLine("dependencies:");
            foreach (var dep in macro.Metadata.Dependencies)
            {
                if (dep is GitDependency gitDep)
                {
                    metadataBlock.AppendLine($"  - repo: {gitDep.GitInfo.RepositoryUrl}");
                    metadataBlock.AppendLine($"    path: {gitDep.GitInfo.FilePath}");
                    if (gitDep.GitInfo.Branch != "main")
                        metadataBlock.AppendLine($"    branch: {gitDep.GitInfo.Branch}");
                    if (gitDep.Name != Path.GetFileNameWithoutExtension(gitDep.GitInfo.FilePath))
                        metadataBlock.AppendLine($"    name: {gitDep.Name}");
                }
            }
        }

        if (macro.Metadata.TriggerEvents.Any())
            metadataBlock.AppendLine($"triggers: {string.Join(", ", macro.Metadata.TriggerEvents)}");

        if (macro.Metadata.PluginDependecies.Any())
            metadataBlock.AppendLine($"plugin_dependencies: {string.Join(", ", macro.Metadata.PluginDependecies)}");

        if (macro.Metadata.PluginsToDisable.Any())
            metadataBlock.AppendLine($"plugins_to_disable: {string.Join(", ", macro.Metadata.PluginsToDisable)}");

        if (macro.Metadata.CraftingLoop)
        {
            metadataBlock.AppendLine($"crafting_loop: true");
            metadataBlock.AppendLine($"craft_loop_count: {macro.Metadata.CraftLoopCount}");
        }

        if (macro.Metadata.AddonEventConfig != null)
        {
            metadataBlock.AppendLine($"addon_event:");
            metadataBlock.AppendLine($"  addon_name: {macro.Metadata.AddonEventConfig.AddonName}");
            metadataBlock.AppendLine($"  event_type: {macro.Metadata.AddonEventConfig.EventType}");
        }

        metadataBlock.AppendLine("--[[End Metadata]]");

        var match = MetadataBlockRegex.Match(macro.Content);
        if (match.Success)
            // When replacing, trim any extra newlines after the existing block
            macro.Content = macro.Content[..match.Index] + metadataBlock.ToString() + macro.Content[(match.Index + match.Length)..].TrimStart('\r', '\n');
        else
            // When adding new, ensure there's a newline between metadata and content
            macro.Content = metadataBlock.ToString() + (macro.Content.StartsWith("\n") ? "" : "\n") + macro.Content;

        C.Save();
        return true;
    }

    /// <summary>
    /// Parses metadata from macro content.
    /// </summary>
    /// <param name="content">The macro content to parse.</param>
    /// <returns>The parsed metadata.</returns>
    public MacroMetadata ParseMetadata(string content)
    {
        /* Metadata looks like
         * --[[SND Metadata]]
         * author: croizat
         * version: 1.0.0
         * --[[End Metadata]]
         */
        var match = MetadataBlockRegex.Match(content);
        if (!match.Success) return new MacroMetadata();

        var metadataContent = match.Groups[1].Value;
        var metadata = new MacroMetadata();

        var lines = metadataContent.Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("--"));

        foreach (var line in lines)
        {
            var parts = line.Split([':', '='], 2);
            if (parts.Length != 2) continue;

            var key = parts[0].Trim().ToLower();
            var value = parts[1].Trim();

            switch (key)
            {
                case "author":
                    metadata.Author = value;
                    break;
                case "version":
                    metadata.Version = value;
                    break;
                case "description":
                    metadata.Description = value;
                    break;
                case "dependencies":
                    ParseDependencies(value, metadata);
                    break;
                case "triggers":
                    ParseTriggers(value, metadata);
                    break;
                case "plugin_dependencies":
                    metadata.PluginDependecies = value.Split(',')
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToArray();
                    break;
                case "plugins_to_disable":
                    metadata.PluginsToDisable = value.Split(',')
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToArray();
                    break;
                case "crafting_loop":
                    metadata.CraftingLoop = value.ToLower() == "true";
                    break;
                case "craft_loop_count":
                    if (int.TryParse(value, out var count))
                        metadata.CraftLoopCount = count;
                    break;
                case "addon_event":
                    metadata.AddonEventConfig = new AddonEventConfig();
                    break;
                case "addon_name":
                    if (metadata.AddonEventConfig != null)
                        metadata.AddonEventConfig.AddonName = value;
                    break;
                case "event_type":
                    if (metadata.AddonEventConfig != null && Enum.TryParse<AddonEvent>(value, true, out var eventType))
                        metadata.AddonEventConfig.EventType = eventType;
                    break;
            }
        }

        return metadata;
    }

    private void ParseDependencies(string value, MacroMetadata metadata)
    {
        // TODO: better determining factors for JSON vs YAML
        try
        {
            if (value.Contains('{'))
                ParseJsonDependencies(value, metadata);
            else if (value.Contains('\n'))
                ParseYamlDependencies(value, metadata);
            else
                ParseSimpleDependencies(value, metadata);
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to parse dependencies");
        }
    }

    private void ParseJsonDependencies(string value, MacroMetadata metadata)
    {
        // Simple JSON-like parsing for dependencies
        var dependencies = new List<IMacroDependency>();
        var matches = Regex.Matches(value, @"{([^}]+)}");

        foreach (Match match in matches)
        {
            string? repo = null;
            string? path = null;
            var branch = "main";
            var name = string.Empty;

            var props = match.Groups[1].Value.Split(',');

            foreach (var prop in props)
            {
                var parts = prop.Split(':');
                if (parts.Length != 2) continue;

                var key = parts[0].Trim().Trim('"', '\'');
                var val = parts[1].Trim().Trim('"', '\'');

                switch (key.ToLower())
                {
                    case "repo":
                        repo = val;
                        break;
                    case "path":
                        path = val;
                        break;
                    case "branch":
                        branch = val;
                        break;
                    case "name":
                        name = val;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(repo) && !string.IsNullOrEmpty(path))
            {
                name = string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(path) : name;
                dependencies.Add(new GitDependency(_gitService, repo, branch, path, name));
            }
        }

        metadata.Dependencies = dependencies;
    }

    private void ParseYamlDependencies(string value, MacroMetadata metadata)
    {
        var dependencies = new List<IMacroDependency>();
        var lines = value.Split('\n');
        string? repo = null;
        string? path = null;
        var branch = "main";
        var name = string.Empty;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith('-'))
            {
                if (!string.IsNullOrEmpty(repo) && !string.IsNullOrEmpty(path))
                {
                    name = string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(path) : name;
                    dependencies.Add(new GitDependency(_gitService, repo, branch, path, name));
                }
                repo = null;
                path = null;
                branch = "main";
                name = string.Empty;
            }
            else
            {
                var parts = trimmed.Split(':');
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var val = parts[1].Trim();

                switch (key.ToLower())
                {
                    case "repo":
                        repo = val;
                        break;
                    case "path":
                        path = val;
                        break;
                    case "branch":
                        branch = val;
                        break;
                    case "name":
                        name = val;
                        break;
                }
            }
        }

        if (!string.IsNullOrEmpty(repo) && !string.IsNullOrEmpty(path))
        {
            name = string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(path) : name;
            dependencies.Add(new GitDependency(_gitService, repo, branch, path, name));
        }

        metadata.Dependencies = dependencies;
    }

    private void ParseSimpleDependencies(string value, MacroMetadata metadata)
    {
        var dependencies = value.Split(',')
            .Select(v => v.Trim())
            .Where(v => !string.IsNullOrEmpty(v))
            .Select(v =>
            {
                var name = Path.GetFileNameWithoutExtension(v);
                return new GitDependency(_gitService, v, "main", v, name);
            })
            .ToList<IMacroDependency>();

        metadata.Dependencies = dependencies;
    }

    private static void ParseTriggers(string value, MacroMetadata metadata)
    {
        try
        {
            var triggers = value.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .Select(t => Enum.Parse<TriggerEvent>(t, true))
                .Where(t => Enum.IsDefined(t))
                .ToList();

            metadata.TriggerEvents = triggers;
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to parse triggers");
        }
    }
}
