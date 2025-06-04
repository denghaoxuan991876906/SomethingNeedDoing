using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using SomethingNeedDoing.Documentation;
using SomethingNeedDoing.Core.Interfaces;
using System.Reflection;

namespace SomethingNeedDoing.Gui.Tabs;
public class HelpLuaTab(LuaDocumentation luaDocs)
{
    public void DrawTab()
    {
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        // Introduction to Lua
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Lua Scripting");
        ImGui.TextWrapped(
            "SomethingNeedDoing supports Lua scripting for advanced automation. " +
            "Lua scripts can do everything native macros can do and much more.");

        ImGui.Separator();

        // Basic syntax and usage
        if (ImGui.CollapsingHeader("Basic Lua Usage", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.TextWrapped(@"
Lua scripts work by yielding commands back to the macro engine.

For example:

yield(""/ac Muscle memory <wait.3>"")
yield(""/ac Precise touch <wait.2>"")
yield(""/echo done!"")
...and so on.

You can also use regular Lua syntax for complex logic:

for i = 1, 5 do
    yield(""/echo Loop iteration "" .. i)
    if i == 3 then
        yield(""/echo Halfway done!"")
    end
end");
        }

        ImGui.Separator();

        // Draw registered Lua modules and functions
        foreach (var module in luaDocs.GetModules())
        {
            if (ImGui.CollapsingHeader(module.Key))
            {
                ImGui.Indent();

                // Module functions
                foreach (var function in module.Value)
                {
                    using var functionId = ImRaii.PushId(function.FunctionName);

                    // Function signature
                    var signature = function.Parameters.Count > 0
                        ? $"{function.FunctionName}({string.Join(", ", function.Parameters.Select(p => p.Name))})"
                        : function.FunctionName;

                    // Determine if it's a method, property, or field based on parameters and return type
                    var isMethod = function.Parameters.Count > 0 || function.FunctionName.Contains('(');
                    var isProperty = !isMethod && function.FunctionName.Contains('.');
                    var isField = !isMethod && !isProperty;

                    // Display type indicator
                    if (isMethod)
                        ImGui.TextColored(ImGuiColors.DalamudGrey, "Method:");
                    else if (isProperty)
                        ImGui.TextColored(ImGuiColors.DalamudGrey, "Property:");
                    else
                        ImGui.TextColored(ImGuiColors.DalamudGrey, "Field:");

                    // Display signature
                    ImGui.TextColored(ImGuiColors.DalamudViolet, signature);

                    // Display return type
                    if (function.ReturnType.TypeName != "void")
                    {
                        ImGui.SameLine();
                        ImGui.TextColored(ImGuiColors.DalamudOrange, $"→ {function.ReturnType.TypeName}");
                    }

                    // Function description
                    if (!string.IsNullOrEmpty(function.Description))
                    {
                        ImGui.TextWrapped(function.Description);
                    }

                    // Parameters
                    if (function.Parameters.Count > 0)
                    {
                        ImGui.TextColored(ImGuiColors.DalamudGrey, "Parameters:");
                        ImGui.Indent();
                        foreach (var param in function.Parameters)
                        {
                            ImGui.TextColored(ImGuiColors.DalamudOrange, param.Name);
                            ImGui.SameLine();
                            ImGui.TextWrapped($"- {param.Description ?? "No description available"}");
                        }
                        ImGui.Unindent();
                    }

                    // If this returns a wrapper type, show its properties in a collapsible section
                    if (function.ReturnType.TypeName.EndsWith("Wrapper"))
                    {
                        DrawWrapperProperties(function.ReturnType.TypeName);
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

                ImGui.Unindent();
            }
        }
    }

    private void DrawWrapperProperties(string wrapperTypeName)
    {
        var wrapperType = Type.GetType($"SomethingNeedDoing.LuaMacro.Wrappers.{wrapperTypeName}, SomethingNeedDoing");
        if (wrapperType == null || !typeof(IWrapper).IsAssignableFrom(wrapperType))
            return;

        var wrapperProperties = wrapperType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttributes(typeof(LuaDocsAttribute), true).Length != 0)
            .ToList();

        var wrapperMethods = wrapperType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(LuaDocsAttribute), true).Length != 0)
            .ToList();

        if (wrapperProperties.Count > 0 || wrapperMethods.Count > 0)
        {
            using var tree = ImRaii.TreeNode("Wrapper Properties");
            if (!tree) return;
            //if (ImGui.TreeNode("Wrapper Properties"))
            //{
            ImGui.Indent();

            if (wrapperProperties.Count > 0)
            {
                ImGui.TextColored(ImGuiColors.DalamudGrey, "Properties:");
                ImGui.Indent();
                foreach (var prop in wrapperProperties)
                {
                    var docs = prop.GetCustomAttributes(typeof(LuaDocsAttribute), true)
                        .Cast<LuaDocsAttribute>()
                        .FirstOrDefault();

                    ImGui.TextColored(ImGuiColors.DalamudOrange, prop.Name);
                    ImGui.SameLine();
                    ImGui.TextColored(ImGuiColors.DalamudGrey, $"→ {GetLuaTypeName(prop.PropertyType)}");

                    if (docs?.Description != null)
                        ImGui.TextWrapped(docs.Description);

                    if (prop.PropertyType.Name.EndsWith("Wrapper"))
                        DrawWrapperProperties(prop.PropertyType.Name);
                }
                ImGui.Unindent();
            }

            if (wrapperMethods.Count > 0)
            {
                if (wrapperProperties.Count > 0)
                    ImGui.Spacing();

                ImGui.TextColored(ImGuiColors.DalamudGrey, "Methods:");
                ImGui.Indent();
                foreach (var method in wrapperMethods)
                {
                    var docs = method.GetCustomAttributes(typeof(LuaDocsAttribute), true)
                        .Cast<LuaDocsAttribute>()
                        .FirstOrDefault();

                    var parameters = method.GetParameters();
                    var methodSignature = parameters.Length > 0
                        ? $"{method.Name}({string.Join(", ", parameters.Select(p => p.Name))})"
                        : method.Name;

                    ImGui.TextColored(ImGuiColors.DalamudOrange, methodSignature);
                    ImGui.SameLine();
                    ImGui.TextColored(ImGuiColors.DalamudGrey, $"→ {GetLuaTypeName(method.ReturnType)}");

                    if (docs?.Description != null)
                        ImGui.TextWrapped(docs.Description);

                    if (method.ReturnType.Name.EndsWith("Wrapper"))
                        DrawWrapperProperties(method.ReturnType.Name);
                }
                ImGui.Unindent();
            }

            ImGui.Unindent();
            //ImGui.TreePop();
            //}
        }
    }

    private static string GetLuaTypeName(Type type)
    {
        if (type == typeof(void)) return "nil";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(string)) return "string";
        if (type.IsNumeric()) return "number";
        if (type.IsList()) return "table";
        if (type.IsTask()) return "async";
        if (typeof(IWrapper).IsAssignableFrom(type)) return type.Name;
        return "object";
    }
}
