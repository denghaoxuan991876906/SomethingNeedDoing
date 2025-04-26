using System.Text.RegularExpressions;
using ECommons.Logging;

namespace SomethingNeedDoing.Framework;

/// <summary>
/// Parser for Git macro metadata.
/// </summary>
public static class GitMacroMetadataParser
{
    private static readonly Regex MetadataBlockRegex = new(
        @"^--\[\[SND\s*Metadata\s*\]\]\s*\n(.*?)\n--\[\[End\s*Metadata\s*\]\]",
        RegexOptions.Singleline | RegexOptions.IgnoreCase);

    /// <summary>
    /// Parses metadata from macro content.
    /// </summary>
    /// <param name="content">The macro content to parse.</param>
    /// <returns>The parsed metadata.</returns>
    public static MacroMetadata ParseMetadata(string content)
    {
        var match = MetadataBlockRegex.Match(content);
        if (!match.Success) return new MacroMetadata();

        var metadataContent = match.Groups[1].Value;
        var metadata = new MacroMetadata();

        // Split into lines and process each line
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
                    // Add more metadata fields as needed
            }
        }

        return metadata;
    }

    private static void ParseDependencies(string value, MacroMetadata metadata)
    {
        try
        {
            if (value.Contains('{'))
            {
                ParseJsonDependencies(value, metadata);
            }
            else if (value.Contains('\n'))
            {
                ParseYamlDependencies(value, metadata);
            }
            else
            {
                ParseSimpleDependencies(value, metadata);
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Failed to parse dependencies: {ex.Message}");
        }
    }

    private static void ParseJsonDependencies(string value, MacroMetadata metadata)
    {
        // Simple JSON-like parsing for dependencies
        var dependencies = new List<IMacroDependency>();
        var matches = Regex.Matches(value, @"{([^}]+)}");

        foreach (Match match in matches)
        {
            var dep = new GitMacroDependency();
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
                        dep.RepositoryUrl = val;
                        break;
                    case "path":
                        dep.FilePath = val;
                        break;
                    case "branch":
                        dep.Branch = val;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(dep.RepositoryUrl) && !string.IsNullOrEmpty(dep.FilePath))
            {
                dependencies.Add(dep);
            }
        }

        metadata.Dependencies = dependencies;
    }

    private static void ParseYamlDependencies(string value, MacroMetadata metadata)
    {
        var dependencies = new List<IMacroDependency>();
        var lines = value.Split('\n');
        GitMacroDependency? currentDep = null;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("-"))
            {
                if (currentDep != null)
                {
                    dependencies.Add(currentDep);
                }
                currentDep = new GitMacroDependency();
            }
            else if (currentDep != null)
            {
                var parts = trimmed.Split(':');
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var val = parts[1].Trim();

                switch (key.ToLower())
                {
                    case "repo":
                        currentDep.RepositoryUrl = val;
                        break;
                    case "path":
                        currentDep.FilePath = val;
                        break;
                    case "branch":
                        currentDep.Branch = val;
                        break;
                }
            }
        }

        if (currentDep != null)
        {
            dependencies.Add(currentDep);
        }

        metadata.Dependencies = dependencies;
    }

    private static void ParseSimpleDependencies(string value, MacroMetadata metadata)
    {
        var dependencies = value.Split(',')
            .Select(v => v.Trim())
            .Where(v => !string.IsNullOrEmpty(v))
            .Select(v => new GitMacroDependency { FilePath = v })
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
            PluginLog.Error($"Failed to parse triggers: {ex.Message}");
        }
    }
}
