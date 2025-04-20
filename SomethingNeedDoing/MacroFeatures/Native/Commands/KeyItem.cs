using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Uses a key item from the inventory.
/// </summary>
public class KeyItemCommand(string text, string itemName, WaitModifier? waitMod = null) : MacroCommandBase(text, waitMod?.WaitDuration ?? 0)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        var itemId = SearchItemId(itemName);
        Svc.Log.Debug($"KeyItem found: {itemId}");

        var count = GetInventoryItemCount(itemId);
        Svc.Log.Debug($"Item Count: {count}");

        if (count == 0)
        {
            if (C.StopMacroIfItemNotFound)
                throw new MacroException("You do not have that item");
            return;
        }

        await context.RunOnFramework(() => UseItem(itemId));
        await PerformWait(token);
    }

    private unsafe void UseItem(uint itemId)
    {
        var agent = AgentInventoryContext.Instance();
        if (agent == null)
            throw new MacroException("AgentInventoryContext not found");

        var result = agent->UseItem(itemId);
        if (result != 0 && C.StopMacroIfCantUseItem)
            throw new MacroException("Failed to use item");
    }

    private unsafe int GetInventoryItemCount(uint itemId)
    {
        var inventoryManager = InventoryManager.Instance();
        return inventoryManager == null
            ? throw new MacroException("InventoryManager not found")
            : inventoryManager->GetInventoryItemCount(itemId);
    }

    private uint SearchItemId(string itemName)
        => FindRow<EventItem>(x => x.Name.ExtractText().Equals(itemName, StringComparison.InvariantCultureIgnoreCase))?.RowId ?? throw new MacroException($"Key item not found: {itemName}");

    /// <summary>
    /// Parses a key item command from text.
    /// </summary>
    public static KeyItemCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);

        var match = Regex.Match(text, @"^/keyitem\s+(?<name>.*?)\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var itemName = match.Groups["name"].Value.Trim('"');
        return new(text, itemName, waitMod as WaitModifier);
    }
}
