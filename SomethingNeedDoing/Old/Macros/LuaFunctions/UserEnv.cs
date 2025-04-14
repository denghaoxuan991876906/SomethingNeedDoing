using ECommons.Reflection;
using FFXIVClientStructs.FFXIV.Client.UI;
using System.Reflection;

namespace SomethingNeedDoing.Old.Macros.LuaFunctions;

public class UserEnv
{
    internal static UserEnv Instance { get; } = new();

    public List<string> ListAllFunctions()
    {
        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var list = new List<string>();
        foreach (var method in methods.Where(x => x.Name != nameof(ListAllFunctions) && x.DeclaringType != typeof(object)))
        {
            var parameterList = method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue ?? "null") : "")}");
            list.Add($"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameterList)})");
        }
        return list;
    }

    public string GetClipboard() => ImGui.GetClipboardText();

    public void SetClipboard(string text) => ImGui.SetClipboardText(text);

    public unsafe void CrashTheGame() => FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->UIModule = (UIModule*)0;

    public void LogInfo(object text) => Svc.Log.Info($"{text}");
    public void LogDebug(object text) => Svc.Log.Debug($"{text}");
    public void LogVerbose(object text) => Svc.Log.Verbose($"{text}");

    public bool HasPlugin(string name) => DalamudReflector.TryGetDalamudPlugin(name, out _, false, true);
    public System.Version GetPluginVersion(string name) => Svc.PluginInterface.InstalledPlugins.FirstOrDefault(p => p?.InternalName == name, null)?.Version ?? new System.Version(0, 0);
}
