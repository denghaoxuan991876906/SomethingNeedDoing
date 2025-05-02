using NLua;
using System.Dynamic;
using System.Reflection;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Creates dynamic proxies for Dalamud services that can be safely used in Lua scripts.
/// </summary>
public class LuaServiceProxy
{
    private class DalamudServiceProxy(string serviceName) : DynamicObject
    {
        private readonly string serviceName = serviceName;

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            try
            {
                if (typeof(Svc).GetProperty(serviceName)?.GetValue(null) is not { } service)
                {
                    result = null;
                    return false;
                }

                if (service.GetType().GetProperty(binder.Name) is not { } property)
                {
                    result = null;
                    return false;
                }

                result = property.GetValue(service);
                return true;
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, $"Failed to get {serviceName}.{binder.Name}");
                result = null;
                return false;
            }
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            try
            {
                if (typeof(Svc).GetProperty(serviceName)?.GetValue(null) is not { } service)
                {
                    result = null;
                    return false;
                }

                if (service.GetType().GetMethod(binder.Name) is not { } method)
                {
                    result = null;
                    return false;
                }

                result = method.Invoke(service, args);
                return true;
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, $"Failed to invoke {serviceName}.{binder.Name}");
                result = null;
                return false;
            }
        }
    }

    /// <summary>
    /// Registers all Dalamud services as dynamically accessible objects in a lua table.
    /// </summary>
    /// <param name="lua"></param>
    public static void RegisterServices(Lua lua)
    {
        // Create a table to hold all services
        lua.DoString("dalamud = {}");

        // Register each service as a dynamic proxy
        foreach (var prop in typeof(Svc).GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            var proxy = new DalamudServiceProxy(prop.Name);
            lua[prop.Name] = proxy;
        }
    }
}
