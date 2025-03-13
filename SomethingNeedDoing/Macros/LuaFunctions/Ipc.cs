using AutoRetainerAPI;
using SomethingNeedDoing.IPC;
using SomethingNeedDoing.Misc;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace SomethingNeedDoing.Macros.Lua;

public class Ipc
{
    internal static Ipc Instance { get; } = new();

    private readonly ARDiscard _ardiscard;
    private readonly Artisan _artisan;
    private readonly AllaganTools _aTools;
    private readonly AutoRetainer _autoretainer;
    private readonly AutoRetainerApi _autoRetainerApi;
    private readonly BossMod _bossmod;
    private readonly Dropbox _dropbox;
    private readonly LifestreamIPC _lifestream;
    private readonly Questionable _questionable;
    private readonly RSR _rsr;
    private readonly VislandIPC _visland;

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

    internal Ipc()
    {
        _ardiscard = new();
        _artisan = new();
        _aTools = new();
        _autoretainer = new();
        _autoRetainerApi = new();
        _bossmod = new();
        _dropbox = new();
        _lifestream = new();
        _questionable = new();
        _rsr = new();
        _visland = new();

        AutoDutyIPC.Init();
        DeliverooIPC.Init();
        NavmeshIPC.Init();
        PandorasBoxIPC.Init();
    }

    internal void Dispose()
    {
        _autoRetainerApi.Dispose();
    }

    #region PandorasBox
    public bool? PandoraGetFeatureEnabled(string feature) => PandorasBoxIPC.GetFeatureEnabled?.InvokeFunc(feature);
    public bool? PandoraGetFeatureConfigEnabled(string feature, string config) => PandorasBoxIPC.GetConfigEnabled?.InvokeFunc(feature, config);
    public void PandoraSetFeatureState(string feature, bool state) => PandorasBoxIPC.SetFeatureEnabled?.InvokeAction(feature, state);
    public void PandoraSetFeatureConfigState(string feature, string config, bool state) => PandorasBoxIPC.SetConfigEnabled?.InvokeAction(feature, config, state);
    public void PandoraPauseFeature(string feature, int ms) => PandorasBoxIPC.PauseFeature?.InvokeAction(feature, ms);
    #endregion

    #region AutoHook
    public unsafe void SetAutoHookState(bool state) => AutoHookIPC.SetPluginState(state);
    public unsafe void SetAutoHookAutoGigState(bool state) => AutoHookIPC.SetAutoGigState(state);
    public unsafe void SetAutoHookAutoGigSize(int size) => AutoHookIPC.SetAutoGigSize(size);
    public unsafe void SetAutoHookAutoGigSpeed(int speed) => AutoHookIPC.SetAutoGigSpeed(speed);
    public unsafe void SetAutoHookPreset(string preset) => AutoHookIPC.SetPreset(preset);
    public unsafe void UseAutoHookAnonymousPreset(string preset) => AutoHookIPC.CreateAndSelectAnonymousPreset(preset);
    public unsafe void DeletedSelectedAutoHookPreset() => AutoHookIPC.DeleteSelectedPreset();
    public unsafe void DeleteAllAutoHookAnonymousPresets() => AutoHookIPC.DeleteAllAnonymousPresets();
    #endregion

    #region Deliveroo
    public bool DeliverooIsTurnInRunning() => DeliverooIPC.IsTurnInRunning!.InvokeFunc();
    #endregion

    #region Questionable

    
    public bool QuestionableIsRunning() => _questionable.IsRunning();
    public string QuestionableGetCurrentQuestId() => _questionable.GetCurrentQuestId();
    public Questionable.StepData QuestionableGetCurrentStepData() => _questionable.GetCurrentStepData();
    public bool QuestionableIsQuestLocked(string questId) => _questionable.IsQuestLocked(questId);
    public bool QuestionableImportQuestPriority(string encodedQuestPriorities) => _questionable.ImportQuestPriority(encodedQuestPriorities);
    public bool QuestionableClearQuestPriority() => _questionable.ClearQuestPriority();
    public bool QuestionableAddQuestPriority(string questId) => _questionable.AddQuestPriority(questId);
    public bool QuestionableInsertQuestPriority(int index, string questId) => _questionable.InsertQuestPriority(index, questId);
    public string QuestionableExportQuestPriority() => _questionable.ExportQuestPriority();
    #endregion

