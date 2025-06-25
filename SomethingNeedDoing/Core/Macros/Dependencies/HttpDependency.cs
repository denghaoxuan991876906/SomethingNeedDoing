using SomethingNeedDoing.Core.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Represents an HTTP dependency.
/// </summary>
public class HttpDependency : CachedDependency
{
    private readonly HttpClient _httpClient = new()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "SomethingNeedDoing/1.0" }
        }
    };

    /// <inheritdoc/>
    public override string Id { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public override string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override DependencyType Type => DependencyType.Remote;

    /// <inheritdoc/>
    public override string Source { get; set; } = string.Empty;

    /// <inheritdoc/>
    protected override async Task<string> DownloadContentAsync()
    {
        var response = await _httpClient.GetAsync(Source);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <inheritdoc/>
    public override async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(Source);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override async Task<DependencyValidationResult> ValidateAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(Source);
            if (!response.IsSuccessStatusCode)
                return DependencyValidationResult.Failure($"HTTP request failed with status code: {response.StatusCode}");

            // Try to read the content to validate it
            await response.Content.ReadAsStringAsync();
            return DependencyValidationResult.Success();
        }
        catch (Exception ex)
        {
            return DependencyValidationResult.Failure($"Error validating HTTP dependency: {ex.Message}");
        }
    }
}
