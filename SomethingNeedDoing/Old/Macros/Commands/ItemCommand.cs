using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Text.RegularExpressions;
using System.Threading;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Macros.Exceptions;
using SomethingNeedDoing.Old.Misc;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class ItemCommand : MacroCommand
{
    public static string[] Commands => ["item"];
    public static string Description => "Use an item, stopping the macro if the item is not present.";
    public static string[] Examples => ["/item Calamari Ripieni", "/item Calamari Ripieni <hq> <wait.3>"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static nint itemContextMenuAgent = nint.Zero;
    public delegate void UseItemDelegate(nint itemContextMenuAgent, uint itemID, uint inventoryPage, uint inventorySlot, short a5);
    public static UseItemDelegate UseItemSig = null!;

    private readonly string itemName;
    private readonly ItemQualityModifier itemQualityMod;

    private ItemCommand(string text, string itemName, WaitModifier wait, ItemQualityModifier itemQualityMod) : base(text, wait)
    {
        this.itemName = itemName.ToLowerInvariant();
        this.itemQualityMod = itemQualityMod;
    }

    public static ItemCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = ItemQualityModifier.TryParse(ref text, out var itemQualityModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");

        return new ItemCommand(text, nameValue, waitModifier, itemQualityModifier);
    }

    public override async System.Threading.Tasks.Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        var itemId = SearchItemId(itemName);
        Svc.Log.Debug($"Item found: {itemId}");

        var count = GetInventoryItemCount(itemId, itemQualityMod.IsHq);
        Svc.Log.Debug($"Item Count: {count}");
        if (count == 0)
        {
            if (C.StopMacroIfItemNotFound)
                throw new MacroCommandError("You do not have that item");
            return;
        }

        UseItem(itemId, itemQualityMod.IsHq);
        await PerformWait(token);
    }

    private unsafe void UseItem(uint itemID, bool isHQ = false)
    {
        var agent = AgentInventoryContext.Instance();
        if (agent == null)
            throw new MacroCommandError("AgentInventoryContext not found");

        if (isHQ)
            itemID += 1_000_000;

        var result = agent->UseItem(itemID);
        if (result != 0 && C.StopMacroIfCantUseItem)
            throw new MacroCommandError("Failed to use item");
    }

    private unsafe int GetInventoryItemCount(uint itemID, bool isHQ)
    {
        var inventoryManager = InventoryManager.Instance();
        return inventoryManager == null
            ? throw new MacroCommandError("InventoryManager not found")
            : inventoryManager->GetInventoryItemCount(itemID, isHQ);
    }

    private uint SearchItemId(string itemName) => FindRow<Sheets.Item>(x => x.Name.ToString().Equals(itemName, System.StringComparison.InvariantCultureIgnoreCase))!.Value.RowId;
}