    #region visland
    public bool IsVislandRouteRunning() => _visland.IsRouteRunning();
    public bool VislandIsRoutePaused() => _visland.IsRoutePaused();
    public void VislandSetRoutePaused(bool state) => _visland.SetRoutePaused(state);
    public void VislandStopRoute() => _visland.StopRoute();
    public void VislandStartRoute(string route, bool once) => _visland.StartRoute(route, once);
    #endregion

    #region navmesh
    public bool NavIsReady() => NavmeshIPC.NavIsReady();
    public float NavBuildProgress() => NavmeshIPC.NavBuildProgress();
    public void NavReload() => NavmeshIPC.NavReload();
    public void NavRebuild() => NavmeshIPC.NavRebuild();
    public void NavPathfind(float x, float y, float z, bool fly = false) => NavmeshIPC.NavPathfind(Svc.ClientState.LocalPlayer!.Position, new Vector3(x, y, z), fly);
    public bool NavIsAutoLoad() => NavmeshIPC.NavIsAutoLoad();
    public void NavSetAutoLoad(bool state) => NavmeshIPC.NavSetAutoLoad(state);
    public float? QueryMeshNearestPointX(float x, float y, float z, float halfExtentXZ, float halfExtentY) => NavmeshIPC.QueryMeshNearestPoint(new Vector3(x, y, z), halfExtentXZ, halfExtentY)?.X;
    public float? QueryMeshNearestPointY(float x, float y, float z, float halfExtentXZ, float halfExtentY) => NavmeshIPC.QueryMeshNearestPoint(new Vector3(x, y, z), halfExtentXZ, halfExtentY)?.Y;
    public float? QueryMeshNearestPointZ(float x, float y, float z, float halfExtentXZ, float halfExtentY) => NavmeshIPC.QueryMeshNearestPoint(new Vector3(x, y, z), halfExtentXZ, halfExtentY)?.Z;
    public float? QueryMeshPointOnFloorX(float x, float y, float z, bool allowUnlandable, float halfExtentXZ) => NavmeshIPC.QueryMeshPointOnFloor(new Vector3(x, y, z), allowUnlandable, halfExtentXZ)?.X;
    public float? QueryMeshPointOnFloorY(float x, float y, float z, bool allowUnlandable, float halfExtentXZ) => NavmeshIPC.QueryMeshPointOnFloor(new Vector3(x, y, z), allowUnlandable, halfExtentXZ)?.Y;
    public float? QueryMeshPointOnFloorZ(float x, float y, float z, bool allowUnlandable, float halfExtentXZ) => NavmeshIPC.QueryMeshPointOnFloor(new Vector3(x, y, z), allowUnlandable, halfExtentXZ)?.Z;
    public void PathMoveTo(float x, float y, float z, bool fly = false) => NavmeshIPC.PathMoveTo([new Vector3(x, y, z)], fly);
    public void PathStop() => NavmeshIPC.PathStop();
    public bool PathIsRunning() => NavmeshIPC.PathIsRunning();
    public int PathNumWaypoints() => NavmeshIPC.PathNumWaypoints();
    public bool PathGetMovementAllowed() => NavmeshIPC.PathGetMovementAllowed();
    public void PathSetMovementAllowed(bool state) => NavmeshIPC.PathSetMovementAllowed(state);
    public bool PathGetAlignCamera() => NavmeshIPC.PathGetAlignCamera();
    public void PathSetAlignCamera(bool state) => NavmeshIPC.PathSetAlignCamera(state);
    public float PathGetTolerance() => NavmeshIPC.PathGetTolerance();
    public void PathSetTolerance(float t) => NavmeshIPC.PathSetTolerance(t);
    public void PathfindAndMoveTo(float x, float y, float z, bool fly = false) => NavmeshIPC.PathfindAndMoveTo(new Vector3(x, y, z), fly);
    public bool PathfindInProgress() => NavmeshIPC.PathfindInProgress();
    #endregion

