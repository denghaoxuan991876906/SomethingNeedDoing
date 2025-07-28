using SomethingNeedDoing.Core.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Core;

/// <summary>
/// Implementation of the dependency manager.
/// </summary>
public class DependencyManager(DependencyFactory factory) : IDependencyManager
{
    private readonly ConcurrentDictionary<string, IMacroDependency> _dependencies = [];
    private readonly ConcurrentDictionary<string, HashSet<string>> _macroDependencies = [];

    public async Task<IMacroDependency?> GetDependencyAsync(string id)
        => _dependencies.TryGetValue(id, out var dependency) ? dependency : null;

    public async Task<IMacroDependency> AddDependencyAsync(string source)
    {
        var dependency = factory.CreateDependency(source);
        _dependencies[dependency.Id] = dependency;
        return dependency;
    }

    public async Task RemoveDependencyAsync(string id)
    {
        _dependencies.TryRemove(id, out _);
        foreach (var macroDeps in _macroDependencies.Values)
            macroDeps.Remove(id);
    }

    public async Task<bool> UpdateDependencyAsync(string id)
    {
        if (!_dependencies.TryGetValue(id, out var dependency))
            return false;

        var validation = await dependency.ValidateAsync();
        if (!validation.IsValid)
            return false;

        if (dependency.Type == DependencyType.Remote)
        {
            try
            {
                // Force a fresh download of the content
                await dependency.GetContentAsync();
                return true;
            }
            catch (Exception ex)
            {
                FrameworkLogger.Error(ex, $"Failed to update dependency {dependency.Name}");
                return false;
            }
        }

        return true;
    }

    public async Task<Dictionary<string, DependencyValidationResult>> ValidateAllAsync()
    {
        var results = new Dictionary<string, DependencyValidationResult>();
        foreach (var dependency in _dependencies.Values)
            results[dependency.Id] = await dependency.ValidateAsync();
        return results;
    }

    public async Task<List<IMacroDependency>> ResolveDependenciesAsync(string macroId)
    {
        var resolved = new HashSet<string>();
        var dependencies = new List<IMacroDependency>();

        if (_macroDependencies.TryGetValue(macroId, out var macroDeps))
            await ResolveDependenciesRecursiveAsync(macroDeps, resolved, dependencies);

        return dependencies;
    }

    private async Task ResolveDependenciesRecursiveAsync(HashSet<string> dependencyIds, HashSet<string> resolved, List<IMacroDependency> dependencies)
    {
        foreach (var id in dependencyIds)
        {
            if (resolved.Contains(id))
                continue;

            if (_dependencies.TryGetValue(id, out var dependency))
            {
                resolved.Add(id);
                dependencies.Add(dependency);

                if (_macroDependencies.TryGetValue(id, out var nestedDeps))
                    await ResolveDependenciesRecursiveAsync(nestedDeps, resolved, dependencies);
            }
        }
    }

    public void AddMacroDependency(string macroId, string dependencyId)
        => _macroDependencies.GetOrAdd(macroId, _ => []).Add(dependencyId);

    public void RemoveMacroDependency(string macroId, string dependencyId)
    {
        if (_macroDependencies.TryGetValue(macroId, out var deps))
            deps.Remove(dependencyId);
    }
}
