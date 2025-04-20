using ECommons.Reflection;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Clicks UI elements in game addons.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ClickCommand"/> class.
/// </remarks>
public class ClickCommand(string text, string addonName, string method, string[] mParams) : MacroCommandBase(text)
{
    private readonly string addonName = addonName;
    private string methodName = method;
    private readonly string[] values = mParams;

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.RunOnFramework(() =>
        {
            unsafe
            {
                if (!TryGetAddonByName<AtkUnitBase>(addonName, out var addon))
                    throw new MacroException($"Addon {addonName} not found");
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
                        var element = m.GetFoP<System.Collections.IEnumerable>(subElement).GetEnumerator();
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
        });

        await PerformWait(token);
    }
}