    #region AutoRetainer
    public unsafe void ARSetSuppressed(bool state) => _autoRetainerApi.Suppressed = state;

    public unsafe List<string> ARGetRegisteredCharacters()
        => _autoRetainerApi.GetRegisteredCharacters().AsParallel()
        .Select(c => $"{_autoRetainerApi.GetOfflineCharacterData(c).Name}@{_autoRetainerApi.GetOfflineCharacterData(c).World}").ToList();

    public unsafe List<string> ARGetRegisteredEnabledCharacters()
        => _autoRetainerApi.GetRegisteredCharacters().AsParallel()
        .Where(c => _autoRetainerApi.GetOfflineCharacterData(c).Enabled)
        .Select(c => $"{_autoRetainerApi.GetOfflineCharacterData(c).Name}@{_autoRetainerApi.GetOfflineCharacterData(c).World}").ToList();

    public unsafe List<string> ARGetRegisteredRetainers()
        => _autoRetainerApi.GetRegisteredCharacters().AsParallel()
        .Select(c => _autoRetainerApi.GetOfflineCharacterData(c).RetainerData.Select(r => r.Name)).SelectMany(names => names).ToList();

    public unsafe List<string> ARGetRegisteredEnabledRetainers()
        => _autoRetainerApi.GetRegisteredCharacters().AsParallel()
        .Where(c => _autoRetainerApi.GetOfflineCharacterData(c).Enabled)
        .Select(c => _autoRetainerApi.GetOfflineCharacterData(c).RetainerData
        .Where(r => r.HasVenture).Select(r => r.Name)).SelectMany(names => names).ToList();

    public unsafe bool ARAnyWaitingToBeProcessed(bool allCharacters = false) => ARRetainersWaitingToBeProcessed(allCharacters) || ARSubsWaitingToBeProcessed(allCharacters);

    public unsafe bool ARRetainersWaitingToBeProcessed(bool allCharacters = false)
    {
        return !allCharacters
            ? _autoRetainerApi.GetOfflineCharacterData(Svc.ClientState.LocalContentId).RetainerData.AsParallel().Any(x => x.HasVenture && x.VentureEndsAt <= DateTime.Now.ToUnixTimestamp())
            : GetAllEnabledCharacters().Any(character => _autoRetainerApi.GetOfflineCharacterData(character).RetainerData.Any(x => x.HasVenture && x.VentureEndsAt <= DateTime.Now.ToUnixTimestamp()));
    }

    public unsafe bool ARSubsWaitingToBeProcessed(bool allCharacters = false)
    {
        return !allCharacters
            ? _autoRetainerApi.GetOfflineCharacterData(Svc.ClientState.LocalContentId).OfflineSubmarineData.AsParallel().Any(x => x.ReturnTime <= DateTime.Now.ToUnixTimestamp())
            : GetAllEnabledCharacters().Any(c => _autoRetainerApi.GetOfflineCharacterData(c).OfflineSubmarineData.Any(x => x.ReturnTime <= DateTime.Now.ToUnixTimestamp()));
    }

    public void ARFinishCharacterPostProcess() => _autoRetainerApi.FinishCharacterPostProcess();
    public List<ulong> ARGetCharacterCIDs() => _autoRetainerApi.GetRegisteredCharacters();
    public AutoRetainerAPI.Configuration.OfflineCharacterData ARGetCharacterData(ulong cid) => _autoRetainerApi.GetOfflineCharacterData(cid);

