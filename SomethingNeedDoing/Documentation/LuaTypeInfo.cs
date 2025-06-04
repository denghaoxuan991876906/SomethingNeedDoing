namespace SomethingNeedDoing.Documentation;
/// <summary>
/// Provides detailed Lua type information.
/// </summary>
public class LuaTypeInfo(string typeName, string description, List<LuaTypeInfo>? genericArguments = null, Type? type = null)
{
    public string TypeName { get; } = typeName;
    public string Description { get; } = description;
    public List<LuaTypeInfo>? GenericArguments { get; } = genericArguments;
    public Type? Type { get; } = type;

    public override string ToString()
        => GenericArguments == null || GenericArguments.Count == 0 ? TypeName : $"{TypeName}<{string.Join(", ", GenericArguments)}>";
}
