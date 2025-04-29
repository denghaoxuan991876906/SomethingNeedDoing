using FFXIVClientStructs.FFXIV.Component.GUI;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Waits for a specific addon to be visible.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WaitAddonCommand"/> class.
/// </remarks>
public class WaitAddonCommand(string text, string addonName, WaitModifier? wait, MaxWaitModifier? maxWait) : MacroCommandBase(text, wait)
{
    private readonly string addonName = addonName;
    private readonly int _maxWait = maxWait?.MaxWaitMilliseconds > 0 ? maxWait.MaxWaitMilliseconds : 5000;
    private const int CHECK_INTERVAL = 250;

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.WaitForCondition(
            () =>
            {
                var result = false;
                context.RunOnFramework(() =>
                {
                    unsafe
                    {
                        if (TryGetAddonByName<AtkUnitBase>(addonName, out var addon))
                            result = addon->IsVisible && addon->UldManager.LoadedState == AtkLoadState.Loaded;
                    }
                }).Wait();
                return result;
            },
            _maxWait,
            CHECK_INTERVAL
        );

        await PerformWait(token);
    }

    private static readonly Regex Regex = new($@"^/waitaddon\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public override WaitAddonCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = MaxWaitModifier.TryParse(ref text, out var maxWaitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");

        return new WaitAddonCommand(text, nameValue, waitModifier as WaitModifier, maxWaitModifier as MaxWaitModifier);
    }
}