    public bool ARGetMultiModeEnabled() => _autoretainer.GetMultiModeEnabled();
    public void ARSetMultiModeEnabled(bool value) => _autoretainer.SetMultiModeEnabled(value);
    public bool ARIsBusy() => _autoretainer.IsBusy();
    public int ARGetInventoryFreeSlotCount() => _autoretainer.GetInventoryFreeSlotCount();
    public Dictionary<ulong, HashSet<string>> ARGetEnabledRetainers() => _autoretainer.GetEnabledRetainers();
    public bool ARAreAnyRetainersAvailableForCurrentChara() => _autoretainer.AreAnyRetainersAvailableForCurrentChara();
    public void ARAbortAllTasks() => _autoretainer.AbortAllTasks();
    public void ARDisableAllFunctions() => _autoretainer.DisableAllFunctions();
    public void AREnableMultiMode() => _autoretainer.EnableMultiMode();
    public void AREnqueueHET() => _autoretainer.EnqueueHET(() => { });
    public bool ARCanAutoLogin() => _autoretainer.CanAutoLogin();
    public bool ARRelog(string charaNameWithWorld) => _autoretainer.Relog(charaNameWithWorld);
    public bool ARGetOptionRetainerSense() => _autoretainer.GetOptionRetainerSense();
    public void ARSetOptionRetainerSense(bool value) => _autoretainer.SetOptionRetainerSense(value);
    public int ARGetOptionRetainerSenseThreshold() => _autoretainer.GetOptionRetainerSenseThreshold();
    public void ARSetOptionRetainerSenseThreshold(int value) => _autoretainer.SetOptionRetainerSenseThreshold(value);
    public long? ARGetClosestRetainerVentureSecondsRemaining(ulong cid) => _autoretainer.GetClosestRetainerVentureSecondsRemaining(cid);
    public void AREnqueueInitiation() => _autoretainer.EnqueueInitiation();
    public (uint ShopDataID, uint ExchangeDataID, Vector3 Position)? ARGetGCInfo() => _autoretainer.GetGCInfo();

    private unsafe ParallelQuery<ulong> GetAllEnabledCharacters() => _autoRetainerApi.GetRegisteredCharacters().AsParallel().Where(c => _autoRetainerApi.GetOfflineCharacterData(c).Enabled);
    #endregion

    #region YesAlready
    public void PauseYesAlready()
    {
        if (Svc.PluginInterface.TryGetData<HashSet<string>>("YesAlready.StopRequests", out var data) && !data.Contains(nameof(SomethingNeedDoing)))
        {
            Svc.Log.Debug("Disabling YesAlready");
            data.Add(nameof(SomethingNeedDoing));
        }
    }

    public void RestoreYesAlready()
    {
        if (Svc.PluginInterface.TryGetData<HashSet<string>>("YesAlready.StopRequests", out var data) && data.Contains(nameof(SomethingNeedDoing)))
        {
            Svc.Log.Debug("Restoring YesAlready");
            data.Remove(nameof(SomethingNeedDoing));
        }
    }
    #endregion

    #region TextAdvance
    internal static void PauseTextAdvance()
    {
        if (Svc.PluginInterface.TryGetData<HashSet<string>>("TextAdvance.StopRequests", out var data) && !data.Contains(nameof(SomethingNeedDoing)))
        {
            Svc.Log.Debug("Disabling TextAdvance");
            data.Add(nameof(SomethingNeedDoing));
        }
    }

    internal static void RestoreTextAdvance()
    {
        if (Svc.PluginInterface.TryGetData<HashSet<string>>("TextAdvance.StopRequests", out var data) && data.Contains(nameof(SomethingNeedDoing)))
        {
            Svc.Log.Debug("Restoring TextAdvance");
            data.Remove(nameof(SomethingNeedDoing));
        }
    }
    #endregion

    #region Dropbox
    public void DropboxStart() => _dropbox.BeginTradingQueue();
    public void DropboxStop() => _dropbox.Stop();
    public bool DropboxIsBusy() => _dropbox.IsBusy();
    public int DropboxGetItemQuantity(uint id, bool hq) => _dropbox.GetItemQuantity(id, hq);
    public void DropboxSetItemQuantity(uint id, bool hq, int quantity) => _dropbox.SetItemQuantity(id, hq, quantity);
    #endregion

