using NLua;
using SomethingNeedDoing.Macros.Lua;

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
        RegisterModule(new TargetingModule());
    }

    public void RegisterModule(ILuaModule module)
    {
        _modules.Add(module);
        _documentation.RegisterModule(module);
    }

    public void RegisterAll(Lua lua) => _modules.ForEach(m => m.Register(lua));
    public void ShowHelp() => _documentation.GenerateInGameHelp();
    public string GenerateMarkdown() => _documentation.GenerateMarkdown();
}
