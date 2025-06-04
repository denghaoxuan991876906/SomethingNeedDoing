using SomethingNeedDoing.Core.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Represents an HTTP dependency.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HttpDependency"/> class.
/// </remarks>
/// <param name="httpClient">The HTTP client.</param>
/// <param name="url">The URL of the dependency.</param>
/// <param name="version">The version of the dependency.</param>
public class HttpDependency(HttpClient httpClient, string url, string version) : IMacroDependency
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    /// <inheritdoc/>
    public string Id { get; } = url ?? throw new ArgumentNullException(nameof(url));

    /// <inheritdoc/>
    public string Name => System.IO.Path.GetFileNameWithoutExtension(Id);

    /// <inheritdoc/>
    public DependencyType Type => DependencyType.Http;

    /// <inheritdoc/>
    public string Version { get; } = version ?? throw new ArgumentNullException(nameof(version));

    /// <inheritdoc/>
    public async Task<string> GetContentAsync()
    {
        var response = await _httpClient.GetAsync(Id);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(Id);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
