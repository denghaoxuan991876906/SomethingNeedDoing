using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.Documentation;
/// <summary>
/// Converts C# types to <see cref="LuaTypeInfo"/>.
/// </summary>
public class LuaTypeConverter
{
    public static LuaTypeInfo GetLuaType(Type type)
    {
        if (type == typeof(void))
            return new LuaTypeInfo("nil", "No return value");

        if (type == typeof(bool))
            return new LuaTypeInfo("boolean", "True or false value");

        if (type == typeof(string))
            return new LuaTypeInfo("string", "Text value");

        if (type.IsNumeric())
            return new LuaTypeInfo("number", "Numeric value");

        if (type.IsVector())
        {
            return type.Name switch
            {
                nameof(Vector2) => new LuaTypeInfo("Vector2", "2D vector with x, y components"),
                nameof(Vector3) => new LuaTypeInfo("Vector3", "3D vector with x, y, z components"),
                nameof(Vector4) => new LuaTypeInfo("Vector4", "4D vector with x, y, z, w components"),
                _ => throw new ArgumentException($"Unexpected vector type: {type.Name}")
            };
        }

        if (type.IsList())
        {
            var elementType = type.GetGenericArguments()[0];
            var elementTypeInfo = GetLuaType(elementType);
            return new LuaTypeInfo("table", $"Array of {elementTypeInfo.TypeName}", [elementTypeInfo]);
        }

        if (type.IsWrapper())
            return new LuaTypeInfo(type.Name, $"Wrapper for {type.Name.Replace("Wrapper", "")}");

        if (type.IsTuple())
        {
            var elementTypes = type.GetGenericArguments().Select(GetLuaType).ToList();

            return new LuaTypeInfo("table", "Tuple of values", elementTypes);
        }

        if (type.IsTask())
        {
            if (type.IsGenericType)
            {
                var resultType = type.GetGenericArguments()[0];
                return new LuaTypeInfo("async", "Asynchronous operation", [GetLuaType(resultType)]);
            }
            return new LuaTypeInfo("async", "Asynchronous operation");
        }

        return new LuaTypeInfo("object", "Complex object");
    }
}
