using FFXIVClientStructs.FFXIV.Client.Game;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.LuaMacro.Modules;
public class InventoryModule : LuaModuleBase
{
    public override string ModuleName => "Inventory";

    [LuaFunction] public InventoryContainerWrapper GetInventoryContainer(InventoryType container) => new(container);
    [LuaFunction] public InventoryItemWrapper GetInventoryItem(InventoryType container, int slot) => new(container, slot);

    [LuaFunction]
    public unsafe InventoryItemWrapper? GetInventoryItem(uint itemId)
    {
        foreach (var type in Enum.GetValues<InventoryType>())
        {
            var container = InventoryManager.Instance()->GetInventoryContainer(type);
            if (container == null) continue;
            for (var i = 0; i < container->Size; i++)
                if (container->Items[i].ItemId == itemId)
                    return new(container, i);
        }
        return null;
    }

    public unsafe class InventoryContainerWrapper(InventoryType container) : IWrapper
    {
        private readonly InventoryContainer* _container = InventoryManager.Instance()->GetInventoryContainer(container);
        [LuaDocs] public uint Count => _container->Size;

        [LuaDocs]
        public int FreeSlots
        {
            get
            {
                var count = 0;
                for (var i = 0; i < Count; i++)
                    if (_container->Items[i].ItemId == 0)
                        count++;
                return count;
            }
        }

        [LuaDocs]
        public List<InventoryItemWrapper> Items
        {
            get
            {
                List<InventoryItemWrapper> list = [];
                for (var i = 0; i < Count; i++)
                    if (_container->Items[i].ItemId != 0)
                        list.Add(new(_container, i));
                return list;
            }
        }

        [LuaDocs] public InventoryItemWrapper this[int index] => new(_container, index);
    }

    public unsafe class InventoryItemWrapper : IWrapper
    {
        private InventoryItem* Item { get; set; }
        public InventoryItemWrapper(InventoryType container, int slot) => Item = InventoryManager.Instance()->GetInventoryContainer(container)->GetInventorySlot(slot);
        public InventoryItemWrapper(InventoryContainer* container, int slot) => Item = container->GetInventorySlot(slot);
        public InventoryItemWrapper(InventoryItem* item) => Item = item;

        [LuaDocs] public uint ItemId => Item->ItemId;
        [LuaDocs] public uint BaseItemId => Item->GetBaseItemId();
        [LuaDocs] public int Count => Item->Quantity;
        [LuaDocs] public ushort SpiritbondOrCollectability => Item->SpiritbondOrCollectability;
        [LuaDocs] public ushort Condition => Item->Condition;
        [LuaDocs] public uint GlamourId => Item->GlamourId;
        [LuaDocs] public bool IsHighQuality => Item->IsHighQuality();
        [LuaDocs] public InventoryItemWrapper? LinkedItem => Item->GetLinkedItem() is not null ? new(Item->GetLinkedItem()) : null;

        [LuaDocs] public InventoryType Container => Item->Container;
        [LuaDocs] public int Slot => Item->Slot;

        [LuaDocs] public void Use() => Game.UseItem(ItemId, IsHighQuality);
    }
}
