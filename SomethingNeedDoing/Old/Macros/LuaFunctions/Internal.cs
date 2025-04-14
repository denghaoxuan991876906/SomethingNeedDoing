using SomethingNeedDoing.Old.Misc;
using System.Reflection;

namespace SomethingNeedDoing.Old.Macros.LuaFunctions;

public class Internal
{
    internal static Internal Instance { get; } = new();

    public List<string> ListAllFunctions()
    {
        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var list = new List<string>();
        foreach (var method in methods.Where(x => x.Name is not nameof(ListAllFunctions) or nameof(InternalGetMacroText) && x.DeclaringType != typeof(object)))
        {
            var parameterList = method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue ?? "null") : "")}");
            list.Add($"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameterList)})");
        }
        return list;
    }

    public string? InternalGetMacroText(string name)
    {
        return C
            .GetAllNodes()
            .OfType<MacroNode>()
            .FirstOrDefault(node =>
                string.Equals(node.Name.Trim(), name.Trim(), StringComparison.InvariantCultureIgnoreCase))?
            .Contents
            .Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
            .Select(line => $"  {line}")
            .Join('\n');
    }

    public void SetSNDProperty(string key, string value) => C.SetProperty(key, value);
    //public object? GetSNDProperty(string key) => C.GetProperty(key);
    public bool IsPauseLoopSet() => Service.MacroManager.PauseAtLoop;
    public bool IsStopLoopSet() => Service.MacroManager.StopAtLoop;
    public string GetActiveMacroName() => Service.MacroManager.ActiveMacroName;
    public bool IsMacroRunningOrQueued(string name) => Service.MacroManager.MacroStatus.Any(m => m.Name == name);
}
