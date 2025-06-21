using Dalamud.Game.Text;
using ECommons.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Text;

namespace SomethingNeedDoing;
/// <summary>
/// Configuration for the plugin.
/// </summary>
public class Config : IEzConfig
{
    public int Version { get; set; } = 1;

    #region General Settings
    public XivChatType ChatType { get; set; } = XivChatType.Debug;
    public XivChatType ErrorChatType { get; set; } = XivChatType.Urgent;

    /// <summary>
    /// Gets or sets whether pausing a macro should also pause its child macros.
    /// </summary>
    public bool PropagateControlsToChildren { get; set; } = true;
    public bool HasCompletedTutorial { get; set; } = false;
    public bool AcknowledgedLegacyWarning { get; set; } = false;
    #endregion

    #region Crafting Settings
    /// <summary>
    /// Gets or sets a value indicating whether to skip craft actions when not crafting.
    /// </summary>
    public bool CraftSkip { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to intelligently wait for crafting actions to complete instead of using wait modifiers.
    /// </summary>
    public bool SmartWait { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to skip quality increasing actions when at 100% HQ chance.
    /// </summary>
    public bool QualitySkip { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to count the /loop number as the total iterations, rather than the amount to loop.
    /// </summary>
    public bool LoopTotal { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to always echo /loop commands.
    /// </summary>
    public bool LoopEcho { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use the "CraftLoop" template.
    /// </summary>
    public bool UseCraftLoopTemplate { get; set; }

    /// <summary>
    /// Gets or sets the "CraftLoop" template.
    /// </summary>
    public string CraftLoopTemplate { get; set; } =
        "/craft {{count}}\n" +
        "/waitaddon \"RecipeNote\" <maxwait.5>" +
        "/click \"RecipeNote Synthesize\"" +
        "/waitaddon \"Synthesis\" <maxwait.5>" +
        "{{macro}}" +
        "/loop";

    /// <summary>
    /// Gets or sets a value indicating whether to start crafting loops from the recipe note window.
    /// </summary>
    public bool CraftLoopFromRecipeNote { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum wait value for the "CraftLoop" maxwait modifier.
    /// </summary>
    public int CraftLoopMaxWait { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether the "CraftLoop" loop should have an echo modifier.
    /// </summary>
    public bool CraftLoopEcho { get; set; }
    #endregion

    #region Error Handling
    /// <summary>
    /// Gets or sets a value indicating whether to stop the macro on error (only used for natives)
    /// </summary>
    public bool StopOnError { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retries when an action does not receive a timely response.
    /// </summary>
    public int MaxTimeoutRetries { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether errors should be audible.
    /// </summary>
    public bool NoisyErrors { get; set; }

    /// <summary>
    /// Gets or sets the beep frequency.
    /// </summary>
    public int BeepFrequency { get; set; } = 900;

    /// <summary>
    /// Gets or sets the beep duration.
    /// </summary>
    public int BeepDuration { get; set; } = 250;

    /// <summary>
    /// Gets or sets the beep count.
    /// </summary>
    public int BeepCount { get; set; } = 3;
    #endregion

    #region AutoRetainer Integration
    public List<ulong> ARCharacterPostProcessExcludedCharacters { get; set; } = [];
    #endregion

    #region Lua Settings
    public string[] LuaRequirePaths { get; set; } = [];
    #endregion

    #region Changelog
    public string LastSeenVersion = string.Empty;
    #endregion

    public List<ConfigMacro> Macros { get; set; } = [];

    public void Save() => EzConfig.Save();

    /// <summary>
    /// Gets all macros in a specific folder.
    /// </summary>
    public IEnumerable<ConfigMacro> GetMacrosInFolder(string folderPath) => Macros.Where(m => m.FolderPath == folderPath);

    /// <summary>
    /// Gets all folder paths.
    /// </summary>
    public IEnumerable<string> GetFolderPaths() => Macros.Select(m => m.FolderPath).Distinct();

    /// <summary>
    /// Moves a macro to a different folder.
    /// </summary>
    public void MoveMacro(string macroId, string newFolderPath) => Macros.FirstOrDefault(m => m.Id == macroId)?.FolderPath = newFolderPath;

    /// <summary>
    /// Deletes a macro.
    /// </summary>
    public void DeleteMacro(string macroId) => Macros.RemoveAll(m => m.Id == macroId);

    #region Helper Methods

    /// <summary>
    /// Gets a macro by its ID.
    /// </summary>
    public ConfigMacro? GetMacro(string macroId)
        => Macros.FirstOrDefault(m => m.Id == macroId);

    /// <summary>
    /// Gets a macro by its name, optionally in a specific folder.
    /// </summary>
    public ConfigMacro? GetMacroByName(string name, string? folderPath = null)
        => Macros.FirstOrDefault(m =>
            m.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
            (folderPath == null || m.FolderPath == folderPath));

    /// <summary>
    /// Gets all macros of a specific type.
    /// </summary>
    public IEnumerable<ConfigMacro> GetMacrosByType(MacroType type)
        => Macros.Where(m => m.Type == type);

    /// <summary>
    /// Gets all macros in a folder and its subfolders.
    /// </summary>
    public IEnumerable<ConfigMacro> GetMacrosInFolderRecursive(string folderPath)
        => Macros.Where(m => m.FolderPath.StartsWith(folderPath));

    /// <summary>
    /// Gets the immediate subfolders of a folder.
    /// </summary>
    public IEnumerable<string> GetSubfolders(string folderPath)
    {
        var prefix = folderPath == "/" ? "" : folderPath + "/";
        return Macros
            .Select(m => m.FolderPath)
            .Where(p => p.StartsWith(prefix))
            .Select(p => p.Split('/', StringSplitOptions.RemoveEmptyEntries)[0])
            .Distinct()
            .Select(f => prefix + f);
    }

    /// <summary>
    /// Gets the full path of a macro.
    /// </summary>
    public string GetMacroPath(string macroId)
    {
        var macro = GetMacro(macroId);
        return macro == null ? string.Empty : Path.Combine(macro.FolderPath, macro.Name);
    }

    /// <summary>
    /// Gets the parent folder path of a macro.
    /// </summary>
    public string GetMacroParentFolder(string macroId)
    {
        var macro = GetMacro(macroId);
        if (macro == null) return string.Empty;

        var lastSlash = macro.FolderPath.LastIndexOf('/');
        return lastSlash <= 0 ? "/" : macro.FolderPath[..lastSlash];
    }

    /// <summary>
    /// Validates if a macro name is valid for the given folder.
    /// </summary>
    public bool IsValidMacroName(string name, string folderPath)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return !GetMacrosInFolder(folderPath)
            .Any(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the number of macros in a folder.
    /// </summary>
    public int GetMacroCount(string folderPath)
        => GetMacrosInFolder(folderPath).Count();

    /// <summary>
    /// Gets the total number of macros in a folder and its subfolders.
    /// </summary>
    public int GetTotalMacroCount(string folderPath)
        => GetMacrosInFolderRecursive(folderPath).Count();

    /// <summary>
    /// Gets all macros that contain a specific text in their content.
    /// </summary>
    public IEnumerable<ConfigMacro> SearchMacros(string searchText, bool caseSensitive = false)
    {
        var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        return Macros.Where(m =>
            m.Name.Contains(searchText, comparison) ||
            m.Content.Contains(searchText, comparison));
    }

    /// <summary>
    /// Gets all folder paths and their depths in the tree.
    /// </summary>
    public IEnumerable<(string Path, int Depth)> GetFolderTree()
    {
        // Get all unique folder paths from macros
        var paths = Macros
            .Select(m => m.FolderPath)
            .Distinct()
            .OrderBy(p => p);

        // Track processed paths to avoid duplicates
        var processedPaths = new HashSet<string>();

        foreach (var path in paths)
        {
            // Skip the root folder itself - we don't want to display it
            if (path == "/")
                continue;

            // Split path into parts and calculate depth
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // If this is a direct child of root (depth 1), yield it with depth 0
            if (parts.Length == 1)
            {
                yield return (path, 0);
                processedPaths.Add(path);
                continue;
            }

            // For deeper paths, build each part of the path
            var currentPath = "";
            for (var i = 0; i < parts.Length; i++)
            {
                currentPath = i == 0 ? "/" + parts[i] : currentPath + "/" + parts[i];
                if (!currentPath.StartsWith("/"))
                    currentPath = "/" + currentPath;

                // Only yield if we haven't processed this path yet
                if (!processedPaths.Contains(currentPath))
                {
                    // For paths with depth > 1, adjust the depth to be relative to the first level
                    var adjustedDepth = i;
                    yield return (currentPath, adjustedDepth);
                    processedPaths.Add(currentPath);
                }
            }
        }
    }

    /// <summary>
    /// Gets the folder structure as a tree with macro counts.
    /// </summary>
    public IEnumerable<(string Path, int Depth, int Count)> GetFolderTreeWithCounts()
    {
        var allFolders = GetFolderTree().ToList();
        var folderCounts = new Dictionary<string, int>();

        // First calculate counts for all folders
        foreach (var macro in Macros)
        {
            var path = macro.FolderPath;
            if (!folderCounts.ContainsKey(path))
                folderCounts[path] = 0;
            folderCounts[path]++;
        }

        // Return folder info with counts
        foreach (var (path, depth) in allFolders)
        {
            var count = folderCounts.TryGetValue(path, out var c) ? c : 0;
            yield return (path, depth, count);
        }
    }

    /// <summary>
    /// Gets all nodes in the configuration.
    /// </summary>
    public IEnumerable<ConfigMacro> GetAllNodes() => Macros;

    /// <summary>
    /// Sets a property value by name.
    /// </summary>
    public void SetProperty(string name, string value)
    {
        var property = GetType().GetProperty(name);
        if (property == null) return;

        try
        {
            var convertedValue = Convert.ChangeType(value, property.PropertyType);
            property.SetValue(this, convertedValue);
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, $"Failed to set property {name} to {value}");
        }
    }

    #endregion
}

public class ConfigFactory : ISerializationFactory
{
    public string DefaultConfigFileName => "ezSomethingNeedDoing.json";

    public bool IsBinary => false;

    public T? Deserialize<T>(string inputData)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(inputData, JsonSerializerSettings);
        }
        catch
        {
            return JsonConvert.DeserializeObject<T>(inputData);
        }
    }

    public string? Serialize(object data, bool pretty = false)
        => JsonConvert.SerializeObject(data, JsonSerializerSettings);
    public string? Serialize(object config) => Serialize(config, false);
    public T? Deserialize<T>(byte[] inputData) => Deserialize<T>(Encoding.UTF8.GetString(inputData));
    public byte[]? SerializeAsBin(object config) => Serialize(config) is { } serialized ? Encoding.UTF8.GetBytes(serialized) : [];

    public static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        SerializationBinder = new CustomSerializationBinder(),
        Formatting = Formatting.Indented,
    };

    public class CustomSerializationBinder : DefaultSerializationBinder
    {
        public override Type BindToType(string? assemblyName, string typeName)
        {
            try
            {
                return base.BindToType(assemblyName, typeName);
            }
            catch (Exception)
            {
                if (assemblyName == null || assemblyName == typeof(Plugin).Assembly.GetName().Name)
                    return typeof(Plugin).Assembly.GetTypes().First(x => x.FullName == typeName);
                throw;
            }
        }
    }
}

