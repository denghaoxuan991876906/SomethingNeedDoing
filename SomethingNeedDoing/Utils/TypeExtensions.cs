using SomethingNeedDoing.Core.Interfaces;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Utils;
/// <summary>
/// Extensions for type checking and Lua type conversion.
/// </summary>
public static class TypeExtensions
{
    public static bool IsNumeric(this Type type)
    {
        if (type == null) return false;

        // Handle nullable types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            type = Nullable.GetUnderlyingType(type)!;

        return type == typeof(byte) ||
               type == typeof(sbyte) ||
               type == typeof(short) ||
               type == typeof(ushort) ||
               type == typeof(int) ||
               type == typeof(uint) ||
               type == typeof(long) ||
               type == typeof(ulong) ||
               type == typeof(float) ||
               type == typeof(double) ||
               type == typeof(decimal);
    }

    public static bool IsTask(this Type type)
    {
        if (type == null) return false;

        if (type == typeof(Task)) return true;

        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();
            return genericType == typeof(Task<>) ||
                   genericType == typeof(ValueTask<>) ||
                   genericType == typeof(ValueTask);
        }

        return false;
    }

    public static bool IsList(this Type type)
    {
        if (type == null) return false;

        return type.IsGenericType &&
            (type.GetGenericTypeDefinition() == typeof(List<>) ||
             type.GetGenericTypeDefinition() == typeof(IList<>) ||
             type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));
    }

    public static bool IsVector(this Type type)
    {
        if (type == null) return false;

        return type == typeof(Vector2) ||
               type == typeof(Vector3) ||
               type == typeof(Vector4);
    }

    public static bool IsTuple(this Type type)
    {
        if (type == null) return false;

        if (!type.IsGenericType) return false;

        var typeDef = type.GetGenericTypeDefinition();
        return typeDef == typeof(ValueTuple<>) ||
               typeDef == typeof(ValueTuple<,>) ||
               typeDef == typeof(ValueTuple<,,>) ||
               typeDef == typeof(ValueTuple<,,,>) ||
               typeDef == typeof(ValueTuple<,,,,>) ||
               typeDef == typeof(ValueTuple<,,,,,>) ||
               typeDef == typeof(ValueTuple<,,,,,,>) ||
               typeDef == typeof(ValueTuple<,,,,,,,>);
    }

    public static bool IsWrapper(this Type type)
    {
        if (type == null) return false;

        if (typeof(IWrapper).IsAssignableFrom(type))
            return true;

        // For properties/methods that return wrapper types
        if (type.IsClass)
            if (Type.GetType($"{nameof(SomethingNeedDoing.LuaMacro.Wrappers)}.{type.Name}, SomethingNeedDoing") is { } wrapper && typeof(IWrapper).IsAssignableFrom(wrapper))
                return true;
        return false;
    }
}
