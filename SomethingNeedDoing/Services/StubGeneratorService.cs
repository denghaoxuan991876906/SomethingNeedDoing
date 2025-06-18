using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Documentation;
using System.IO;
using System.Reflection;
using System.Text;

namespace SomethingNeedDoing.Services;

public class StubGeneratorService : IDisposable
{
    public StubGeneratorService(LuaDocumentation luaDocs)
    {
        var output = new StringBuilder();

        output.AppendLine(GenerateSectionHeader("C# OBJECT DEFINITIONS"));
        output.AppendLine(GenerateCSharpStub(luaDocs));

        output.AppendLine(GenerateSectionHeader("ENUM DEFINITIONS"));
        output.AppendLine(GenerateEnumStub(luaDocs));

        output.AppendLine(GenerateSectionHeader("WRAPPER DEFINITIONS"));
        output.AppendLine(GenerateWrapperStub(luaDocs));

        output.AppendLine(GenerateSectionHeader("MODULE DEFINITIONS"));
        output.AppendLine(GenerateModulesStub(luaDocs));

        output.AppendLine(GenerateSectionHeader("IPC DEFINITIONS"));
        output.AppendLine(GenerateIPCStub(luaDocs));

        File.WriteAllText(GetStubPath("snd-stubs.lua"), output.ToString());
    }

    private string GenerateCSharpStub(LuaDocumentation luaDocs)
    {
        var output = new StringBuilder();

        var types = GetAllReferencedTypes(luaDocs)
            .Where(t => t != null && t.IsVector())
            .Distinct();

        foreach (var type in types)
        {
            output.AppendLine($"--- @class {type.Name}");

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var luaType = LuaTypeConverter.GetLuaType(field.FieldType);
                output.AppendLine($"--- @field {field.Name} {luaType}");
            }

            foreach (var prop in properties)
            {
                if (prop.GetIndexParameters().Length > 0)
                    continue;

                var luaType = LuaTypeConverter.GetLuaType(prop.PropertyType);
                output.AppendLine($"--- @field {prop.Name} {luaType}");
            }

            output.AppendLine();
        }

