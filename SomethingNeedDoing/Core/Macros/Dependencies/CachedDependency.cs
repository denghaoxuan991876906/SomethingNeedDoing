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
    public abstract string Name { get; set; }

    /// <inheritdoc/>
    public abstract DependencyType Type { get; }

    /// <inheritdoc/>
    public abstract string Source { get; set; }

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
        var cacheKey = Convert.ToHexString(hash).ToLowerInvariant();
        return cacheKey;
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
            {
                var age = DateTime.Now - cacheTime;
                var isValid = age < _cacheExpiration;
                Svc.Log.Debug($"[{nameof(CachedDependency)}] Cache age: {age.TotalHours:F2} hours, Valid: {isValid}");
                return isValid;
            }
            else
                Svc.Log.Debug($"[{nameof(CachedDependency)}] Failed to parse metadata for {Name}");
        }
        catch (Exception ex)
        {
            Svc.Log.Warning($"[{nameof(CachedDependency)}] Failed to read cache metadata for {Name}: {ex.Message}");
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
        }
        catch (Exception ex)
        {
            Svc.Log.Warning($"[{nameof(CachedDependency)}] Failed to cache dependency {Name}: {ex.Message}");
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
            var cacheKey = GetCacheKey();
            Svc.Log.Debug($"[{nameof(CachedDependency)}] Attempting to load {Name} with key '{cacheKey}' from {CacheFilePath}");

            if (IsCacheValid())
                return File.ReadAllText(CacheFilePath);
            else
                Svc.Log.Debug($"[{nameof(CachedDependency)}] Cache not valid for {Name} (key: {cacheKey})");
        }
        catch (Exception ex)
        {
            Svc.Log.Warning($"[{nameof(CachedDependency)}] Failed to load cached dependency {Name}: {ex.Message}");
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
        {
            Svc.Log.Debug($"[{nameof(CachedDependency)}] Using cached content for {Name}");
            return cachedContent;
        }

        Svc.Log.Debug($"[{nameof(CachedDependency)}] Cache miss for {Name}, downloading content");
        var content = await DownloadContentAsync();
        Svc.Log.Debug($"[{nameof(CachedDependency)}] Downloaded content for {Name}, length: {content.Length} characters");
        SaveToCache(content);
        return content;
    }

    /// <inheritdoc/>
    public abstract Task<bool> IsAvailableAsync();

    /// <inheritdoc/>
    public abstract Task<DependencyValidationResult> ValidateAsync();
}
