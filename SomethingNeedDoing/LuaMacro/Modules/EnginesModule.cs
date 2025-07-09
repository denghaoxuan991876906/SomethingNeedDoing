using NLua;
using System.Threading.Tasks;

namespace SomethingNeedDoing.LuaMacro.Modules;

/// <summary>
/// Lua module that provides access to various execution engines.
/// </summary>
public class EnginesModule : LuaModuleBase
{
    private readonly Dictionary<string, IEngine> _engines = [];

    public override string ModuleName => "Engines";

    public EnginesModule(IEnumerable<IEngine> engines)
    {
        foreach (var engine in engines)
            _engines[engine.Name.ToLowerInvariant()] = engine;
    }

    public override void Register(Lua lua)
    {
        base.Register(lua);
        RegisterEngineFunctions(lua);
    }

    private void RegisterEngineFunctions(Lua lua)
    {
        // Register each engine
        foreach (var (name, engine) in _engines)
        {
            lua[$"{ModuleName}.{name}"] = new
            {
                Run = new Action<string>(content => ExecuteEngine(engine, content)),
                RunAsync = new Func<string, Task>(content => ExecuteEngineAsync(engine, content)),
            };
        }

        // Register helper functions for auto-detection
        lua[$"{ModuleName}.Run"] = new Action<string>(ExecuteBestEngine);
        lua[$"{ModuleName}.RunAsync"] = new Func<string, Task>(ExecuteBestEngineAsync);
    }

    /// <summary>
    /// Executes content using the specified engine synchronously (fire and forget).
    /// </summary>
    /// <param name="engine">The engine to use.</param>
    /// <param name="content">The content to execute.</param>
    private void ExecuteEngine(IEngine engine, string content)
    {
        // Execute synchronously (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await engine.ExecuteAsync(content);
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"Error executing {engine.Name} content: {ex}");
            }
        });
    }

    /// <summary>
    /// Executes content using the specified engine asynchronously.
    /// </summary>
    /// <param name="engine">The engine to use.</param>
    /// <param name="content">The content to execute.</param>
    /// <returns>A task representing the execution.</returns>
    private async Task ExecuteEngineAsync(IEngine engine, string content) => await engine.ExecuteAsync(content);

    /// <summary>
    /// Executes content using the best available engine.
    /// </summary>
    /// <param name="content">The content to execute.</param>
    private void ExecuteBestEngine(string content)
    {
        if (FindBestEngine(content) is { } engine)
            ExecuteEngine(engine, content);
        else
            Svc.Log.Warning($"No suitable engine found for content: {content}");
    }

    /// <summary>
    /// Executes content using the best available engine asynchronously.
    /// </summary>
    /// <param name="content">The content to execute.</param>
    /// <returns>A task representing the execution.</returns>
    private async Task ExecuteBestEngineAsync(string content)
    {
        if (FindBestEngine(content) is { } engine)
            await ExecuteEngineAsync(engine, content);
        else
            Svc.Log.Warning($"No suitable engine found for content: {content}");
    }

    /// <summary>
    /// Finds the best engine for executing the given content.
    /// </summary>
    /// <param name="content">The content to find an engine for.</param>
    /// <returns>The best engine, or null if none found.</returns>
    private IEngine? FindBestEngine(string content)
    {
        return _engines.Values.FirstOrDefault(engine => engine.CanExecute(content));
    }
}
