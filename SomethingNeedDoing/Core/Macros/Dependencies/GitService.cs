using SomethingNeedDoing.Framework.Interfaces;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;

/// <summary>
/// Implementation of the Git service using the GitHub API.
/// </summary>
public class GitService : IGitService
{
    private readonly HttpClient _httpClient;
    private const string GitHubApiBaseUrl = "https://api.github.com";

    /// <summary>
    /// Initializes a new instance of the <see cref="GitService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public GitService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SomethingNeedDoing");
    }

    /// <inheritdoc/>
    public async Task<string> GetFileContentAsync(string repositoryUrl, string branch, string path)
    {
        var apiUrl = GetGitHubApiUrl(repositoryUrl, branch, path);
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var fileInfo = JsonSerializer.Deserialize<GitHubFileInfo>(content);
        return fileInfo?.Content;
    }

    /// <inheritdoc/>
    public async Task<bool> FileExistsAsync(string repositoryUrl, string branch, string path)
    {
        try
        {
            var apiUrl = GetGitHubApiUrl(repositoryUrl, branch, path);
            var response = await _httpClient.GetAsync(apiUrl);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string GetGitHubApiUrl(string repositoryUrl, string branch, string path)
    {
        // Convert repository URL to GitHub API URL
        var uri = new Uri(repositoryUrl);
        var pathParts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var owner = pathParts[0];
        var repo = pathParts[1].Replace(".git", "");

        return $"{GitHubApiBaseUrl}/repos/{owner}/{repo}/contents/{path}?ref={branch}";
    }

    private class GitHubFileInfo
    {
        public string Content { get; set; } = string.Empty;
        public string Encoding { get; set; } = string.Empty;
    }
}