        return output.ToString();
    }

    private string GenerateEnumStub(LuaDocumentation luaDocs)
    {
        var output = new StringBuilder();

        var enums = GetAllReferencedTypes(luaDocs).Where(t => t.IsEnum);

        foreach (var enumType in enums)
        {
            var names = Enum.GetNames(enumType);
            var values = Enum.GetValues(enumType);

            if (names == null || values == null || names.Length != values.Length)
            {
                Svc.Log.Error($"Enum {enumType} has mismatched names and values.");
                continue;
            }

            output.AppendLine($"--- @alias {enumType.Name}");

            for (int i = 0; i < names.Length; i++)
            {
                var numericValue = Convert.ChangeType(values.GetValue(i), Enum.GetUnderlyingType(enumType));
                output.AppendLine($"---| {numericValue} # {names[i]}");
            }
            output.AppendLine();
        }

        return output.ToString();
    }

    private string GenerateWrapperStub(LuaDocumentation luaDocs)
    {
        var output = new StringBuilder();
        Svc.Log.Debug("Generating Lua wrapper stubs...");

        var wrappers = GetAllReferencedTypes(luaDocs).Where(t => t.IsWrapper());

        foreach (var wrapperType in wrappers)
        {
            output.AppendLine($"--- @class {wrapperType.Name}");

            var wrapperProperties = wrapperType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(LuaDocsAttribute), true).Length != 0)
                .ToList();

            foreach (var prop in wrapperProperties)
            {
                if (prop.Name == "Item" && prop.GetIndexParameters() is { Length: > 0 }) continue;

                output.AppendLine($"--- @field {prop.Name} {LuaTypeConverter.GetLuaType(prop.PropertyType)}");
            }

            var wrapperMethods = wrapperType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes(typeof(LuaDocsAttribute), true).Length != 0)
                .ToList();

            foreach (var method in wrapperMethods)
            {
                var parameters = method.GetParameters().Select(p => $"{p.Name}: {LuaTypeConverter.GetLuaType(p.ParameterType)}");

                string signature = $"fun({string.Join(", ", parameters)}): {LuaTypeConverter.GetLuaType(method.ReturnType)}";

                output.AppendLine($"--- @field {method.Name} {signature}");
            }

            output.AppendLine();
        }

        return output.ToString();
    }

    private string GenerateModulesStub(LuaDocumentation luaDocs)
    {
        var output = new StringBuilder();
        Svc.Log.Debug("Generating Lua module stubs...");

        foreach (var module in luaDocs.GetModules())
        {
            if (module.Key == "IPC")
            {
                continue;
            }

            output.AppendLine($"--- @class {module.Key}");

            Svc.Log.Debug($"Registering Lua module: {module.Key}");
            foreach (var doc in module.Value)
            {
                var type = doc.IsMethod ? GetLuaFunctionSignature(doc) : doc.ReturnType.ToString();

                output.AppendLine($"--- @field {doc.FunctionName} {type}");
            }

            output.AppendLine();
            output.AppendLine($"--- @type {module.Key}");
            output.AppendLine($"{module.Key} = {{}}");
            output.AppendLine();
        }

        return output.ToString();
    }

    private string GenerateIPCStub(LuaDocumentation luaDocs)
    {
        var output = new StringBuilder();
        Svc.Log.Debug("Generating Lua IPC stubs...");

        foreach (var module in luaDocs.GetModules())
        {
            if (module.Key != "IPC")
            {
                continue;
            }

            var groupedFunctions = module.Value.GroupBy(f => f.ModuleName.Contains('.') ? f.ModuleName.Split('.')[1] : "Root");

            foreach (var group in groupedFunctions)
            {
                output.AppendLine($"--- @class {group.Key}");

                foreach (var doc in group)
                {
                    var type = doc.IsMethod ? GetLuaFunctionSignature(doc) : doc.ReturnType.ToString();

                    output.AppendLine($"--- @field {doc.FunctionName} {type}");
                }

                output.AppendLine();
            }

            output.AppendLine($"--- @class IPC");
            foreach (var group in groupedFunctions)
            {
                output.AppendLine($"--- @field {group.Key} {group.Key}");
            }

            output.AppendLine();
            output.AppendLine($"--- @type {module.Key}");
            output.AppendLine($"{module.Key} = {{}}");
            output.AppendLine();
        }

        return output.ToString();
    }

    private string GetLuaFunctionSignature(LuaFunctionDoc doc)
    {
        var parameters = doc.Parameters != null && doc.Parameters.Any()
            ? string.Join(", ", doc.Parameters.Select(p => $"{p.Name}: {p.Type}"))
            : "";

        return $"fun({parameters}): {doc.ReturnType}";
    }

    private string GenerateSectionHeader(string title, int totalWidth = 50, char borderChar = '=')
    {
        string borderLine = $"--{new string(borderChar, totalWidth)}--";

        var sb = new StringBuilder();
        sb.AppendLine(borderLine);

        int padding = totalWidth - title.Length;
        int padLeft = padding / 2;
        int padRight = padding - padLeft;

        sb.AppendLine($"--{new string(' ', padLeft)}{title}{new string(' ', padRight)}--");
        sb.AppendLine(borderLine);

        return sb.ToString();
    }

    private IEnumerable<Type> GetAllNestedTypes(Type type)
    {
        var stack = new Stack<Type>();
        var visited = new HashSet<Type>();

        stack.Push(type);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == null || !visited.Add(current))
                continue;

            yield return current;

            if (current.IsGenericType)
            {
                foreach (var arg in current.GetGenericArguments())
                    stack.Push(arg);
            }

            if (current.IsArray)
            {
                stack.Push(current.GetElementType());
            }

            if (Nullable.GetUnderlyingType(current) is Type nullableType)
            {
                stack.Push(nullableType);
            }

            if (current.BaseType != null && current.BaseType != typeof(object))
            {
                stack.Push(current.BaseType);
            }
        }
    }

    private IEnumerable<Type> GetAllReferencedTypes(LuaDocumentation luaDocs)
    {
        var seen = new HashSet<Type>();

        void AddType(Type? type)
        {
            if (type == null) return;

            foreach (var nested in GetAllNestedTypes(type))
            {
                if (seen.Add(nested))
                {
                    if (typeof(IWrapper).IsAssignableFrom(nested))
                    {
                        foreach (var prop in nested.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (prop.GetCustomAttribute<LuaDocsAttribute>() != null)
                                AddType(prop.PropertyType);
                        }

                        foreach (var method in nested.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (method.GetCustomAttribute<LuaDocsAttribute>() == null)
                                continue;

                            AddType(method.ReturnType);
                            foreach (var param in method.GetParameters())
                                AddType(param.ParameterType);
                        }
                    }
                }
            }
        }

        foreach (var module in luaDocs.GetModules())
        {
            foreach (var doc in module.Value)
            {
                AddType(doc.ReturnType.Type);

                if (doc.Parameters != null)
                {
                    foreach (var param in doc.Parameters)
                        AddType(param.Type.Type);
                }
            }
        }

        return seen;
    }

    private string GetStubPath(string filename)
    {
        return Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, filename);
    }

    public void Dispose()
    {
    }
}
