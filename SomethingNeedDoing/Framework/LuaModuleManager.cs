using NLua;
using SomethingNeedDoing.MacroFeatures.LuaModules;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Manages Lua modules and their registration.
/// </summary>
public class LuaModuleManager
{
    private readonly List<ILuaModule> _modules = [];
    private readonly LuaDocumentation _documentation = new();

    public LuaModuleManager()
    {
        RegisterModule(new GameStateModule());
        RegisterModule(new IPCModule());
        RegisterModule(new TargetingModule());
    }

    public void RegisterAll(Lua lua) => _modules.ForEach(m => m.Register(lua));
    public T? GetModule<T>() where T : class, ILuaModule => _modules.FirstOrDefault(m => m is T, null) as T;
    private void RegisterModule(ILuaModule module)
    {
        _modules.Add(module);
        _documentation.RegisterModule(module);
    }
}
