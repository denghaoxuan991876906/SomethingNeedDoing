namespace SomethingNeedDoing.Framework;
/// <summary>
/// Represents documentation for a Lua function.
/// </summary>
public class LuaFunctionDoc(string moduleName, string functionName, string? description, LuaTypeInfo returnType, List<(string Name, LuaTypeInfo Type, string? Description)> parameters, string[]? examples)
{
    public string ModuleName { get; } = moduleName;
    public string FunctionName { get; } = functionName;
    public string? Description { get; } = description;
    public LuaTypeInfo ReturnType { get; } = returnType;
    public List<(string Name, LuaTypeInfo Type, string? Description)> Parameters { get; } = parameters;
    public string[]? Examples { get; } = examples;
}
