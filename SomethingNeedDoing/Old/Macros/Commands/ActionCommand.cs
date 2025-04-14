using ECommons;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.LuaFunctions;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Macros.Exceptions;
using SomethingNeedDoing.Old.Misc;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class ActionCommand : MacroCommand
{
    public static string[] Commands => ["ac", "action"];
    public static string Description => "Execute an action and wait for the server to respond.";
    public static string[] Examples => ["/ac Groundwork", "/ac \"Tricks of the Trade\""];
    private const int SafeCraftMaxWait = 5000;

    private static readonly Regex Regex = new($@"^/(?:{string.Join("|", Commands)})\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly HashSet<string> CraftingActionNames = [];
    private static readonly HashSet<string> CraftingQualityActionNames = [];

    private readonly string actionName;
    private readonly UnsafeModifier unsafeMod;
    private readonly ConditionModifier conditionMod;

    static ActionCommand()
    {
        PopulateCraftingNames();
        PopulateCraftingQualityActionNames();
    }

    private ActionCommand(string text, string actionName, WaitModifier waitMod, UnsafeModifier unsafeMod, ConditionModifier conditionMod) : base(text, waitMod)
    {
        this.actionName = actionName.ToLowerInvariant();
        this.unsafeMod = unsafeMod;
        this.conditionMod = conditionMod;
    }

    private static ManualResetEvent DataWaiter => Service.GameEventManager.DataAvailableWaiter;

    public static ActionCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = UnsafeModifier.TryParse(ref text, out var unsafeModifier);
        _ = ConditionModifier.TryParse(ref text, out var conditionModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");

        return new ActionCommand(text, nameValue, waitModifier, unsafeModifier, conditionModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        if (!conditionMod.HasCondition())
        {
            Svc.Log.Debug($"Condition skip: {Text}");
            return;
        }

        if (IsCraftingAction(actionName))
        {
            if (C.CraftSkip)
            {
                if (CraftingState.Instance.IsNotCrafting())
                {
                    Svc.Log.Debug($"Not crafting skip: {Text}");
                    return;
                }

                if (CraftingState.Instance.HasMaxProgress())
                {
                    Svc.Log.Debug($"Max progress skip: {Text}");
                    return;
                }
            }

            if (C.QualitySkip && IsSkippableCraftingQualityAction(actionName) && CraftingState.Instance.HasMaxQuality())
            {
                Svc.Log.Debug($"Max quality skip: {Text}");
                return;
            }

            DataWaiter.Reset();

            Old.Service.ChatManager.SendMessage(Text);

            if (C.SmartWait)
            {
                Svc.Log.Debug("Smart wait");

                if (unsafeMod.IsUnsafe)
                {
                    // Pause a moment to let the action begin
                    await Task.Delay(250, token);
                }
                else
                {
                    // Wait for the data update
                    if (!DataWaiter.WaitOne(SafeCraftMaxWait) && C.StopMacroIfActionTimeout)
                        throw new MacroActionTimeoutError("Did not receive a timely response");
                }

                while (Svc.Condition[ConditionFlag.Crafting40])
                    await Task.Delay(250, token);
            }
            else
            {
                await PerformWait(token);

                if (!unsafeMod.IsUnsafe && !DataWaiter.WaitOne(SafeCraftMaxWait) && C.StopMacroIfActionTimeout)
                    throw new MacroActionTimeoutError("Did not receive a timely response");
            }
        }
        else
        {
            Old.Service.ChatManager.SendMessage(Text);
            await PerformWait(token);
        }
    }

    private static bool IsCraftingAction(string name) => CraftingActionNames.Contains(name);
    private static bool IsSkippableCraftingQualityAction(string name) => CraftingQualityActionNames.Contains(name);

    private static void PopulateCraftingNames()
    {
        CraftingActionNames.AddRange(FindRows<Sheets.Action>(x => x.ActionCategory.RowId == 7 && !x.Name.IsEmpty).Select(x => x.Name.ExtractText().ToLowerInvariant()));
        CraftingActionNames.AddRange(FindRows<Sheets.CraftAction>(x => !x.Name.IsEmpty).Select(x => x.Name.ExtractText().ToLowerInvariant()));
    }

    private static void PopulateCraftingQualityActionNames()
    {
        var craftIDs = new uint[]
        {
            100002, 100016, 100031, 100046, 100061, 100076, 100091, 100106, // Basic Touch
            100004, 100018, 100034, 100048, 100064, 100078, 100093, 100109, // Standard Touch
            100411, 100412, 100413, 100414, 100415, 100416, 100417, 100418, // Advanced Touch
            100128, 100129, 100130, 100131, 100132, 100133, 100134, 100135, // Precise Touch
            100227, 100228, 100229, 100230, 100231, 100232, 100233, 100234, // Prudent Touch
            100243, 100244, 100245, 100246, 100247, 100248, 100249, 100250, // Focused Touch
            // 100283, 100284, 100285, 100286, 100287, 100288, 100289, 100290, // Trained Eye
            100299, 100300, 100301, 100302, 100303, 100304, 100305, 100306, // Preparatory Touch
            100339, 100340, 100341, 100342, 100343, 100344, 100345, 100346, // Byregot's Blessing
            100355, 100356, 100357, 100358, 100359, 100360, 100361, 100362, // Hasty Touch
            100435, 100436, 100437, 100438, 100439, 100440, 100441, 100442, // Trained Finesse
            // 100387, 100388, 100389, 100390, 100391, 100392, 100393, 100394, // Reflect
            // 100323, 100324, 100325, 100326, 100327, 100328, 100329, 100330, // Delicate Synthesis
        };

        var actionIDs = new uint[]
        {
            19004, 19005, 19006, 19007, 19008, 19009, 19010, 19011, // Innovation
            260, 261, 262, 263, 264, 265, 266, 267, // Great Strides
        };

        var actions = Svc.Data.GetExcelSheet<Sheets.Action>()!;
        foreach (var actionID in actionIDs)
        {
            var name = actions.GetRow(actionID)!.Name.ExtractText().ToLowerInvariant();
            CraftingQualityActionNames.Add(name);
        }

        var craftActions = Svc.Data.GetExcelSheet<Sheets.CraftAction>()!;
        foreach (var craftID in craftIDs)
        {
            var name = craftActions.GetRow(craftID)!.Name.ExtractText().ToLowerInvariant();
            CraftingQualityActionNames.Add(name);
        }
    }
}
