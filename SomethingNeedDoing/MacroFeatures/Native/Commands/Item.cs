using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.MacroFeatures.Native.Modifiers;

namespace SomethingNeedDoing.MacroFeatures.Native.Commands;
/// <summary>
/// Uses an item from the inventory.
/// </summary>
public class ItemCommand(string text, string itemName, WaitModifier? waitMod = null, ItemQualityModifier? qualityMod = null) : MacroCommandBase(text, waitMod?.WaitDuration ?? 0)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        var itemId = SearchItemId(itemName);
        Svc.Log.Debug($"Item found: {itemId}");

        var count = GetInventoryItemCount(itemId, qualityMod?.IsHighQuality ?? false);
        Svc.Log.Debug($"Item Count: {count}");

        if (count == 0)
        {
            if (C.StopMacroIfItemNotFound)
                throw new MacroException("You do not have that item");
            return;
        }

        await context.RunOnFramework(() => UseItem(itemId, qualityMod?.IsHighQuality ?? false));
        await PerformWait(token);
    }

    private unsafe void UseItem(uint itemId, bool isHQ)
    {
        var agent = AgentInventoryContext.Instance();
        if (agent == null)
            throw new MacroException("AgentInventoryContext not found");

        if (isHQ)
            itemId += 1_000_000;

        var result = agent->UseItem(itemId);
        if (result != 0 && C.StopMacroIfCantUseItem)
            throw new MacroException("Failed to use item");
    }

    private unsafe int GetInventoryItemCount(uint itemId, bool isHQ)
    {
        var inventoryManager = InventoryManager.Instance();
        return inventoryManager == null
            ? throw new MacroException("InventoryManager not found")
            : inventoryManager->GetInventoryItemCount(itemId, isHQ);
    }

    private uint SearchItemId(string itemName)
        => FindRow<Sheets.Item>(x => x.Name.ToString().Equals(itemName, StringComparison.InvariantCultureIgnoreCase))?.RowId ?? throw new MacroException($"Item not found: {itemName}");

    /// <summary>
    /// Parses an item command from text.
    /// </summary>
    public static ItemCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);
        _ = ItemQualityModifier.TryParse(ref text, out var qualityMod);

        var match = Regex.Match(text, @"^/item\s+(?<name>.*?)\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var itemName = match.Groups["name"].Value.Trim('"');
        return new(text, itemName, waitMod as WaitModifier, qualityMod as ItemQualityModifier);
    }
}
