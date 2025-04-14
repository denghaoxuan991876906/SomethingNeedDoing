using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Misc;
using SomethingNeedDoing.Old.Macros.Exceptions;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class WaitAddonCommand : MacroCommand
{
    public static string[] Commands => ["waitaddon"];
    public static string Description => "Wait for an addon, otherwise known as a UI component to be present. You can discover these names by using the \"Addon Inspector\" view inside the \"/xldata\" window.";
    public static string[] Examples => ["/waitaddon RecipeNote"];

    private const int AddonCheckMaxWait = 5000;
    private const int AddonCheckInterval = 500;

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string addonName;
    private readonly int maxWait;

    private WaitAddonCommand(string text, string addonName, WaitModifier wait, MaxWaitModifier maxWait) : base(text, wait)
    {
        this.addonName = addonName;
        this.maxWait = maxWait.Wait == 0 ? AddonCheckMaxWait : maxWait.Wait;
    }

    public static WaitAddonCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = MaxWaitModifier.TryParse(ref text, out var maxWaitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");

        return new WaitAddonCommand(text, nameValue, waitModifier, maxWaitModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        var (addonPtr, isVisible) = await LinearWait(AddonCheckInterval, maxWait, IsAddonVisible, token);

        if (addonPtr == nint.Zero && C.StopMacroIfAddonNotFound)
            throw new MacroCommandError("Addon not found");

        if (!isVisible && C.StopMacroIfAddonNotVisible)
            throw new MacroCommandError("Addon not visible");

        await PerformWait(token);
    }

    private unsafe (nint Addon, bool IsVisible) IsAddonVisible()
    {
        var addonPtr = Svc.GameGui.GetAddonByName(addonName, 1);
        if (addonPtr == nint.Zero)
            return (addonPtr, false);

        var addon = (AtkUnitBase*)addonPtr;
        return !addon->IsVisible || addon->UldManager.LoadedState != AtkLoadState.Loaded ? ((nint Addon, bool IsVisible))(addonPtr, false) : ((nint Addon, bool IsVisible))(addonPtr, true);
    }
}
