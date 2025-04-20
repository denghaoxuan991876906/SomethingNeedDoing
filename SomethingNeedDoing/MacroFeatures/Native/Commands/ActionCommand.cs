using ECommons.Automation;
using ECommons.UIHelpers.AddonMasterImplementations;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Executes game actions and waits for their completion.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ActionCommand"/> class.
/// </remarks>
public class ActionCommand(string text, string actionName, WaitModifier? waitMod, UnsafeModifier? unsafeMod, ConditionModifier? conditionMod) : MacroCommandBase(text, waitMod?.WaitDuration ?? 0)
{
    private readonly string actionName = actionName.ToLowerInvariant();
    private readonly bool unsafeMode = unsafeMod != null;
    private readonly string? condition = conditionMod?.Conditions.FirstOrDefault();

    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        var craftingComplete = new TaskCompletionSource<bool>();

        void OnConditionChange(ConditionFlag flag, bool value)
        {
            if (flag == ConditionFlag.Crafting40 && !value)
                craftingComplete.TrySetResult(true);
        }

        // Must run on framework thread since it accesses game state
        await context.RunOnFramework(() =>
        {
            if (Game.Crafting.GetCondition() is not AddonMaster.Synthesis.Condition.Normal && condition is { })
            {
                if (Game.Crafting.GetCondition().ToString().EqualsIgnoreCase(condition))
                {
                    Svc.Log.Debug($"Condition skip: {CommandText}");
                    return;
                }
            }

            if (C.CraftSkip)
            {
                if (!Game.Crafting.IsCrafting())
                {
                    Svc.Log.Debug($"Not crafting skip: {CommandText}");
                    return;
                }

                if (Game.Crafting.IsMaxProgress())
                {
                    Svc.Log.Debug($"Max progress skip: {CommandText}");
                    return;
                }
            }

            if (C.QualitySkip && IsSkippableCraftingQualityAction(actionName) && Game.Crafting.IsMaxProgress())
            {
                Svc.Log.Debug($"Max quality skip: {CommandText}");
                return;
            }

            if (!unsafeMode)
                Svc.Condition.ConditionChange += OnConditionChange;

            Chat.Instance.SendMessage(CommandText);
        });

        if (!unsafeMode)
        {
            try
            {
                using var cts = new CancellationTokenSource(5000); // 5 second timeout
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);
                await craftingComplete.Task.WaitAsync(linkedCts.Token);
            }
            catch (TimeoutException)
            {
                if (C.StopMacroIfActionTimeout)
                    throw new MacroTimeoutException("Did not receive a timely response");
            }
            finally
            {
                await context.RunOnFramework(() =>
                    Svc.Condition.ConditionChange -= OnConditionChange);
            }
        }

        await PerformWait(token);
    }

    private bool IsSkippableCraftingQualityAction(string name) => CraftingQualityActionNames.Contains(name);

    private static readonly HashSet<string> CraftingQualityActionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // Basic Touch variants
        "Basic Touch",
        "Standard Touch",
        "Advanced Touch",
        "Precise Touch",
        "Prudent Touch",
        "Focused Touch",
        "Preparatory Touch",
        "Trained Finesse",
        // Buffs
        "Innovation",
        "Great Strides",
        // Finishers
        "Byregot's Blessing"
    };

    /// <summary>
    /// Parses an action command from text.
    /// </summary>
    public static ActionCommand Parse(string text)
    {

        _ = WaitModifier.TryParse(ref text, out var waitMod);
        _ = UnsafeModifier.TryParse(ref text, out var unsafeMod);
        _ = ConditionModifier.TryParse(ref text, out var conditionMod);

        var match = Regex.Match(text, @"^/(?:ac|action)\s+(?<name>.*?)\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = match.Groups["name"].Value.Trim('"');

        return new(text, nameValue, waitMod as WaitModifier, unsafeMod as UnsafeModifier, conditionMod as ConditionModifier);
    }
}
