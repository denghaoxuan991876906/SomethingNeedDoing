using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Uses an item from the inventory.
/// </summary>
[GenericDoc(
    "Use an item from your inventory",
    ["itemName"],
    ["/item \"Potion\"", "/item \"Potion\" <hq>", "/item \"Potion\" <errorif.itemnotfound>"]
)]
public class ItemCommand(string text, string itemName) : MacroCommandBase(text)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        var itemId = SearchItemId(itemName);
        Svc.Log.Debug($"Item found: {itemId}");

        var count = Game.GetInventoryItemCount(itemId, ItemQualityModifier?.IsHighQuality ?? false);
        Svc.Log.Debug($"Item Count: {count}");

        if (count == 0)
        {
            if (C.StopOnError || ErrorIfModifier?.Condition == Modifiers.ErrorCondition.ItemNotFound)
                throw new MacroException("You do not have that item");
            return;
        }

        await context.RunOnFramework(() => Game.UseItem(itemId, ItemQualityModifier?.IsHighQuality ?? false));
        await PerformWait(token);
    }

    private uint SearchItemId(string itemName)
        => FindRow<Sheets.Item>(x => x.Name.ToString().Equals(itemName, StringComparison.InvariantCultureIgnoreCase))?.RowId ?? throw new MacroException($"Item not found: {itemName}");
}
