using NLua;
using SomethingNeedDoing.LuaMacro.Wrappers;

namespace SomethingNeedDoing.LuaMacro.Modules;
public unsafe class AddonModule : LuaModuleBase
{
    public override string ModuleName => "Addons";
    public override void Register(Lua lua)
    {
        lua.DoString("NodeType = luanet.import_type('FFXIVClientStructs.FFXIV.Component.GUI.NodeType')");
        base.Register(lua);
    }

    [LuaFunction] public AddonWrapper GetAddon(string name) => new(name);
}
