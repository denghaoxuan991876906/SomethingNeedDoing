using ECommons.Automation;
using ECommons.UIHelpers.AddonMasterImplementations;
using Lumina.Excel.Sheets;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Executes game actions and waits for their completion.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ActionCommand"/> class.
/// </remarks>
[GenericDoc(
    "Execute a game action",
    ["actionName"],
    ["/action \"Basic Synthesis\"", "/action \"Basic Touch\" <condition.good>", "/action \"Basic Synthesis\" <errorif.actiontimeout>"]
)]
public class ActionCommand(string text, string actionName) : MacroCommandBase(text)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;
    private bool awaitCraftAction;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        awaitCraftAction = false;
        Svc.Log.Debug($"ActionCommand.Execute: Starting {actionName}, UnsafeModifier: {UnsafeModifier != null}, WaitDuration: {WaitDuration}");

        // Simple execution if a wait is present
        if (WaitDuration > 0)
        {
            await context.RunOnFramework(() => Chat.SendMessage(CommandText));
            await PerformWait(token);
            return;
        }

        var craftingComplete = new TaskCompletionSource<bool>();
        void OnConditionChange(ConditionFlag flag, bool value)
        {
            if (flag == ConditionFlag.ExecutingCraftingAction && !value)
                craftingComplete.TrySetResult(true);
        }

        await context.RunOnFramework(() =>
        {
            if (Game.Crafting.GetCondition() is not AddonMaster.Synthesis.Condition.Normal && ConditionModifier?.Conditions.FirstOrDefault() is { } cnd)
            {
                if (Game.Crafting.GetCondition().ToString().EqualsIgnoreCase(cnd))
                {
                    Svc.Log.Debug($"Condition skip: {CommandText}");
                    return;
                }
            }

            if (IsCraftAction(actionName))
            {
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

                if (UnsafeModifier is null)
                {
                    awaitCraftAction = true;
                    Svc.Condition.ConditionChange += OnConditionChange;
                }
            }

            Chat.SendMessage(CommandText);
        });

        if (awaitCraftAction)
        {
            try
            {
                using var cts = new CancellationTokenSource(5000); // 5 second timeout
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);
                await craftingComplete.Task.WaitAsync(linkedCts.Token);
            }
            catch (TimeoutException)
            {
                if (ErrorIfModifier?.Condition == Modifiers.ErrorCondition.ActionTimeout)
                    throw new MacroTimeoutException("Did not receive a timely response");
            }
            finally
            {
                await context.RunOnFramework(() => Svc.Condition.ConditionChange -= OnConditionChange);
            }
        }
    }

    private bool IsCraftAction(string name) => FindRows<CraftAction>(x => !x.Name.IsEmpty).Select(x => x.Name).Distinct().Contains(name);
    private bool IsSkippableCraftingQualityAction(string name) => CraftingQualityActionNames.ContainsIgnoreCase(name);

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
}
