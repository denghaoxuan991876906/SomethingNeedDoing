using Lumina.Excel.Sheets;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Uses a key item from the inventory.
/// </summary>
[GenericDoc(
    "Use a key item from your inventory",
    ["itemName"],
    ["/keyitem \"Wondrous Tails\"", "/keyitem \"Wondrous Tails\" <errorif.itemnotfound>"]
)]
public class KeyItemCommand(string text, string itemName) : MacroCommandBase(text)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        var itemId = SearchItemId(itemName);
        Svc.Log.Debug($"KeyItem found: {itemId}");

        var count = Game.GetInventoryItemCount(itemId, false);
        Svc.Log.Debug($"Item Count: {count}");

        if (count == 0)
        {
            if (C.StopOnError || ErrorIfModifier?.Condition == Modifiers.ErrorCondition.ItemNotFound)
                throw new MacroException("You do not have that item");
            return;
        }

        await context.RunOnFramework(() => Game.UseItem(itemId, false));
        await PerformWait(token);
    }

    private uint SearchItemId(string itemName)
        => FindRow<EventItem>(x => x.Name.ExtractText().Equals(itemName, StringComparison.InvariantCultureIgnoreCase))?.RowId ?? throw new MacroException($"Key item not found: {itemName}");
}
