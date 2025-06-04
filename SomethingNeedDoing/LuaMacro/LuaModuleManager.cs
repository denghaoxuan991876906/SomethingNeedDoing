using NLua;
using SomethingNeedDoing.Documentation;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.LuaMacro.Modules;

namespace SomethingNeedDoing.LuaMacro;
/// <summary>
/// Manages Lua modules and their registration.
/// </summary>
public class LuaModuleManager
{
    private readonly List<ILuaModule> _modules = [];
    private readonly LuaDocumentation _documentation;

    public LuaModuleManager(LuaDocumentation documentation)
    {
        _documentation = documentation;
        RegisterModule(new ActionsModule());
        RegisterModule(new InstancesModule());
        RegisterModule(new EntityModule());
        RegisterModule(new IPCModule());
        RegisterModule(new ExcelModule());
        RegisterModule(new AddonModule());
        RegisterModule(new PlayerModule());
        RegisterModule(new InstancedContentModule());
        RegisterModule(new QuestsModule());
        RegisterModule(new SystemModule());
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

        if (module is IPCModule ipcModule)
            ipcModule.RegisterDocumentation(_documentation);
        else
            _documentation.RegisterModule(module);
    }
}
