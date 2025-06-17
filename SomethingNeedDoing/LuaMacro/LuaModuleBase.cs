using NLua;
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

    private readonly Dictionary<string, Delegate> _propertyDelegateMap = [];

    public virtual void Register(NLua.Lua lua)
    {
        // Create module table
        var modulePath = GetModulePath();
        lua.DoString($"{modulePath} = {{}}");
        RegisterMetaTable(lua, modulePath);

        // Register all methods marked with LuaFunction attribute
        foreach (var method in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (method.GetCustomAttribute<LuaFunctionAttribute>() is not { } attr) continue;
            var name = attr.Name ?? method.Name;
            lua[$"{modulePath}.{name}"] = CreateDelegate(method);
        }

        // Register all properties marked with LuaFunction attribute as getter functions for the metatable
        foreach (var property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetCustomAttribute<LuaFunctionAttribute>() is not { } attr) continue;
            var name = attr.Name ?? property.Name;
            _propertyDelegateMap[name] = CreateDelegate(property.GetMethod!);
        }
    }

    protected virtual object? MetaIndex(LuaTable table, string key) => null;

    private object? MetatableOnIndex(LuaTable table, string key)
    {
        if (_propertyDelegateMap.TryGetValue(key, out var propertyFunc))
            return propertyFunc.DynamicInvoke();
        return MetaIndex(table, key);
    }
    
    private void RegisterMetaTable(NLua.Lua lua, string modulePath)
    {
        var metaPath = $"{modulePath}.__mt";
        var metaTable = lua.GetTable(metaPath);
        if (metaTable == null)
        {
            lua.NewTable(metaPath);
            lua.DoString($"setmetatable({modulePath}, {metaPath})");
            metaTable = lua.GetTable(metaPath);
        }
        
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        metaTable[nameof(MetatableOnIndex)] = CreateDelegate(typeof(LuaModuleBase).GetMethod(nameof(MetatableOnIndex), flags)!);
        lua.DoString($"{metaPath}.__index = function(...) return {metaPath}.{nameof(MetatableOnIndex)}(...) end");
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
