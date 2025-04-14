using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Text.RegularExpressions;
using System.Threading;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Macros.Exceptions;
using SomethingNeedDoing.Old.Misc;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class KeyItemCommand : MacroCommand
{
    public static string[] Commands => ["keyitem"];
    public static string Description => "Use a key item, stopping the macro if the item is not present.";
    public static string[] Examples => ["/keyitem Wondrous Tails", "/keyitem Gazelleskin Treasure Map"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static nint itemContextMenuAgent = nint.Zero;
    public delegate void UseItemDelegate(nint itemContextMenuAgent, uint itemID, uint inventoryPage, uint inventorySlot, short a5);
    public static UseItemDelegate UseItemSig = null!;

    private readonly string itemName;

    private KeyItemCommand(string text, string itemName, WaitModifier wait) : base(text, wait)
    {
        this.itemName = itemName.ToLowerInvariant();
    }

    public static KeyItemCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");

        return new KeyItemCommand(text, nameValue, waitModifier);
    }

    public override async System.Threading.Tasks.Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        var itemId = SearchItemId(itemName);
        Svc.Log.Debug($"KeyItem found: {itemId}");

        var count = GetInventoryItemCount(itemId);
        Svc.Log.Debug($"Item Count: {count}");
        if (count == 0)
        {
            if (C.StopMacroIfItemNotFound)
                throw new MacroCommandError("You do not have that item");
            return;
        }

        UseItem(itemId);
        await PerformWait(token);
    }

    private unsafe void UseItem(uint itemID)
    {
        var agent = AgentInventoryContext.Instance();
        if (agent == null)
            throw new MacroCommandError("AgentInventoryContext not found");

        var result = agent->UseItem(itemID);
        if (result != 0 && C.StopMacroIfCantUseItem)
            throw new MacroCommandError("Failed to use item");
    }

    private unsafe int GetInventoryItemCount(uint itemID)
    {
        var inventoryManager = InventoryManager.Instance();
        return inventoryManager == null
            ? throw new MacroCommandError("InventoryManager not found")
            : inventoryManager->GetInventoryItemCount(itemID);
    }

    private uint SearchItemId(string itemName) => FindRow<Sheets.EventItem>(x => x.Name.ExtractText().Equals(itemName, System.StringComparison.InvariantCultureIgnoreCase))!.Value.RowId;
}
