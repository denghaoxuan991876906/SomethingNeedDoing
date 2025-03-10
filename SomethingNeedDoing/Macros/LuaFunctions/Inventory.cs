using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Reflection;

namespace SomethingNeedDoing.Macros.Lua;

public class Inventory
{
    internal static Inventory Instance { get; } = new();

    private enum ItemRarity : byte
    {
        White = 1,
        Pink = 7,
        Green = 2,
        Blue = 3,
        Purple = 4
    }

    public List<string> ListAllFunctions()
    {
        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var list = new List<string>();
        foreach (var method in methods.Where(x => x.Name != nameof(ListAllFunctions) && x.DeclaringType != typeof(object)))
        {
            var parameterList = method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue ?? "null") : "")}");
            list.Add($"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameterList)})");
        }
        return list;
    }

    public unsafe int GetItemCount(int itemID, bool includeHQ = true)
       => includeHQ ? InventoryManager.Instance()->GetInventoryItemCount((uint)itemID, true) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000)
       : InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000);

    public unsafe int GetItemCountInContainer(uint itemID, uint container) => GetItemInInventory(itemID, (InventoryType)container)->Quantity;

    public unsafe int GetInventoryFreeSlotCount()
    {
        InventoryType[] types = [InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4];
        var slots = 0;
        foreach (var x in types)
        {
            var cont = InventoryManager.Instance()->GetInventoryContainer(x);
            for (var i = 0; i < cont->Size; i++)
                if (cont->Items[i].ItemId == 0)
                    slots++;
        }
        return slots;
    }

    public unsafe uint GetItemIdInSlot(uint container, uint slot)
        => InventoryManager.Instance()->GetInventoryContainer((InventoryType)container)->GetInventorySlot((ushort)slot)->ItemId;

    public unsafe int GetItemCountInSlot(uint container, uint slot)
        => InventoryManager.Instance()->GetInventoryContainer((InventoryType)container)->GetInventorySlot((ushort)slot)->Quantity;

    public unsafe List<uint> GetItemIdsInContainer(uint container)
    {
        var cont = InventoryManager.Instance()->GetInventoryContainer((InventoryType)container);
        var list = new List<uint>();
        for (var i = 0; i < cont->Size; i++)
            if (cont->Items[i].ItemId != 0)
                list.Add(cont->Items[i].ItemId);
        return list;
    }

    public unsafe int GetFreeSlotsInContainer(uint container)
    {
        var inv = InventoryManager.Instance();
        var cont = inv->GetInventoryContainer((InventoryType)container);
        var slots = 0;
        for (var i = 0; i < cont->Size; i++)
            if (cont->Items[i].ItemId == 0)
                slots++;
        return slots;
    }

    public unsafe void MoveItemToContainer(uint itemID, uint srcContainer, uint dstContainer)
        => InventoryManager.Instance()->MoveItemSlot((InventoryType)srcContainer, (ushort)GetItemInInventory(itemID, (InventoryType)srcContainer)->Slot, (InventoryType)dstContainer, GetFirstAvailableSlot((InventoryType)dstContainer), 1);

    private static unsafe InventoryItem* GetItemInInventory(uint itemId, InventoryType inv, bool mustBeHQ = false)
    {
        var cont = InventoryManager.Instance()->GetInventoryContainer(inv);
        for (var i = 0; i < cont->Size; ++i)
            if (cont->GetInventorySlot(i)->ItemId == itemId && (!mustBeHQ || cont->GetInventorySlot(i)->Flags == InventoryItem.ItemFlags.HighQuality))
                return cont->GetInventorySlot(i);
        return null;
    }

    private static unsafe ushort GetFirstAvailableSlot(InventoryType container)
    {
        var cont = InventoryManager.Instance()->GetInventoryContainer(container);
        for (var i = 0; i < cont->Size; i++)
            if (cont->Items[i].ItemId == 0)
                return (ushort)i;
        return 0;
    }

    private static unsafe InventoryItem* GetItemForSlot(InventoryType type, int slot)
        => InventoryManager.Instance()->GetInventoryContainer(type)->GetInventorySlot(slot);

    public List<uint> GetTradeableWhiteItemIDs() => Svc.Data.GetExcelSheet<Item>()!.Where(x => !x.IsUntradable && x.Rarity == (byte)ItemRarity.White).Select(x => x.RowId).ToList();

    public string GetItemName(uint itemId) => Svc.Data.GetExcelSheet<Item>()!.GetRow(itemId).Name.ExtractText();
}
