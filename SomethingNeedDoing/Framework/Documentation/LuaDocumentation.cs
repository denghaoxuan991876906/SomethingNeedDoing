using System.Reflection;

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
        }

        _documentation[module.ModuleName] = docs;
    }
}
