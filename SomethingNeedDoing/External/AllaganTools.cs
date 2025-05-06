using ECommons.EzIpcManager;
using FFXIVClientStructs.FFXIV.Client.Game;
using SomethingNeedDoing.Framework.Interfaces;

namespace SomethingNeedDoing.External;
public class AllaganTools : IPC
{
    public override string Name => "InventoryTools";
    public override string Repo => Repos.FirstParty;

    [EzIPC]
    [LuaFunction(
        description: "Gets the count of items in a specific inventory type",
        parameterDescriptions: ["InventoryType id", "CID (optional)"])]
    public readonly Func<uint, ulong?, uint> InventoryCountByType = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets the count of items across multiple inventory types",
        parameterDescriptions: ["Array of InventoryType ids", "CID (optional)"])]
    public readonly Func<uint[], ulong?, uint> InventoryCountByTypes = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets the count of a specific item in a specific inventory",
        parameterDescriptions: ["Item ID", "CID", "InventoryType id"])]
    public readonly Func<uint, ulong, int, uint> ItemCount = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets the count of a specific high-quality item in a specific inventory",
        parameterDescriptions: ["Item ID", "CID", "InventoryType id"])]
    public readonly Func<uint, ulong, int, uint> ItemCountHQ = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets the count of a specific item across multiple inventories",
        parameterDescriptions: ["Item ID", "Whether to check only the current character", "Array of InventoryType ids"])]
    public readonly Func<uint, bool, uint[], uint> ItemCountOwned = null!;

    [EzIPC]
    [LuaFunction(
        description: "Enables a UI filter",
        parameterDescriptions: ["Filter key to enable"])]
    public readonly Func<string, bool> EnableUiFilter = null!;

    [EzIPC]
    [LuaFunction(description: "Disables all UI filters")]
    public readonly Func<bool> DisableUiFilter = null!;

    [EzIPC]
    [LuaFunction(
        description: "Toggles a UI filter on/off",
        parameterDescriptions: ["Filter key"])]
    public readonly Func<string, bool> ToggleUiFilter = null!;

    [EzIPC]
    [LuaFunction(
        description: "Enables a background filter",
        parameterDescriptions: ["Filter key to enable"])]
    public readonly Func<string, bool> EnableBackgroundFilter = null!;

    [EzIPC]
    [LuaFunction(description: "Disables all background filters")]
    public readonly Func<bool> DisableBackgroundFilter = null!;

    [EzIPC]
    [LuaFunction(
        description: "Toggles a background filter on/off",
        parameterDescriptions: ["Filter key"])]
    public readonly Func<string, bool> ToggleBackgroundFilter = null!;

    [EzIPC]
    [LuaFunction(
        description: "Enables a craft list",
        parameterDescriptions: ["Filter key to enable"])]
    public readonly Func<string, bool> EnableCraftList = null!;

    [EzIPC]
    [LuaFunction(description: "Disables all craft lists")]
    public readonly Func<bool> DisableCraftList = null!;

    [EzIPC]
    [LuaFunction(
        description: "Toggles a craft list on/off",
        parameterDescriptions: ["Filter key"])]
    public readonly Func<string, bool> ToggleCraftList = null!;

    [EzIPC]
    [LuaFunction(
        description: "Adds an item to a craft list",
        parameterDescriptions: ["Filter key", "Item ID", "Quantity"])]
    public readonly Func<string, uint, uint, bool> AddItemToCraftList = null!;

    [EzIPC]
    [LuaFunction(
        description: "Removes an item from a craft list",
        parameterDescriptions: ["Filter key", "Item ID", "Quantity"])]
    public readonly Func<string, uint, uint, bool> RemoveItemFromCraftList = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets all items in a filter",
        parameterDescriptions: ["Filter key"])]
    public readonly Func<string, Dictionary<uint, uint>> GetFilterItems = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets all items in a craft list",
        parameterDescriptions: ["Filter key"])]
    public readonly Func<string, Dictionary<uint, uint>> GetCraftItems = null!;

    [EzIPC]
    [LuaFunction(description: "Gets all items in the retrieval list")]
    public readonly Func<Dictionary<uint, uint>> GetRetrievalItems = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets all items owned by a character",
        parameterDescriptions: ["CID"])]
    public readonly Func<ulong, HashSet<ulong[]>> GetCharacterItems = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets all characters owned by the active character",
        parameterDescriptions: ["Whether to include the owner"])]
    public readonly Func<bool, HashSet<ulong>> GetCharactersOwnedByActive = null!;

    [EzIPC]
    [LuaFunction(
        description: "Gets all items of a specific type owned by a character",
        parameterDescriptions: ["CID", "InventoryType id"])]
    public readonly Func<ulong, uint, HashSet<ulong[]>> GetCharacterItemsByType = null!;

    [EzIPCEvent]
    [LuaFunction(
        description: "Event triggered when an item is added to inventory",
        parameterDescriptions: ["Item ID", "Item flags", "CID", "Quantity"])]
    public readonly Func<(uint, InventoryItem.ItemFlags, ulong, uint), bool> ItemAdded = null!;

    [EzIPCEvent]
    [LuaFunction(
        description: "Event triggered when an item is removed from inventory",
        parameterDescriptions: ["Item ID", "Item flags", "CID", "Quantity"])]
    public readonly Func<(uint, InventoryItem.ItemFlags, ulong, uint), bool> ItemRemoved = null!;

    [EzIPC]
    [LuaFunction(description: "Gets all craft lists")]
    public readonly Func<Dictionary<string, string>> GetCraftLists = null!;

    [EzIPC]
    [LuaFunction(description: "Gets all search filters")]
    public readonly Func<Dictionary<string, string>> GetSearchFilters = null!;

    [EzIPC]
    [LuaFunction(
        description: "Adds a new craft list",
        parameterDescriptions: ["The name of the craft list", "Dictionary of items to add"])]
    public readonly Func<string, Dictionary<uint, uint>, string> AddNewCraftList = null!;

    [EzIPC]
    [LuaFunction(description: "Gets the current character ID")]
    public readonly Func<ulong?> CurrentCharacter = null!;

    [EzIPCEvent]
    [LuaFunction(
        description: "Event triggered when a retainer is changed",
        parameterDescriptions: ["Retainer ID"])]
    public readonly Func<ulong?, bool> RetainerChanged = null!;

    [EzIPC]
    [LuaFunction(description: "Checks if the plugin is initialized")]
    public readonly Func<bool> IsInitialized = null!;

}
