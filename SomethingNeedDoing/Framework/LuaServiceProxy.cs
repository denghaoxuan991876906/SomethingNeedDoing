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
            var service = typeof(Svc).GetProperty(serviceName)?.GetValue(null);
            if (service == null)
            {
                result = null;
                return false;
            }

            var property = service.GetType().GetProperty(binder.Name);
            if (property == null)
            {
                result = null;
                return false;
            }

            result = property.GetValue(service);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            var service = typeof(Svc).GetProperty(serviceName)?.GetValue(null);
            if (service == null)
            {
                result = null;
                return false;
            }

            var method = service.GetType().GetMethod(binder.Name);
            if (method == null)
            {
                result = null;
                return false;
            }

            result = method.Invoke(service, args);
            return true;
        }
    }

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
