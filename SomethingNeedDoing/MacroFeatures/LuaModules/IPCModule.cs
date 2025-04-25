using NLua;
using System.Reflection;

namespace SomethingNeedDoing.MacroFeatures.LuaModules;

/// <summary>
/// Lua module that exposes IPC functions from various plugins to Lua macros.
/// </summary>
public class IPCModule : LuaModuleBase
{
    public override string ModuleName => "IPC";
    private readonly Dictionary<string, IPC> _ipcInstances = [];

    public IPCModule()
    {
        var ipcTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IPC).IsAssignableFrom(t));
        foreach (var type in ipcTypes)
        {
            try
            {
                if (Activator.CreateInstance(type) is IPC ipc)
                    _ipcInstances[ipc.Name] = ipc;
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"Failed to create instance of {type.Name}: {ex.Message}");
            }
        }
    }

    public override void Register(Lua lua)
    {
        lua.DoString($"{ModuleName} = {{}}");

        RegisterHelperFunctions(lua);

        // Register each IPC class as a submodule
        foreach (var (name, instance) in _ipcInstances)
        {
            lua.DoString($"{ModuleName}.{name} = {{}}");

            // Register all fields marked with LuaFunction attribute
            var fields = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<LuaFunctionAttribute>() != null);

            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<LuaFunctionAttribute>();
                var fieldName = attr?.Name ?? field.Name;

                if (field.GetValue(instance) is { } fieldValue)
                    // Register the field value as a Lua function
                    lua[$"{ModuleName}.{name}.{fieldName}"] = fieldValue;
            }
        }
    }

    /// <summary>
    /// Registers helper functions in the Lua environment
    /// </summary>
    private void RegisterHelperFunctions(Lua lua)
    {
        lua[$"{ModuleName}.IsInstalled"] = new Func<string, bool>(IsIPCInstalled);
        lua[$"{ModuleName}.GetAvailablePlugins"] = new Func<string[]>(GetAvailablePlugins);
    }

    /// <summary>
    /// Checks if an IPC plugin is installed
    /// </summary>
    private bool IsIPCInstalled(string name) => GetIPC<IPC>(name)?.IsInstalled ?? false;

    /// <summary>
    /// Gets all available IPC plugins
    /// </summary>
    private string[] GetAvailablePlugins() => _ipcInstances.Values.Where(ipc => ipc.IsInstalled).Select(ipc => ipc.Name).ToArray();

    /// <summary>
    /// Gets an IPC instance by name
    /// </summary>
    private T? GetIPC<T>(string name) where T : IPC => _ipcInstances.TryGetValue(name, out var instance) ? instance as T : null;
}
