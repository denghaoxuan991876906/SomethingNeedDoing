namespace SomethingNeedDoing.Documentation;
/// <summary>
/// Converts C# types to <see cref="LuaTypeInfo"/>.
/// </summary>
public class LuaTypeConverter
{
    public static LuaTypeInfo GetLuaType(Type type)
    {
        if (type == typeof(void))
            return new LuaTypeInfo("nil", "No return value", null, type);

        if (type == typeof(string))
            return new LuaTypeInfo("string", "Text value", null, type);

        if (type == typeof(bool))
            return new LuaTypeInfo("boolean", "True or false value", null, type);

        if (type.IsNumeric())
            return new LuaTypeInfo("number", "Numeric value", null, type);

        if (type.IsEnum)
            return new LuaTypeInfo(type.Name, $"Enumeration of {type.Name} values", null, type);

        if (type.IsVector())
        {
            return type.Name switch
            {
                nameof(Vector2) => new LuaTypeInfo("Vector2", "2D vector with x, y components", null, type),
                nameof(Vector3) => new LuaTypeInfo("Vector3", "3D vector with x, y, z components", null, type),
                nameof(Vector4) => new LuaTypeInfo("Vector4", "4D vector with x, y, z, w components", null, type),
                _ => throw new ArgumentException($"Unexpected vector type: {type.Name}")
            };
        }

        if (type.IsList())
        {
            var elementType = type.GetGenericArguments()[0];
            var elementTypeInfo = elementType.IsWrapper()
                ? new LuaTypeInfo(elementType.Name, $"Wrapper for {elementType.Name.Replace("Wrapper", "")}", null, elementType)
                : GetLuaType(elementType);
            return new LuaTypeInfo("table", $"Array of {elementTypeInfo.TypeName}", [elementTypeInfo], type);
        }

        if (type.IsWrapper())
            return new LuaTypeInfo(type.Name, $"Wrapper for {type.Name.Replace("Wrapper", "")}", null, type);

        if (type.IsTuple())
            return new LuaTypeInfo("table", "Tuple of values", [.. type.GetGenericArguments().Select(GetLuaType)], type);

        if (type.IsTask())
        {
            if (type.IsGenericType)
            {
                var resultType = type.GetGenericArguments()[0];
                return new LuaTypeInfo("async", "Asynchronous operation", [GetLuaType(resultType)], type);
            }
            return new LuaTypeInfo("async", "Asynchronous operation", null, type);
        }

        return new LuaTypeInfo("object", "Complex object", null, type);
    }
}
