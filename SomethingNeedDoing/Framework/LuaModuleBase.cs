using NLua;
using System.Linq.Expressions;
using System.Reflection;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Base class for Lua API modules
/// </summary>
public abstract class LuaModuleBase : ILuaModule
{
    public abstract string ModuleName { get; }

    public virtual void Register(Lua lua)
    {
        // Create module table
        lua.DoString($"{ModuleName} = {{}}");

        // Register all methods marked with LuaFunction attribute
        foreach (var method in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (method.GetCustomAttribute<LuaFunctionAttribute>() is not { } attr) continue;
            lua[$"{ModuleName}.{attr.Name ?? method.Name}"] = CreateDelegate(method);
        }
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