    #region Lifestream
    public bool LifestreamAethernetTeleport(string aethernetName) => _lifestream.AethernetTeleport(aethernetName);
    public bool LifestreamTeleport(uint destination, byte subIndex) => _lifestream.Teleport(destination, subIndex);
    public bool LifestreamTeleportToHome() => _lifestream.TeleportToHome();
    public bool LifestreamTeleportToFC() => _lifestream.TeleportToFC();
    public bool LifestreamTeleportToApartment() => _lifestream.TeleportToApartment();
    public bool LifestreamIsBusy() => _lifestream.IsBusy();
    public void LifestreamExecuteCommand(string command) => _lifestream.ExecuteCommand(command);
    public void LifestreamAbort() => _lifestream.Abort();
    #endregion

    #region RSR
    public void RSRAddPriorityNameID(uint nameId) => _rsr.AddPriorityNameID(nameId);
    public void RSRRemovePriorityNameID(uint nameId) => _rsr.RemovePriorityNameID(nameId);
    public void RSRAddBlacklistNameID(uint nameId) => _rsr.AddBlacklistNameID(nameId);
    public void RSRRemoveBlacklistNameID(uint nameId) => _rsr.RemoveBlacklistNameID(nameId);
    public void RSRChangeOperatingMode(byte stateCommand) => _rsr.ChangeOperatingMode((RSR.StateCommandType)stateCommand);
    public void RSRTriggerSpecialState(byte specialCommand) => _rsr.TriggerSpecialState((RSR.SpecialCommandType)specialCommand);
    #endregion

    #region Artisan
    public bool ArtisanGetEnduranceStatus() => _artisan.GetEnduranceStatus();
    public void ArtisanSetEnduranceStatus(bool state) => _artisan.SetEnduranceStatus(state);
    public bool ArtisanIsListRunning() => _artisan.IsListRunning();
    public bool ArtisanIsListPaused() => _artisan.IsListPaused();
    public void ArtisanSetListPause(bool state) => _artisan.SetListPause(state);
    public bool ArtisanGetStopRequest() => _artisan.GetStopRequest();
    public void ArtisanSetStopRequest(bool state) => _artisan.SetStopRequest(state);
    public void ArtisanCraftItem(ushort recipeID, int amount) => _artisan.CraftItem(recipeID, amount);
    #endregion

    #region ARDiscard
    public List<uint> ARDiscardGetItemsToDiscard() => [.. _ardiscard.GetItemsToDiscard()];
    #endregion

    #region AutoDuty
    public void ADListConfig() => AutoDutyIPC.ListConfig?.InvokeAction();
    public string? ADGetConfig(string config) => AutoDutyIPC.GetConfig?.InvokeFunc(config);
    public void ADSetConfig(string config, string setting) => AutoDutyIPC.SetConfig?.InvokeAction(config, setting);
    public void ADRun(uint territoryType, int loops = 0, bool bareMode = false) => AutoDutyIPC.Run?.InvokeAction(territoryType, loops, bareMode);
    public void ADStart(bool startFromZero = true) => AutoDutyIPC.Start?.InvokeAction(startFromZero);
    public void ADStop() => AutoDutyIPC.Stop?.InvokeAction();
    public bool? ADIsNavigating() => AutoDutyIPC.IsNavigating?.InvokeFunc();
    public bool? ADIsLooping() => AutoDutyIPC.IsLooping?.InvokeFunc();
    public bool? ADIsStopped() => AutoDutyIPC.IsStopped?.InvokeFunc();
    public bool? ADContentHasPath(uint territoryType) => AutoDutyIPC.ContentHasPath?.InvokeFunc(territoryType);
    #endregion

