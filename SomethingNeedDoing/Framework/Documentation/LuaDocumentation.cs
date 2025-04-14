using System.Collections;
using System.Reflection;
using System.Text;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Provides documentation for Lua modules and functions.
/// </summary>
public class LuaDocumentation
{
    private readonly Dictionary<string, List<LuaFunctionDoc>> _documentation = [];

    public void RegisterModule(ILuaModule module)
    {
        var docs = new List<LuaFunctionDoc>();

        foreach (var method in module.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            var attr = method.GetCustomAttribute<LuaFunctionAttribute>();
            if (attr == null) continue;

            // Get parameter information
            var methodParams = method.GetParameters();
            var parameters = new List<(string Name, LuaTypeInfo Type, string? Description)>();

            for (var i = 0; i < methodParams.Length; i++)
            {
                var param = methodParams[i];
                var paramType = LuaTypeConverter.GetLuaType(param.ParameterType);
                var paramDesc = attr.ParameterDescriptions?.Length > i ? attr.ParameterDescriptions[i] : null;

                parameters.Add((param.Name ?? $"param{i}", paramType, paramDesc));
            }

            // Get return type information
            var returnType = LuaTypeConverter.GetLuaType(method.ReturnType);

            // Get description from either LuaFunction attribute or Description attribute
            var description = attr.Description ??
                method.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description;

            var doc = new LuaFunctionDoc(
                module.ModuleName,
                attr.Name ?? method.Name,
                description,
                returnType,
                parameters,
                attr.Examples
            );

            docs.Add(doc);
        }

        _documentation[module.ModuleName] = docs;
    }

    public string GenerateMarkdown()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Lua API Documentation");
        sb.AppendLine();

        foreach (var (moduleName, functions) in _documentation.OrderBy(x => x.Key))
        {
            sb.AppendLine($"## {moduleName}");
            sb.AppendLine();

            foreach (var func in functions.OrderBy(x => x.FunctionName))
            {
                sb.AppendLine($"### {func.FunctionName}");
                sb.AppendLine();

                if (func.Description != null)
                {
                    sb.AppendLine(func.Description);
                    sb.AppendLine();
                }

                sb.AppendLine("#### Syntax");
                sb.AppendLine("```lua");
                var paramList = string.Join(", ", func.Parameters.Select(p => p.Name));
                sb.AppendLine($"{moduleName}.{func.FunctionName}({paramList})");
                sb.AppendLine("```");
                sb.AppendLine();

                if (func.Parameters.Any())
                {
                    sb.AppendLine("#### Parameters");
                    foreach (var (name, type, desc) in func.Parameters)
                    {
                        var typeStr = type.ToString();
                        if (type.GenericArguments?.Count > 0)
                        {
                            // Add more detailed type information for complex types
                            typeStr += " - " + string.Join(", ", type.GenericArguments.Select(t => t.ToString()));
                        }
                        sb.AppendLine($"- `{name}` ({typeStr}){(desc != null ? $": {desc}" : "")}");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("#### Returns");
                var returnTypeStr = func.ReturnType.ToString();
                if (func.ReturnType.GenericArguments?.Count > 0)
                {
                    returnTypeStr += "\n\nGeneric type parameters:";
                    foreach (var genericType in func.ReturnType.GenericArguments)
                    {
                        returnTypeStr += $"\n- {genericType}";
                    }
                }
                sb.AppendLine($"`{returnTypeStr}`");
                sb.AppendLine();

                if (func.Examples?.Any() == true)
                {
                    sb.AppendLine("#### Examples");
                    foreach (var example in func.Examples)
                    {
                        sb.AppendLine("```lua");
                        sb.AppendLine(example);
                        sb.AppendLine("```");
                    }
                    sb.AppendLine();
                }
            }
        }

        // Add type reference section
        sb.AppendLine("## Type Reference");
        sb.AppendLine();

        // Vector types
        sb.AppendLine("### Vector Types");
        sb.AppendLine();
        sb.AppendLine("#### Vector2");
        sb.AppendLine("Two-dimensional vector with x and y components.");
        sb.AppendLine("```lua");
        sb.AppendLine("-- Accessing vector components");
        sb.AppendLine("local v = someFunction()");
        sb.AppendLine("print(v.x, v.y)");
        sb.AppendLine("```");

        sb.AppendLine("#### Vector3");
        sb.AppendLine("Three-dimensional vector with x, y, and z components.");
        sb.AppendLine("```lua");
        sb.AppendLine("-- Accessing vector components");
        sb.AppendLine("local v = someFunction()");
        sb.AppendLine("print(v.x, v.y, v.z)");
        sb.AppendLine("```");

        // Lists
        sb.AppendLine("### Lists");
        sb.AppendLine("Lists are represented as Lua tables with numeric indices (1-based).");
        sb.AppendLine("```lua");
        sb.AppendLine("-- Iterating over a list");
        sb.AppendLine("local list = someFunction()");
        sb.AppendLine("for i, value in ipairs(list) do");
        sb.AppendLine("    print(i, value)");
        sb.AppendLine("end");
        sb.AppendLine("```");

        // Tuples
        sb.AppendLine("### Tuples");
        sb.AppendLine("Tuples are represented as Lua tables and can be unpacked into multiple values.");
        sb.AppendLine("```lua");
        sb.AppendLine("-- Unpacking a tuple");
        sb.AppendLine("local first, second = table.unpack(someFunction())");
        sb.AppendLine("```");

        // Async operations
        sb.AppendLine("### Async Operations");
        sb.AppendLine("Functions that return an async result will automatically wait for completion.");
        sb.AppendLine("```lua");
        sb.AppendLine("-- Async function call");
        sb.AppendLine("local result = someAsyncFunction()  -- Automatically waits for result");
        sb.AppendLine("```");

        return sb.ToString();
    }

    public void GenerateInGameHelp()
    {
        foreach (var (moduleName, functions) in _documentation)
        {
            foreach (var func in functions)
            {
                var helpText = new StringBuilder();
                helpText.AppendLine($"{moduleName}.{func.FunctionName}");

                if (func.Description != null)
                    helpText.AppendLine($"\n{func.Description}");

                if (func.Parameters.Count > 0)
                {
                    helpText.AppendLine("\nParameters:");
                    foreach (var (name, type, desc) in func.Parameters)
                    {
                        var typeStr = type.ToString();
                        if (type.GenericArguments?.Count > 0)
                        {
                            typeStr += $" <{string.Join(", ", type.GenericArguments.Select(t => t.ToString()))}>";
                        }
                        helpText.AppendLine($"  {name} ({typeStr}){(desc != null ? $": {desc}" : "")}");
                    }
                }

                var returnTypeStr = func.ReturnType.ToString();
                if (func.ReturnType.GenericArguments?.Count > 0)
                {
                    returnTypeStr += $" <{string.Join(", ", func.ReturnType.GenericArguments.Select(t => t.ToString()))}>";
                }
                helpText.AppendLine($"\nReturns: {returnTypeStr}");

                if (func.Examples?.Length > 0)
                {
                    helpText.AppendLine("\nExamples:");
                    foreach (var example in func.Examples)
                        helpText.AppendLine($"  {example}");
                }

                Svc.Chat.Print(helpText.ToString());
            }
        }
    }
}
