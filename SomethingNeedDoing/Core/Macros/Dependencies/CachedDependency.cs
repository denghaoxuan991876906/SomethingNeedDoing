using SomethingNeedDoing.Core.Interfaces;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Base class for dependencies that can be cached to disk.
/// </summary>
public abstract class CachedDependency : IMacroDependency
{
    private static readonly string CacheDirectory = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "cache", "dependencies");
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromDays(1);

    static CachedDependency()
    {
        Directory.CreateDirectory(CacheDirectory);
    }

    /// <inheritdoc/>
    public abstract string Id { get; }

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract DependencyType Type { get; }

    /// <inheritdoc/>
    public abstract string Source { get; }

    /// <summary>
    /// Gets the cache file path for this dependency.
    /// </summary>
    protected string CacheFilePath => Path.Combine(CacheDirectory, $"{GetCacheKey()}.cache");

    /// <summary>
    /// Gets the cache metadata file path for this dependency.
    /// </summary>
    protected string CacheMetadataPath => Path.Combine(CacheDirectory, $"{GetCacheKey()}.meta");

    /// <summary>
    /// Generates a unique cache key for this dependency.
    /// </summary>
    /// <returns>The cache key.</returns>
    protected virtual string GetCacheKey()
    {
        using var sha256 = SHA256.Create();
        var cacheString = $"{Type}_{Source}_{Name}";

        if (this is GitDependency gitDep)
            cacheString += $"_{gitDep.GitInfo.Branch}_{gitDep.GitInfo.FilePath}";

        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(cacheString));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Checks if the cached content is still valid.
    /// </summary>
    /// <returns>True if the cache is valid, false otherwise.</returns>
    protected bool IsCacheValid()
    {
        if (!File.Exists(CacheFilePath) || !File.Exists(CacheMetadataPath))
            return false;

        try
        {
            var metadata = File.ReadAllText(CacheMetadataPath);
            if (DateTime.TryParse(metadata, out var cacheTime))
                return DateTime.Now - cacheTime < _cacheExpiration;
        }
        catch (Exception ex)
        {
            Svc.Log.Warning($"Failed to read cache metadata for {Name}: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Saves content to the cache.
    /// </summary>
    /// <param name="content">The content to cache.</param>
    protected void SaveToCache(string content)
    {
        try
        {
            var cacheKey = GetCacheKey();
            File.WriteAllText(CacheFilePath, content);
            File.WriteAllText(CacheMetadataPath, DateTime.Now.ToString("O"));
            Svc.Log.Debug($"Cached dependency {Name} (key: {cacheKey}) to {CacheFilePath}");
        }
        catch (Exception ex)
        {
            Svc.Log.Warning($"Failed to cache dependency {Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads content from the cache.
    /// </summary>
    /// <returns>The cached content, or null if not available.</returns>
    protected string? LoadFromCache()
    {
        try
        {
            if (IsCacheValid())
            {
                var content = File.ReadAllText(CacheFilePath);
                var cacheKey = GetCacheKey();
                Svc.Log.Debug($"Loaded cached dependency {Name} (key: {cacheKey}) from {CacheFilePath}");
                return content;
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Warning($"Failed to load cached dependency {Name}: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Downloads the content from the remote source.
    /// </summary>
    /// <returns>The downloaded content.</returns>
    protected abstract Task<string> DownloadContentAsync();

    /// <inheritdoc/>
    public async Task<string> GetContentAsync()
    {
        var cachedContent = LoadFromCache();
        if (cachedContent != null)
            return cachedContent;

        var content = await DownloadContentAsync();
        SaveToCache(content);
        return content;
    }

    /// <inheritdoc/>
    public abstract Task<bool> IsAvailableAsync();

    /// <inheritdoc/>
    public abstract Task<DependencyValidationResult> ValidateAsync();
}
