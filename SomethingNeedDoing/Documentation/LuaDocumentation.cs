using SomethingNeedDoing.Framework.Interfaces;
using SomethingNeedDoing.LuaMacro;
using System.Reflection;

namespace SomethingNeedDoing.Documentation;
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
            if (method.GetCustomAttribute<LuaFunctionAttribute>() is not { } attr) continue;

            var methodParams = method.GetParameters();
            var parameters = new List<(string Name, LuaTypeInfo Type, string? Description)>();

            for (var i = 0; i < methodParams.Length; i++)
            {
                var param = methodParams[i];
                var paramType = LuaTypeConverter.GetLuaType(param.ParameterType);
                var paramDesc = attr.ParameterDescriptions?.Length > i ? attr.ParameterDescriptions[i] : null;

                parameters.Add((param.Name ?? $"param{i}", paramType, paramDesc));
            }

            var returnType = LuaTypeConverter.GetLuaType(method.ReturnType);

            var description = attr.Description ?? method.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description;

            var modulePath = module is LuaModuleBase baseModule ? baseModule.GetModulePath() : module.ModuleName;

            var doc = new LuaFunctionDoc(
                modulePath,
                attr.Name ?? method.Name,
                description,
                returnType,
                parameters,
                attr.Examples
            );

            docs.Add(doc);

            // If the return type is a wrapper class, register its properties and methods
            if (method.ReturnType.IsClass && method.ReturnType.IsNested)
            {
                // Register properties
                var wrapperProperties = method.ReturnType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in wrapperProperties)
                {
                    if (prop.GetCustomAttribute<LuaDocsAttribute>() is not { } wrapperAttr) continue;

                    var propType = LuaTypeConverter.GetLuaType(prop.PropertyType);
                    var propDoc = new LuaFunctionDoc(
                        modulePath,
                        $"{attr.Name ?? method.Name}.{wrapperAttr.Name ?? prop.Name}",
                        wrapperAttr.Description ?? $"Property of {method.ReturnType.Name}",
                        propType,
                        [],
                        wrapperAttr.Examples
                    );
                    docs.Add(propDoc);
                }

                // Register methods
                var wrapperMethods = method.ReturnType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var wrapperMethod in wrapperMethods)
                {
                    if (wrapperMethod.GetCustomAttribute<LuaDocsAttribute>() is not { } wrapperAttr) continue;

                    var wrapperMethodParams = wrapperMethod.GetParameters();
                    var wrapperParameters = new List<(string Name, LuaTypeInfo Type, string? Description)>();

                    for (var i = 0; i < wrapperMethodParams.Length; i++)
                    {
                        var param = wrapperMethodParams[i];
                        var paramType = LuaTypeConverter.GetLuaType(param.ParameterType);
                        var paramDesc = wrapperAttr.ParameterDescriptions?.Length > i ? wrapperAttr.ParameterDescriptions[i] : null;

                        wrapperParameters.Add((param.Name ?? $"param{i}", paramType, paramDesc));
                    }

                    var wrapperReturnType = LuaTypeConverter.GetLuaType(wrapperMethod.ReturnType);
                    var wrapperDoc = new LuaFunctionDoc(
                        modulePath,
                        $"{attr.Name ?? method.Name}.{wrapperAttr.Name ?? wrapperMethod.Name}",
                        wrapperAttr.Description ?? $"Method of {method.ReturnType.Name}",
                        wrapperReturnType,
                        wrapperParameters,
                        wrapperAttr.Examples
                    );
                    docs.Add(wrapperDoc);
                }
            }
        }

        _documentation[module.ModuleName] = docs;
    }

    public Dictionary<string, List<LuaFunctionDoc>> GetModules() => _documentation;
}
