using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Documentation;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SomethingNeedDoing.Gui.Tabs;
public class HelpLuaTab(LuaDocumentation luaDocs)
{
    public void DrawTab()
    {
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Lua Scripting");
        ImGui.TextWrapped(
            "SomethingNeedDoing supports Lua scripting for advanced automation. " +
            "Lua scripts can do everything native macros can do and much more.");

        ImGui.Separator();

        foreach (var module in luaDocs.GetModules())
        {
            if (ImGui.CollapsingHeader(module.Key))
            {
                using var font = ImRaii.PushFont(UiBuilder.MonoFont);
                using var _ = ImRaii.PushIndent();

                foreach (var (function, index) in module.Value.WithIndex())
                {
                    using var functionId = ImRaii.PushId(function.FunctionName);

                    var isMethod = function.IsMethod;
                    var isProperty = !isMethod && function.FunctionName.Contains('.');
                    var isField = !isMethod && !isProperty;

                    // display with types but don't copy types
                    var displaySignature = isMethod
                        ? $"{function.FunctionName}({string.Join(", ", function.Parameters.Select(p => $"{p.Name}: {p.Type}"))})"
                        : function.FunctionName;
                    var copySignature = isMethod
                        ? $"{function.FunctionName}({string.Join(", ", function.Parameters.Select(p => p.Name))})"
                        : function.FunctionName;

                    if (isMethod)
                        FunctionText(function.FunctionName, function.Parameters);
                    else
                        ImGuiEx.TextCopy(ImGuiColors.DalamudViolet, displaySignature, copySignature);

                    if (function.ReturnType.TypeName != "void")
                    {
                        ImGui.SameLine();
                        ImGui.TextColored(ImGuiColors.DalamudGrey, $"→ {function.ReturnType}");
                    }

                    if (function.ReturnType.TypeName.EndsWith("Wrapper"))
                        DrawWrapperProperties(function.ReturnType.TypeName, $"{function.FunctionName}_return_{index}", copySignature);
                    else if (function.ReturnType.TypeName == "table" && function.ReturnType.GenericArguments?.Count > 0)
                    {
                        var elementType = function.ReturnType.GenericArguments[0];
                        if (elementType.TypeName.EndsWith("Wrapper"))
                            DrawWrapperProperties(elementType.TypeName, $"{function.FunctionName}_return_{index}", copySignature);
                    }

                    if (function.Examples is { Length: > 0 } examples)
                    {
                        foreach (var ex in examples)
                        {
                            if (!string.IsNullOrEmpty(ex))
                            {
                                ImGui.TextColored(ImGuiColors.DalamudGrey, "Example:");
                                using var exampleColor = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
                                ImGui.TextWrapped(ex);
                            }
                        }
                    }

                    ImGui.Separator();
                }
            }
        }
    }

    private void FunctionText(string functionName, List<(string Name, LuaTypeInfo Type, string? Description)> parameters)
    {
        ImGui.TextColored(ImGuiColors.DalamudViolet, functionName);
        ImGui.SameLine(0, 0);
        ImGui.TextUnformatted("(");
        ImGui.SameLine(0, 0);

        for (var i = 0; i < parameters.Count; i++)
        {
            var (name, type, _) = parameters[i];
            ImGui.TextColored(ImGuiColors.DalamudOrange, type.ToString());
            ImGui.SameLine();
            ImGui.TextUnformatted(name);

            if (i < parameters.Count - 1)
            {
                ImGui.SameLine(0, 0);
                ImGui.Text(", ");
                ImGui.SameLine(0, 0);
            }
        }

        ImGui.SameLine(0, 0);
        ImGui.Text(")");
    }

    private void DrawWrapperProperties(string wrapperTypeName, string id, string parentChain = "")
    {
        var wrapperType = typeof(HelpLuaTab).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name == wrapperTypeName && typeof(IWrapper).IsAssignableFrom(t));

