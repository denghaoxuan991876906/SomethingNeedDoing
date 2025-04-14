using ECommons.EzIpcManager;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace SomethingNeedDoing.Old.IPC;
#nullable disable
public class AllaganTools
{
    public AllaganTools() => EzIPC.Init(this, "InventoryTools");

    /// <remarks> uint inventoryType, ulong? characterId </remarks>
    [EzIPC] public readonly Func<uint, ulong?, uint> InventoryCountByType;

    /// <remarks> uint[] inventoryTypes, ulong? characterId </remarks>
    [EzIPC] public readonly Func<uint[], ulong?, uint> InventoryCountByTypes;

    /// <remarks> uint itemId, ulong characterId, int inventoryType </remarks>
    [EzIPC] public readonly Func<uint, ulong, int, uint> ItemCount;

    /// <remarks> uint itemId, ulong characterId, int inventoryType </remarks>
    [EzIPC] public readonly Func<uint, ulong, int, uint> ItemCountHQ;

    /// <remarks> uint itemId, bool currentCharacterOnly, uint[] inventoryTypes </remarks>
    [EzIPC] public readonly Func<uint, bool, uint[], uint> ItemCountOwned;

    /// <remarks> string filterKey </remarks>
    [EzIPC] public readonly Func<string, bool> EnableUiFilter;

    [EzIPC] public readonly Func<bool> DisableUiFilter;

    /// <remarks> string filterKey </remarks>
    [EzIPC] public readonly Func<string, bool> ToggleUiFilter;

    /// <remarks> string filterKey </remarks>
    [EzIPC] public readonly Func<string, bool> EnableBackgroundFilter;

    [EzIPC] public readonly Func<bool> DisableBackgroundFilter;

    /// <remarks> string filterKey </remarks>
    [EzIPC] public readonly Func<string, bool> ToggleBackgroundFilter;

    /// <remarks> string filterKey </remarks>
    [EzIPC] public readonly Func<string, bool> EnableCraftList;

    [EzIPC] public readonly Func<bool> DisableCraftList;

    /// <remarks> string filterKey </remarks>
    [EzIPC] public readonly Func<string, bool> ToggleCraftList;

    /// <remarks> string filterKey, uint itemId, uint quantity </remarks>
    [EzIPC] public readonly Func<string, uint, uint, bool> AddItemToCraftList;

    /// <remarks> string filterKey, uint itemId, uint quantity </remarks>
    [EzIPC] public readonly Func<string, uint, uint, bool> RemoveItemFromCraftList;

    /// <remarks> string filterKey </remarks>
    [EzIPC] public readonly Func<string, Dictionary<uint, uint>> GetFilterItems;

    /// <remarks> string filterKey </remarks>
    [EzIPC] public readonly Func<string, Dictionary<uint, uint>> GetCraftItems;

    [EzIPC] public readonly Func<Dictionary<uint, uint>> GetRetrievalItems;

    /// <remarks> ulong characterId </remarks>
    [EzIPC] public readonly Func<ulong, HashSet<ulong[]>> GetCharacterItems;

    /// <remarks> bool includeOwner </remarks>
    [EzIPC] public readonly Func<bool, HashSet<ulong>> GetCharactersOwnedByActive;

    /// <remarks> ulong characterId, uint inventoryType </remarks>
    [EzIPC] public readonly Func<ulong, uint, HashSet<ulong[]>> GetCharacterItemsByType;

    /// <remarks> uint itemId, InventoryItem.ItemFlags itemFlags, ulong characterId, uint quantity </remarks>
    [EzIPCEvent] public readonly Func<(uint, InventoryItem.ItemFlags, ulong, uint), bool> ItemAdded;

    /// <remarks> uint itemId, InventoryItem.ItemFlags itemFlags, ulong characterId, uint quantity </remarks>
    [EzIPCEvent] public readonly Func<(uint, InventoryItem.ItemFlags, ulong, uint), bool> ItemRemoved;

    [EzIPC] public readonly Func<Dictionary<string, string>> GetCraftLists;

    [EzIPC] public readonly Func<Dictionary<string, string>> GetSearchFilters;

    /// <remarks> string craftListName, Dictionary<uint, uint> items </remarks>
    [EzIPC] public readonly Func<string, Dictionary<uint, uint>, string> AddNewCraftList;

    [EzIPC] public readonly Func<ulong?> CurrentCharacter;

    /// <remarks> ulong? retainerId </remarks>
    [EzIPCEvent] public readonly Func<ulong?, bool> RetainerChanged;

    [EzIPC] public readonly Func<bool> IsInitialized;
}
