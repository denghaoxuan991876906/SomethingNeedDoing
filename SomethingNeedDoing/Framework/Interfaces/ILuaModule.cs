using NLua;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Base interface for all Lua API modules.
/// </summary>
public interface ILuaModule
{
    /// <summary>
    /// Gets the name of the module as it will appear in Lua.
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Registers this module's functions with the Lua environment.
    /// </summary>
    void Register(Lua lua);
}
