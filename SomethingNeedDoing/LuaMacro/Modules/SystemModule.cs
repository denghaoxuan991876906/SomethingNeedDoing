namespace SomethingNeedDoing.LuaMacro.Modules;
public unsafe class SystemModule : LuaModuleBase
{
    public override string ModuleName => "System";

    [LuaFunction] public void FlashIcon() => WindowFlash.Flash();
}
