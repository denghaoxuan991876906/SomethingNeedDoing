using ECommons;
using ECommons.Reflection;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using SomethingNeedDoing.Old.Macros.Exceptions;
using SomethingNeedDoing.Old.Misc;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class ClickCommand : MacroCommand
{
    public static string[] Commands => ["click"];
    public static string Description => "Click a pre-defined button in an addon or window.";
    public static string[] Examples => ["/click RecipeNote Synthesize", "/click SelectString Entries[3].Select", "/click RecipeNote Material 0 true"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<click>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string addonName;
    private string methodName;
    private readonly string[] values = [];

    private ClickCommand(string text, string addon, string method, string[] mParams) : base(text)
    {
        addonName = addon;
        methodName = method;
        values = mParams;
    }

    public static unsafe ClickCommand Parse(string text)
    {
        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var clickValue = ExtractAndUnquote(match, "click").Split(' ');
        var addonName = clickValue[0];
        var methodName = clickValue[1];
        var values = clickValue.Skip(2).ToArray();

        return new ClickCommand(text, addonName, methodName, values);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        try
        {
            unsafe
            {
                if (!TryGetAddonByName<AtkUnitBase>(addonName, out var addon)) throw new MacroCommandError($"Addon {addonName} not found.");
                var type = typeof(AddonMaster).GetNestedType(addonName) ?? throw new NullReferenceException($"Type {addonName} not found");
                var m = Activator.CreateInstance(type, [(nint)addon]) ?? throw new InvalidOperationException($"Could not create instance of type {type}");
                if (methodName.Contains('.'))
                {
                    var splitMethod = methodName.Split('.');
                    var subElement = splitMethod[0];
                    if (subElement.EndsWith(']'))
                    {
                        var index = int.Parse(subElement[(subElement.IndexOf('[') + 1)..^1]);
                        Svc.Log.Verbose($"Index: {index}");
                        subElement = subElement[..subElement.IndexOf('[')];
                        Svc.Log.Verbose($"SubElement: {subElement}");
                        var element = m.GetFoP<IEnumerable>(subElement).GetEnumerator();
                        for (var i = 0; i <= index; i++)
                            element.MoveNext();
                        m = element.Current;
                    }
                    else
                        m = m.GetFoP(splitMethod[0]);

                    methodName = splitMethod[1];
                }
                if (m.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).TryGetFirst(x => x.Name == methodName && x.GetParameters().Length == values.Length, out var methodInfo))
                {
                    var methodParams = new object[values.Length];
                    for (var i = 0; i < values.Length; i++)
                    {
                        var input = values[i];
                        var param = methodInfo.GetParameters()[i];
                        if (param.ParameterType == input.GetType())
                            methodParams[i] = input;
                        else
                        {
                            var parseMethod = param.ParameterType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, [input.GetType()]) ?? throw new InvalidOperationException($"Could not find parse method for {input} ({param.ParameterType}) [{i}]");
                            var parsed = parseMethod.Invoke(null, [input]) ?? throw new NullReferenceException($"Failed to parse {input} with {parseMethod.Name}");
                            methodParams[i] = parsed;
                        }
                    }
                    methodInfo.Invoke(m, methodParams);
                }
                else
                    throw new InvalidOperationException($"Could not find method {methodName} with {values.Length} arguments for {addonName} ");
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error(ex, "Unexpected click error");
            throw new MacroCommandError("Unexpected click error", ex);
        }

        await PerformWait(token);
    }
}
