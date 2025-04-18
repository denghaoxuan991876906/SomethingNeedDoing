using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.LuaFunctions;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Misc;
using SomethingNeedDoing.Old.Macros.Exceptions;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class RequireCommand : MacroCommand
{
    public static string[] Commands => ["require"];
    public static string Description => "Require a certain effect to be present before continuing.";
    public static string[] Examples => ["/require \"Well Fed\""];

    private const int StatusCheckMaxWait = 1000;
    private const int StatusCheckInterval = 250;

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly uint[] statusIDs;
    private readonly int maxWait;

    private RequireCommand(string text, string statusName, WaitModifier wait, MaxWaitModifier maxWait) : base(text, wait)
    {
        statusName = statusName.ToLowerInvariant();
        var sheet = Svc.Data.GetExcelSheet<Sheets.Status>()!;
        statusIDs = sheet
            .Where(row => row.Name.ExtractText().Equals(statusName, StringComparison.InvariantCultureIgnoreCase))
            .Select(row => row.RowId)
            .ToArray()!;

        this.maxWait = maxWait.Wait == 0
            ? StatusCheckMaxWait
            : maxWait.Wait;
    }

    public static RequireCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = MaxWaitModifier.TryParse(ref text, out var maxWaitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");

        return new RequireCommand(text, nameValue, waitModifier, maxWaitModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        bool IsStatusPresent() => CharacterState.Instance.HasStatusId(statusIDs);

        var hasStatus = await LinearWait(StatusCheckInterval, maxWait, IsStatusPresent, token);

        if (!hasStatus)
            throw new MacroCommandError("Status effect not found");

        await PerformWait(token);
    }
}
