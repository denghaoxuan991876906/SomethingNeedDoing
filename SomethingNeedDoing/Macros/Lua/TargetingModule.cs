namespace SomethingNeedDoing.Macros.Lua;
public class TargetingModule : LuaModuleBase
{
    public override string ModuleName => "targeting";

    public bool SetTarget(string name)
    {
        if (Svc.Objects.FirstOrDefault(o => o.Name.TextValue.Equals(name, StringComparison.OrdinalIgnoreCase)) is not { } target) return false;
        Svc.Targets.Target = target;
        return true;
    }

    public string? GetTargetName() => Svc.Targets.Target?.Name.TextValue;
}