        if (wrapperType == null || !typeof(IWrapper).IsAssignableFrom(wrapperType))
            return;

        var wrapperProperties = wrapperType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttributes(typeof(LuaDocsAttribute), true).Length != 0)
            .ToList();

        var wrapperMethods = wrapperType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(LuaDocsAttribute), true).Length != 0)
            .ToList();

        using var _ = ImRaii.PushId(id);

        if (wrapperProperties.Count > 0 || wrapperMethods.Count > 0)
        {
            using var tree = ImRaii.TreeNode($"Return Value Properties");
            if (!tree) return;
            using var __ = ImRaii.PushIndent();

            if (wrapperProperties.Count > 0)
            {
                ImGui.TextColored(ImGuiColors.DalamudGrey, "Properties:");
                using var ___ = ImRaii.PushIndent();
                foreach (var (prop, index) in wrapperProperties.WithIndex())
                {
                    var docs = prop.GetCustomAttributes(typeof(LuaDocsAttribute), true).Cast<LuaDocsAttribute>().FirstOrDefault();
                    var fullChain = string.IsNullOrEmpty(parentChain) ? prop.Name : $"{parentChain}.{prop.Name}";

                    ImGuiEx.TextCopy(ImGuiColors.DalamudOrange, prop.Name, fullChain);
                    ImGui.SameLine();
                    ImGui.TextColored(ImGuiColors.DalamudGrey, $"→ {LuaTypeConverter.GetLuaType(prop.PropertyType)}");

                    if (docs?.Description != null)
                        ImGui.TextWrapped(docs.Description);

                    if (prop.PropertyType.Name.EndsWith("Wrapper"))
                        DrawWrapperProperties(prop.PropertyType.Name, $"{id}_prop_{index}", fullChain);
                    else if (prop.PropertyType.IsList())
                    {
                        var elementType = prop.PropertyType.GetGenericArguments()[0];
                        if (elementType.Name.EndsWith("Wrapper"))
                            DrawWrapperProperties(elementType.Name, $"{id}_prop_{index}", fullChain);
                    }
                }
            }

            if (wrapperMethods.Count > 0)
            {
                if (wrapperProperties.Count > 0)
                    ImGui.Spacing();

                ImGui.TextColored(ImGuiColors.DalamudGrey, "Methods:");
                using var ___ = ImRaii.PushIndent();
                foreach (var (method, index) in wrapperMethods.WithIndex())
                {
                    var docs = method.GetCustomAttributes(typeof(LuaDocsAttribute), true).Cast<LuaDocsAttribute>().FirstOrDefault();
                    var parameters = method.GetParameters().Select(p => (
                        Name: p.Name ?? "unk",
                        TypeInfo: LuaTypeConverter.GetLuaType(p.ParameterType),
                        Description: (string?)null
                    )).ToList();
                    var copySignature = parameters.Count > 0 ? $"{method.Name}({string.Join(", ", parameters.Select(p => p.TypeInfo.TypeName))})" : method.Name;

                    var fullChain = string.IsNullOrEmpty(parentChain) ? copySignature : $"{parentChain}:{copySignature}";
                    FunctionText(method.Name, parameters);
                    ImGui.SameLine();
                    ImGui.TextColored(ImGuiColors.DalamudGrey, $"→ {LuaTypeConverter.GetLuaType(method.ReturnType)}");

                    if (docs?.Description != null)
                        ImGui.TextWrapped(docs.Description);

                    if (method.ReturnType.Name.EndsWith("Wrapper"))
                        DrawWrapperProperties(method.ReturnType.Name, $"{id}_method_{index}", fullChain);
                    else if (method.ReturnType.IsList())
                    {
                        var elementType = method.ReturnType.GetGenericArguments()[0];
                        if (elementType.Name.EndsWith("Wrapper"))
                            DrawWrapperProperties(elementType.Name, $"{id}_method_{index}", fullChain);
                    }
                }
            }
        }
    }
}
