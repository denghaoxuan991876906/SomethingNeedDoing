using ECommons;
using SomethingNeedDoing.Core.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace SomethingNeedDoing.LuaMacro;
/// <summary>
/// Base class for Lua API modules
/// </summary>
public abstract class LuaModuleBase : ILuaModule
{
    public abstract string ModuleName { get; }
    public ILuaModule? ParentModule { get; set; }
    public virtual Type? ParentType => null;

    private LuaModuleManager? _moduleManager;
    internal void SetModuleManager(LuaModuleManager manager) => _moduleManager = manager;
    protected T? GetModule<T>() where T : class, ILuaModule => _moduleManager?.GetModule<T>();

    public virtual void Register(NLua.Lua lua)
    {
        // Create module table
        var modulePath = GetModulePath();
        lua.DoString($"{modulePath} = {{}}");

        // Register all methods marked with LuaFunction attribute
        foreach (var method in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (method.GetCustomAttribute<LuaFunctionAttribute>() is not { } attr) continue;
            var name = attr.Name ?? method.Name;
            lua[$"{modulePath}.{name}"] = CreateDelegate(method);

            if (name.StartsWith("__"))
                RegisterMetamethod(lua, modulePath, name);
        }

        // Register all properties marked with LuaFunction attribute as getter functions
        foreach (var property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetCustomAttribute<LuaFunctionAttribute>() is not { } attr) continue;
            var name = attr.Name ?? property.Name;
            lua[$"{modulePath}.{name}"] = CreateDelegate(property.GetMethod!);

            if (name.StartsWith("__"))
                RegisterMetamethod(lua, modulePath, name);
        }
    }

    private static void RegisterMetamethod(NLua.Lua lua, string modulePath, string name)
    {
        var metaPath = $"{modulePath}.__metatable";
        var table = lua.GetTable(metaPath);
        if (table == null)
        {
            lua.NewTable(metaPath);
            lua.DoString($"setmetatable({modulePath}, {metaPath})");
            table = lua.GetTable(metaPath);
        }

        if (table != null)
        {
            lua.DoString($"{metaPath}.{name} = function(...) return {modulePath}.{name}(...) end");
        }
    }

    public string GetModulePath()
    {
        if (ParentModule == null)
            return ModuleName;

        if (ParentModule is LuaModuleBase parent)
            return $"{parent.GetModulePath()}.{ModuleName}";

        return $"{ParentModule.ModuleName}.{ModuleName}";
    }

    private Delegate CreateDelegate(MethodInfo method)
    {
        var delegateType = GetDelegateType(method);
        return Delegate.CreateDelegate(delegateType, this, method);
    }

    private Type GetDelegateType(MethodInfo method)
    {
        var parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
        return method.ReturnType == typeof(void) ? Expression.GetActionType(parameters) : Expression.GetFuncType([.. parameters, method.ReturnType]);
    }
}
