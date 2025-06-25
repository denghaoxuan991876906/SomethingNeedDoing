using SomethingNeedDoing.Core.Interfaces;
using System.Net.Http;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Factory for creating dependencies.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DependencyFactory"/> class.
/// </remarks>
/// <param name="httpClient">The HTTP client.</param>
/// <param name="gitService">The Git service.</param>
public class DependencyFactory(HttpClient httpClient, IGitService gitService)
{

    /// <summary>
    /// Creates a dependency from a source string, automatically detecting the type.
    /// </summary>
    /// <param name="source">The source URL or path.</param>
    /// <returns>The created dependency.</returns>
    public IMacroDependency CreateDependency(string source)
    {
        var normalizedSource = NormalizeSource(source);

        if (normalizedSource.StartsWith("git://"))
        {
            var parts = normalizedSource[6..].Split('/');
            if (parts.Length >= 2)
            {
                var owner = parts[0];
                var repo = parts[1];
                var branch = parts.Length > 2 ? parts[2] : "main";
                var path = parts.Length > 3 ? string.Join("/", parts.Skip(3)) : null;

                return new GitDependency(gitService, $"https://github.com/{owner}/{repo}", branch, path, repo);
            }
        }
        else if (System.IO.File.Exists(source))
            return new LocalDependency(source, System.IO.Path.GetFileNameWithoutExtension(source));

        return new HttpDependency(httpClient, source, "latest");
    }

    /// <summary>
    /// Normalizes a source URL to a consistent format.
    /// </summary>
    private string NormalizeSource(string source)
    {
        if (source.Contains("github.com"))
        {
            var uri = new Uri(source);
            var pathParts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length >= 2)
            {
                var owner = pathParts[0];
                var repo = pathParts[1];
                var branch = pathParts.Length > 3 && pathParts[2] == "blob" ? pathParts[3] : "main";
                var path = pathParts.Length > 4 && pathParts[2] == "blob" ? string.Join("/", pathParts.Skip(4)) : null;

                return $"git://{owner}/{repo}/{branch}/{path}";
            }
        }

        return source;
    }
}