    #region Allagan Tools
    public uint ATInventoryCountByType(uint inventoryType, ulong? characterId) => _aTools.InventoryCountByType(inventoryType, characterId);
    public uint ATInventoryCountByTypes(uint[] inventoryTypes, ulong? characterId) => _aTools.InventoryCountByTypes(inventoryTypes, characterId);
    public uint ATItemCount(uint itemId, ulong characterId, int inventoryType) => _aTools.ItemCount(itemId, characterId, inventoryType);
    public uint ATItemCountHQ(uint itemId, ulong characterId, int inventoryType) => _aTools.ItemCountHQ(itemId, characterId, inventoryType);
    public uint ATItemCountOwned(uint itemId, bool currentCharacterOnly, uint[] inventoryTypes) => _aTools.ItemCountOwned(itemId, currentCharacterOnly, inventoryTypes);
    public bool ATEnableUiFilter(string filterKey) => _aTools.EnableUiFilter(filterKey);
    public bool ATDisableUiFilter() => _aTools.DisableUiFilter();
    public bool ATToggleUiFilter(string filterKey) => _aTools.ToggleUiFilter(filterKey);
    public bool ATEnableBackgroundFilter(string filterKey) => _aTools.EnableBackgroundFilter(filterKey);
    public bool ATDisableBackgroundFilter() => _aTools.DisableBackgroundFilter();
    public bool ATToggleBackgroundFilter(string filterKey) => _aTools.ToggleBackgroundFilter(filterKey);
    public bool ATEnableCraftList(string filterKey) => _aTools.EnableCraftList(filterKey);
    public bool ATDisableCraftList() => _aTools.DisableCraftList();
    public bool ATToggleCraftList(string filterKey) => _aTools.ToggleCraftList(filterKey);
    public bool ATAddItemToCraftList(string filterKey, uint itemId, uint quantity) => _aTools.AddItemToCraftList(filterKey, itemId, quantity);
    public bool ATRemoveItemFromCraftList(string filterKey, uint itemId, uint quantity) => _aTools.RemoveItemFromCraftList(filterKey, itemId, quantity);
    public Dictionary<uint, uint> ATGetFilterItems(string filterKey) => _aTools.GetFilterItems(filterKey);
    public Dictionary<uint, uint> ATGetCraftItems(string filterKey) => _aTools.GetCraftItems(filterKey);
    public Dictionary<uint, uint> ATGetRetrievalItems() => _aTools.GetRetrievalItems();
    public HashSet<ulong[]> ATGetCharacterItems(ulong characterId) => _aTools.GetCharacterItems(characterId);
    public HashSet<ulong> ATGetCharactersOwnedByActive(bool includeOwner) => _aTools.GetCharactersOwnedByActive(includeOwner);
    public HashSet<ulong[]> ATGetCharacterItemsByType(ulong characterId, uint inventoryType) => _aTools.GetCharacterItemsByType(characterId, inventoryType);
    public Dictionary<string, string> ATGetCraftLists() => _aTools.GetCraftLists();
    public Dictionary<string, string> ATGetSearchFilters() => _aTools.GetSearchFilters();
    public string ATAddNewCraftList(string craftListName, Dictionary<uint, uint> items) => _aTools.AddNewCraftList(craftListName, items);
    public ulong? ATCurrentCharacter() => _aTools.CurrentCharacter();
    public bool ATIsInitialized() => _aTools.IsInitialized();
    #endregion

    #region BossMod
    public string? BMGet(string name) => _bossmod.Get(name);
    public bool BMCreate(string presetSerialized, bool overwrite) => _bossmod.Create(presetSerialized, overwrite);
    public bool BMDelete(string name) => _bossmod.Delete(name);
    public string BMGetActive() => _bossmod.GetActive();
    public bool BMSetActive(string name) => _bossmod.SetActive(name);
    public bool BMClearActive() => _bossmod.ClearActive();
    public bool BMGetForceDisabled() => _bossmod.GetForceDisabled();
    public bool BMSetForceDisabled() => _bossmod.SetForceDisabled();
    public bool BMAddTransientStrategy(string presetName, string moduleTypeName, string trackName, string value) => _bossmod.AddTransientStrategy(presetName, moduleTypeName, trackName, value);
    public bool BMAddTransientStrategyTargetEnemyOID(string presetName, string moduleTypeName, string trackName, string value, int oid) => _bossmod.AddTransientStrategyTargetEnemyOID(presetName, moduleTypeName, trackName, value, oid);
    #endregion
}
