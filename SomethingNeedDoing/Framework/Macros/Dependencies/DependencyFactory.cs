using System.Net.Http;

namespace SomethingNeedDoing.Framework;

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
    private readonly HttpClient _httpClient = httpClient;
    private readonly IGitService _gitService = gitService;

    /// <summary>
    /// Creates a dependency based on the specified type and parameters.
    /// </summary>
    /// <param name="type">The type of dependency.</param>
    /// <param name="name">The name of the dependency.</param>
    /// <param name="parameters">The parameters for creating the dependency.</param>
    /// <returns>The created dependency.</returns>
    /// <exception cref="ArgumentException">Thrown when the dependency type is not supported or required parameters are missing.</exception>
    public IMacroDependency CreateDependency(DependencyType type, string name, params string[] parameters)
    {
        return type switch
        {
            DependencyType.GitRepository => CreateGitDependency(name, parameters),
            DependencyType.Http => CreateHttpDependency(name, parameters),
            _ => throw new ArgumentException($"Unsupported dependency type: {type}", nameof(type))
        };
    }

    private IMacroDependency CreateGitDependency(string name, string[] parameters)
    {
        if (parameters.Length < 1)
            throw new ArgumentException("Git dependency requires at least a repository URL", nameof(parameters));

        var repositoryUrl = parameters[0];
        var branch = parameters.Length > 1 ? parameters[1] : "main";
        var path = parameters.Length > 2 ? parameters[2] : null;

        return new GitRepositoryDependency(_gitService, repositoryUrl, branch, path, name);
    }

    private IMacroDependency CreateHttpDependency(string name, string[] parameters)
    {
        if (parameters.Length < 1)
            throw new ArgumentException("HTTP dependency requires a URL", nameof(parameters));

        var url = parameters[0];
        return new HttpDependency(_httpClient, url, name);
    }
}
