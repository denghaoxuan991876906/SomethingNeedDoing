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
        RegisterModule(new InstancesModule());
        RegisterModule(new GameStateModule());
        RegisterModule(new IPCModule());
        RegisterModule(new ExcelModule());
        RegisterModule(new TargetingModule());
        //RegisterModule(new FateModule());
    }

    public void RegisterAll(Lua lua) => _modules.ForEach(m => m.Register(lua));
    public T? GetModule<T>() where T : class, ILuaModule => _modules.FirstOrDefault(m => m is T, null) as T;

    public void RegisterModule(ILuaModule module)
    {
        if (module is LuaModuleBase baseModule && baseModule.ParentType != null)
        {
            if (_modules.FirstOrDefault(m => m.GetType() == baseModule.ParentType) is { } parent)
                baseModule.ParentModule = parent;
            else
                throw new InvalidOperationException($"Parent module of type {baseModule.ParentType.Name} not found for {module.GetType().Name}");
        }

        _modules.Add(module);
        _documentation.RegisterModule(module);
    }
}
