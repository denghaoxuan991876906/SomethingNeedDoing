using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace SomethingNeedDoing.Misc;
internal class Changelog
{
    public static void Draw()
    {
        static void DisplayChangelog(string date, string changes, bool separator = true)
        {
            ImGui.TextUnformatted(date);
            using var colour = ImRaii.PushColor(ImGuiCol.Text, ImGuiUtils.ShadedColor);
            ImGui.TextWrapped(changes);

            if (separator)
                ImGui.Separator();
        }
        using var font = ImRaii.PushFont(UiBuilder.MonoFont);

        DisplayChangelog(
        "2025-03-10",
        "- Added QuestionableGetCurrentStepData()\n" +
        "- Added QuestionableIsQuestLocked()\n" +
        "- Added QuestionableImportQuestPriority()\n" +
        "- Added QuestionableClearQuestPriority()\n" +
        "- Added QuestionableAddQuestPriority()\n" +
        "- Added QuestionableInsertQuestPriority()\n" +
        "- Added QuestionableExportQuestPriority()");

        DisplayChangelog(
        "2025-02-22",
        "- Added GetItemName()\n");

        DisplayChangelog(
        "2025-02-17",
        "- Added IsLeveAccepted()\n");

        DisplayChangelog(
        "2025-02-14",
        "- Reverted the behaviour of scripts running on the game thread for the time being. (PLEASE SUBSCRIBE TO THE TESTING VERSION IN THE FUTURE)\n" +
        "- Added GetPluginVersion()\n");

        DisplayChangelog(
        "2025-02-08",
        "- Added GetShieldPercentage()\n");

        DisplayChangelog(
        "2025-01-30",
        "Excel viewer changes\n" +
        "- In-sheet search works again\n" +
        "- Initial support for large (>100 column) sheets\n" +
        "- Support for subrow sheets\n" +
        "- Layout has been adjusted. Columns now flex to their contents\n");

        DisplayChangelog(
        "2025-01-29",
        "Added GetPlayerContentId()\n" +
        "Added GetPlayerAccountId()\n");

        DisplayChangelog(
        "2025-01-27",
        "- Added GetPartyMemberWorldId()\n" +
        "- Added GetPartyMemberWorldName()\n" +
        "- Added GetTargetWorldId()\n" +
        "- Added GetTargetWorldName()\n" +
        "- Added AllaganTools IPCs\n" +
        "- Added BossMod IPCs\n");

        DisplayChangelog(
        "2024-12-19",
        "- Added /keyitem\n");

        DisplayChangelog(
        "2024-11-27",
        "- Added visland ipcs\n");

        DisplayChangelog(
        "2024-11-25",
        "- Added GetItemCountInSlot()\n");

        DisplayChangelog(
        "2024-11-18",
        "- Blind fix for populating craft action names\n" +
        "- Added sheets back to the excel viewer. It's not very usable at the moment.");

        DisplayChangelog(
        "2024-10-30",
        "- Added various AutoDuty IPC functions()\n");

        DisplayChangelog(
        "2024-10-25",
        "- Added GetItemsToDiscard()\n");

        DisplayChangelog(
        "2024-10-22",
        "- Added GetZoneName()\n" +
        "- Added GetAetherytesInZone()\n" +
        "- Added GetAetheryteName()\n" +
        "- Added GetAetheryteRawPos()\n" +
        "- Added GetPartyLeadIndex()\n" +
        "- Added GetAcceptedQuests()\n" +
        "- Added GetQuestAlliedSociety()\n");

        DisplayChangelog(
        "2024-09-30",
        "- Added InSanctuary()\n");

        DisplayChangelog(
        "2024-09-27",
        "- Added ARFinishCharacterPostProcess()\n" +
        "- Added ARGetCharacterCIDs()\n" +
        "- Added ARGetCharacterData()\n");

        DisplayChangelog(
        "2024-09-26",
        "- Added GetActiveMacroName()\n" +
        "- Added IsMacroRunningOrQueued()\n");

        DisplayChangelog(
        "2024-09-25",
        "- Fixed new AR ipc command names\n");

        DisplayChangelog(
        "2024-09-24",
        "- Added additional autoretainer ipc commands()\n" +
        "- Fixed display of ipc commands in help\n");

        DisplayChangelog(
        "2024-09-21",
        "- Added artisan ipc commands\n" +
        "- Added GetJobExp()\n" +
        "- Added GetFateIconId()\n" +
        "- Added GetFateLevel()\n" +
        "- Added GetFateMaxLevel()\n" +
        "- Added GetFateChain()\n" +
        "- Added IsLevelSynced()\n" +
        "- Added GetFateEventItem()\n" +
        "- Added GetFateState()\n" +
        "- Added TargetClosestFateEnemy()\n" +
        "- Fixed artisan/rsr ipc function names\n");

        DisplayChangelog(
        "2024-09-18",
        "- Added GetItemIdInSlot()\n" +
        "- Added IsFriendOnline()\n" +
        "- Added GetItemIdsInContainer()\n");

        DisplayChangelog(
        "2024-09-11",
        "- Added RSR IPC commands\n");

        DisplayChangelog(
        "2024-09-07",
        "- Added GetFateRadius()\n" +
        "- Removed GetFateNpcObjectId()\n");

        DisplayChangelog(
        "2024-09-04",
        "- Added WeeklyBingoNumPlacedStickers()\n" +
        "- Added QuestionableIsRunning()\n" +
        "- Added QuestionableGetCurrentQuestId()\n");

        DisplayChangelog(
        "2024-09-03",
        "- Normalised distance functions\n");

        DisplayChangelog(
        "2024-08-30",
        "- Added HasWeeklyBingoJournal()\n" +
        "- Added IsWeeklyBingoExpired()\n" +
        "- Added WeeklyBingoNumSecondChancePoints()\n +" +
        "- Added GetWeeklyBingoTaskStatus()\n" +
        "- Added GetWeeklyBingoOrderDataKey()\n" +
        "- Added GetWeeklyBingoOrderDataType()\n" +
        "- Added GetWeeklyBingoOrderDataData()\n" +
        "- Added GetWeeklyBingoOrderDataText()\n" +
        "- Added IsAetheryteUnlocked()\n" +
        "- Added GetAetheryteList()\n");

        DisplayChangelog(
        "2024-08-24",
        "- Fixed MoveItemToContainer()\n");

        DisplayChangelog(
        "2024-08-23",
        "- Added GetFateStartTimeEpoch()\n" +
        "- Added GetFateName()\n" +
        "- Added GetFateIsBonus()\n +" +
        "- Added GetFateNpcObjectId()\n");

        DisplayChangelog(
        "2024-08-21",
        "- Added LifestreamAethernetTeleport()\n" +
        "- Added LifestreamTeleport()\n" +
        "- Added LifestreamTeleportToHome()\n +" +
        "- Added LifestreamTeleportToFC()\n" +
        "- Added LifestreamTeleportToApartment()\n" +
        "- Added LifestreamIsBusy()\n" +
        "- Added LifestreamExecuteCommand()\n" +
        "- Added LifestreamAbort()\n");

        DisplayChangelog(
        "2024-08-14",
        "- Added GetZoneInstance()\n" +
        "- Added IsPauseLoopSet()\n" +
        "- Added IsStopLoopSet()\n +" +
        "- Added GetMonsterNoteRankInfo()\n");

        DisplayChangelog(
        "2024-07-29",
        "- Added dalamud services to lua\n" +
        "- Potentially fixed the /click and /callback commands to allow wait modifiers\n");

        DisplayChangelog(
        "2024-07-26",
        "- Added lua options for requiring paths (thanks OhKannaDuh)\n");

        DisplayChangelog(
        "2024-07-25",
        "- Added options to run a macro after auto retainer's character post process\n");

        DisplayChangelog(
        "2024-07-24",
        "- Added MoveItemToContainer()\n" +
        "- Added GetItemCountInContainer()\n" +
        "- Added GetTradeableWhiteItemIDs()\n");

        DisplayChangelog(
        "2024-07-23",
        "- Added TerritorySupportsMounting()\n" +
        "- Added GetActiveMiniMapGatheringMarker()\n" +
        "- Added missing modifiers to the help menu\n" +
        "- Updated clicks help menu to be accurate (again)\n");

        DisplayChangelog(
        "2024-07-21",
        "- Readded /click support for clicks that took arguments (e.g. RecipeNote Materials)\n");

        DisplayChangelog(
        "2024-07-11",
        "- Fixed IsAddonReady erroneously returning true.\n");

        DisplayChangelog(
        "2024-07-07",
        "- Removed the alt /item option.\n" +
        "- Added an excel sheet browser\n" +
        "- Added JournalDetail clicks\n" +
        "- More /callback safeties and more accurate logging.\n" +
        "- Added SelectIconString entry clicks\n");

        DisplayChangelog(
        "2024-07-05",
        "- Fixed CraftLoop to support the new click system.\n" +
        "- Cleaned up the help menu in relation to the new click system and added some missing commands.\n" +
        "- Commands in the help menu can now be copied to clipboard via clicking.\n" +
        "- Added retainer selection clicks\n");

        DisplayChangelog(
        "2024-07-04",
        "- Fixed /click command. This update dropped clicklib support and click names are slightly different now. Please consult the help menu.\n");

        DisplayChangelog(
        "2024-07-03",
        "- Added the /callback command.\n");

        DisplayChangelog(
        "2024-06-29",
        "- Updated for Dawntrail/APIX.\n" +
        "- Removed the different GetUsedActionID commands\n");

        DisplayChangelog(
        "2024-06-23",
        "- Added GetLastInstanceServerID()\n" +
        "- Added GetLastInstanceZoneID()\n");

        DisplayChangelog(
        "2024-06-21",
        "- Added DropboxStart()\n" +
        "- Added DropboxStop()\n" +
        "- Added DropboxIsBusy()\n" +
        "- Added DropboxGetItemQuantity()\n" +
        "- Added DropboxSetItemQuantity()\n");

        DisplayChangelog(
        "2024-06-19",
        "- Added GetFreeSlotsInContainer()\n" +
        "- Added GetCurrentWorld()\n" +
        "- Added GetHomeWorld()\n" +
        "- Added the relevant sheet info needed for the above commands to Help\n");

        DisplayChangelog(
        "2024-05-23",
        "- Fixes for the last two commands.\n");

        DisplayChangelog(
        "2024-05-18",
        "- Added SetMapFlag()\n" +
        "- Added DistanceBetween()\n");

        DisplayChangelog(
        "2024-05-11",
        "- Added HasTarget()\n");

        DisplayChangelog(
        "2024-05-09",
        "- Added IsAchievementComplete() (requires achievements to be loaded manually)\n" +
        "- Added HasFlightUnlocked()\n");

        DisplayChangelog(
        "2024-05-06",
        "- Added HasPlugin()\n" +
        "- Added SetNodeText()\n");

        DisplayChangelog(
        "2024-04-22",
        "- IsNodeVisible() supports checking arbitrarily nested nodes. (breaking change from requiring node positions to node ids)\n" +
        "- Added GetHP()\n" +
        "- Added GetMaxHP()\n" +
        "- Added GetMP()\n" +
        "- Added GetMaxMP()\n");

        DisplayChangelog(
        "2024-04-18",
        "- Fixed some instances where certain entity functions return null\n");

        DisplayChangelog(
        "2024-04-15",
        "- Added GetTargetHitboxRadius()\n" +
        "- Added GetObjectHitboxRadius()\n");

        DisplayChangelog(
        "2024-04-14",
        "- Added GetTargetHuntRank()\n" +
        "- Added GetObjectHuntRank()\n");

        DisplayChangelog(
        "2024-04-11",
        "- Fixed a couple pandora IPC functions\n");

        DisplayChangelog(
        "2024-04-10",
        "- Fixed GetPassageLocation() return type\n" +
        "- Changed TeleportToGCTown() to use tickets optionally\n" +
        "- Added the cfg argument to the main plugin command\n" +
        "- Added SetSNDProperty()\n" +
        "- Added GetSNDProperty()\n" +
        "- Fixed the log functions to take in any object as opposed to only strings.\n");

        DisplayChangelog(
        "2024-04-08",
        "- Added GetObjectDataID()\n" +
        "- Added GetBronzeChestLocations()\n" +
        "- Added GetSilverChestLocations()\n" +
        "- Added GetGoldChestLocations()\n" +
        "- Added GetMimicChestLocations()\n" +
        "- Added GetPassageLocation()\n" +
        "- Added GetTrapLocations()\n" +
        "- Added GetDDPassageProgress()\n");

        DisplayChangelog(
        "2024-04-07",
        "- Added party index support to /target (i.e. /target <2>)\n");

        DisplayChangelog(
        "2024-04-05",
        "- Added GetBuddyTimeRemaining()\n" +
        "- Added IsTargetMounted()\n" +
        "- Added IsPartyMemberMounted()\n" +
        "- Added IsObjectMounted()\n" +
        "- Added IsTargetInCombat()\n" +
        "- Added IsObjectInCombat()\n" +
        "- Added IsPartyMemberInCombat()\n");

        DisplayChangelog(
        "2024-03-28",
        "- Added /interact\n");

        DisplayChangelog(
        "2024-03-26",
        "- Added DoesObjectExist()\n" +
        "- Fixed window position resetting after each update.\n");

        DisplayChangelog(
        "2024-03-25",
        "- Updated TeleportToGCTown to use aetheryte tickets if you have them.()\n" +
        "- Updated QueryMeshPointOnFloor to latest navmesh IPC\n");

        DisplayChangelog(
        "2024-03-15",
        "- Added GetTargetFateID()\n" +
        "- Added GetFocusTargetFateID()\n" +
        "- Added GetObjectFateID()\n");

        DisplayChangelog(
        "2024-03-07",
        "- Added GetPartyMemberName()\n");

        DisplayChangelog(
        "2024-03-06",
        "- Further improvements to require(). Support for absolute and relative paths, or macros (thanks stjornur!)\n");

        DisplayChangelog(
        "2024-03-05",
        "- Added TargetHasStatus()\n" +
        "- Added FocusTargetHasStatus()\n" +
        "- Added ObjectHasStatus()\n" +
        "- Added GetPartyMemberRawXPos()\n" +
        "- Added GetPartyMemberRawYPos()\n" +
        "- Added GetPartyMemberRawZPos()\n" +
        "- Added GetDistanceToPartyMember()\n" +
        "- Added IsPartyMemberCasting()\n" +
        "- Added GetPartyMemberActionID()\n" +
        "- Added GetPartyMemberUsedActionID()\n" +
        "- Added GetPartyMemberHP()\n" +
        "- Added GetPartyMemberMaxHP()\n" +
        "- Added GetPartyMemberHPP()\n" +
        "- Added GetPartyMemberRotation()\n" +
        "- Added PartyMemberHasStatus()\n");

        DisplayChangelog(
        "2024-03-04",
        "- Added LogInfo()\n" +
        "- Added LogDebug()\n" +
        "- Added LogVerbose()\n" +
        "- Added counter node support to GetNodeText()\n" +
        "- Navmesh ipc fixes\n" +
        "- Added support for require() to require other macros (thanks stjornur!)\n");

        DisplayChangelog(
        "2024-03-03",
        "- Added /equipitem()\n" +
        "- Added NavPathfind()\n" +
        "- Changed QueryMeshNearestPointX()\n" +
        "- Changed QueryMeshNearestPointY()\n" +
        "- Changed QueryMeshNearestPointZ()\n" +
        "- Added QueryMeshPointOnFloorX()\n" +
        "- Added QueryMeshPointOnFloorY()\n" +
        "- Added QueryMeshPointOnFloorZ()\n" +
        "- Changed PathMoveTo()\n" +
        "- Removed PathFlyTo()\n" +
        "- Added PathfindAndMoveTo()\n" +
        "- Added PathfindInProgress()\n");

        DisplayChangelog(
        "2024-02-28",
        "- Added QueryMeshNearestPointX()\n" +
        "- Added QueryMeshNearestPointY()\n" +
        "- Added QueryMeshNearestPointZ()\n" +
        "- Added PathGetAlignCamera()\n" +
        "- Added PathSetAlignCamera()\n");

        DisplayChangelog(
        "2024-02-27",
        "- Added ClearTarget()\n" +
        "- Added ClearFocusTarget()\n" +
        "- Added /targetenemy\n" +
        "- Added IsObjectCasting()\n" +
        "- Added GetObjectActionID()\n" +
        "- Added GetObjectUsedActionID()\n" +
        "- Added GetObjectHP()\n" +
        "- Added GetObjectMaxHP()\n" +
        "- Added GetObjectHPP()\n" +
        "- Added GetDistanceToFocusTarget()\n" +
        "- Added GetTargetRotation()\n" +
        "- Added GetFocusTargetRotation()\n" +
        "- Added GetObjectRotation()\n" +
        "- Fixed TargetClosestEnemy()\n");

        DisplayChangelog(
        "2024-02-26",
        "- Added NavIsReady()\n" +
        "- Added NavBuildProgress()\n" +
        "- Added NavReload()\n" +
        "- Added NavRebuild()\n" +
        "- Added NavIsAutoLoad()\n" +
        "- Added NavSetAutoLoad()\n" +
        "- Added PathMoveTo()\n" +
        "- Added PathFlyTo()\n" +
        "- Added PathStop()\n" +
        "- Added PathIsRunning()\n" +
        "- Added PathNumWaypoints()\n" +
        "- Added PathGetMovementAllowed()\n" +
        "- Added PathSetMovementAllowed()\n" +
        "- Added PathGetTolerance()\n" +
        "- Added PathSetTolerance()\n" +
        "- Added TargetClosestEnemy()\n" +
        "- Added GetTargetObjectKind()\n" +
        "- Added GetTargetSubKind()\n" +
        "- Fixed GetNearbyObjectNames() to return sorted by distance\n");

        DisplayChangelog(
        "2024-02-24",
        "- Added SetDFLanguageJ()\n" +
        "- Added SetDFLanguageE()\n" +
        "- Added SetDFLanguageD()\n" +
        "- Added SetDFLanguageF()\n" +
        "- Added SetDFJoinInProgress()\n" +
        "- Added SetDFUnrestricted()\n" +
        "- Added SetDFLevelSync()\n" +
        "- Added SetDFMinILvl()\n" +
        "- Added SetDFSilenceEcho()\n" +
        "- Added SetDFExplorerMode()\n" +
        "- Added SetDFLimitedLeveling()\n" +
        "- Added GetDiademAetherGaugeBarCount()\n" +
        "- Added IsPlayerAvailable()\n");

        DisplayChangelog(
        "2024-02-22",
        "- Added ExecuteAction()\n" +
        "- Added ExecuteGeneralAction()\n" +
        "- Added GetFocusTargetName()\n" +
        "- Added GetFocusTargetRawXPos()\n" +
        "- Added GetFocusTargetRawYPos()\n" +
        "- Added GetFocusTargetRawZPos()\n" +
        "- Added IsFocusTargetCasting()\n" +
        "- Added GetFocusTargetActionID()\n" +
        "- Added GetFocusTargetUsedActionID()\n" +
        "- Added GetFocusTargetHP()\n" +
        "- Added GetFocusTargetMaxHP()\n" +
        "- Added GetFocusTargetHPP()\n" +
        "- Fixed collectables not counting in item counts\n");

        DisplayChangelog(
        "2024-02-20",
        "- Added GetNearbyObjectNames()\n" +
        "- Added GetFlagZone()\n" +
        "- Added GetAccursedHoardRawX()\n" +
        "- Added GetAccursedHoardRawY()\n" +
        "- Added GetAccursedHoardRawZ()\n" +
        "- Fixed OpenRegularDuty\n");

        DisplayChangelog(
        "2024-02-17",
        "- Added GetPenaltyRemainingInMinutes()\n" +
        "- Added GetMaelstromGCRank()\n" +
        "- Added GetFlamesGCRank()\n" +
        "- Added GetAddersGCRank()\n" +
        "- Added SetMaelstromGCRank()\n" +
        "- Added SetFlamesGCRank()\n" +
        "- Added SetAddersGCRank()\n");

        DisplayChangelog(
        "2024-02-13",
        "- Added GetTargetMaxHP()\n" +
        "- Fixed GetTargetHPP()\n");

        DisplayChangelog(
        "2024-02-11",
        "- Added the ability to toggle ending scripts when encountering certain errors.\n" +
        "- Added an alternative system for /useitem\n");

        DisplayChangelog(
        "2024-02-09",
        "- Added GetCurrentBait()\n" +
        "- Added GetLimitBreakCurrentValue()\n" +
        "- Added GetLimitBreakBarValue()\n" +
        "- Added GetLimitBreakBarCount()\n");

        DisplayChangelog(
        "2024-02-07",
        "- Added more global variables\n");

        DisplayChangelog(
        "2024-02-06",
        "- Added DeleteAllAutoHookAnonymousPresets()\n" +
        "- Added ARGetRegisteredRetainers()\n" +
        "- Added ARGetRegisteredEnabledRetainers()\n" +
        "- Added ARSetSuppressed()\n");

        DisplayChangelog(
        "2024-02-05",
        "- Added many global variables usable in any script now. See help menu for a brief explanation.\n");

        DisplayChangelog(
         "2024-02-04",
         "- Fixed the AR character query commands to only check enabled characters\n" +
         "- Added PauseTextAdvance()\n" +
         "- Added RestoreTextAdvance()\n" +
         "- Added PandoraGetFeatureEnabled()\n" +
         "- Added PandoraGetFeatureConfigEnabled()\n" +
         "- Added PandoraSetFeatureState()\n" +
         "- Added PandoraSetFeatureConfigState()\n" +
         "- Added PandoraPauseFeature()\n\n" +
         "- Added GetClipboard()\n" +
         "- Added SetClipboard()\n" +
         "- Added CrashTheGame()\n" +
         "- Added IsPlayerOccupied()\n");

        DisplayChangelog(
         "2024-02-01",
         "- Added GetTargetHP()\n" +
         "- Added GetTargetHPP()\n\n" +
         "- Added RequestAchievementProgress()\n" +
         "- Added GetRequestedAchievementProgress()\n\n" +
         "- Added GetContentTimeLeft()\n" +
         "- Replaced GetCurrentOceanFishingDuration() with GetCurrentOceanFishingZoneTimeLeft()\n" +
         "- Added GetCurrentOceanFishingScore()\n" +
         "- Added GetCurrentOceanFishingTimeOfDay()\n" +
         "- Added GetCurrentOceanFishingMission1Goal()\n" +
         "- Added GetCurrentOceanFishingMission2Goal()\n" +
         "- Added GetCurrentOceanFishingMission3Goal()\n" +
         "- Added GetCurrentOceanFishingMission1Name()\n" +
         "- Added GetCurrentOceanFishingMission2Name()\n" +
         "- Added GetCurrentOceanFishingMission3Name()\n\n" +
         "- Added SetAutoHookState()\n" +
         "- Added SetAutoHookAutoGigState()\n" +
         "- Added SetAutoHookAutoGigSize()\n" +
         "- Added SetAutoHookAutoGigSpeed()\n" +
         "- Added SetAutoHookPreset()\n" +
         "- Added UseAutoHookAnonymousPreset()\n" +
         "- Added DeleteSelectedAutoHookPreset()\n");

        DisplayChangelog(
         "2024-01-30",
         "- Added GetObjectRawXPos()\n" +
         "- Added GetObjectRawYPos()\n" +
         "- Added GetObjectRawZPos()\n" +
         "- Added GetCurrentOceanFishingRoute()\n" +
         "- Added GetCurrentOceanFishingStatus()\n" +
         "- Added GetCurrentOceanFishingZone()\n" +
         "- Added GetCurrentOceanFishingDuration()\n" +
         "- Added GetCurrentOceanFishingTimeOffset()\n" +
         "- Added GetCurrentOceanFishingWeatherID()\n" +
         "- Added OceanFishingIsSpectralActive()\n" +
         "- Added GetCurrentOceanFishingMission1Type()\n" +
         "- Added GetCurrentOceanFishingMission2Type()\n" +
         "- Added GetCurrentOceanFishingMission3Type()\n" +
         "- Added GetCurrentOceanFishingMission1Progress()\n" +
         "- Added GetCurrentOceanFishingMission2Progress()\n" +
         "- Added GetCurrentOceanFishingMission3Progress()\n" +
         "- Added GetCurrentOceanFishingPoints()\n" +
         "- Added GetCurrentOceanFishingTotalScore()\n" +
         "- Added \"Ocean Fishing Routes\" to the Game Data tab");

        DisplayChangelog(
         "2024-01-29",
         "- Added TeleportToGCTown()\n" +
         "- Added GetPlayerGC()\n" +
         "- Added GetActiveFates()\n" +
         "- Added ARGetRegisteredCharacters()\n" +
         "- Added ARGetRegisteredEnabledCharacters()\n" +
         "- Added IsVislandRouteRunning()\n" +
         "- Added GetToastNodeText()\n" +
         "- Added PauseYesAlready()\n" +
         "- Added RestoreYesAlready()\n\n" +
         "- Added OpenRouletteDuty()\n" +
         "- Added OpenRegularDuty()\n" +
         "- Added CFC and Roulette entries to the GameData section in help for using the above two functions\n");

        DisplayChangelog(
          "2024-01-27",
          "- Added IsInFate()\n" +
          "- Added GetNearestFate()\n" +
          "- Added GetFateDuration()\n" +
          "- Added GetFateHandInCount()\n" +
          "- Added GetFateLocationX()\n" +
          "- Added GetFateLocationY()\n" +
          "- Added GetFateLocationZ()\n" +
          "- Added GetFateProgress()\n\n" +
          "- Added GetCurrentEorzeaTimestamp()\n" +
          "- Added GetCurrentEorzeaSecond()\n" +
          "- Added GetCurrentEorzeaMinute()\n" +
          "- Added GetCurrentEorzeaHour()\n\n" +
          "- Added GetDistanceToObject()\n");

        DisplayChangelog(
          "2024-01-26",
          "- Added GetRecastTimeElapsed()\n" +
          "- Added GetRealRecastTimeElapsed()\n" +
          "- Added GetRecastTime()\n" +
          "- Added GetRealRecastTime()\n" +
          "- Added GetSpellCooldown()\n" +
          "- Added GetRealSpellCooldown()\n" +
          "- Added GetSpellCooldownInt()\n" +
          "- Added GetActionStackCount()\n\n" +
          "- Added GetStatusStackCount()\n" +
          "- Added GetStatusTimeRemaining()\n" +
          "- Added GetStatusSourceID()\n\n" +
          "- Added GetFCGrandCompany()\n" +
          "- Added GetFCOnlineMembers()\n" +
          "- Added GetFCTotalMembers()\n");

        DisplayChangelog(
            "2024-01-25",
            "- Added IsTargetCasting()\n" +
            "- Added GetTargetActionID()\n" +
            "- Added GetTargetUsedActionID()\n" +
            "- Changed the Lua menu to be more dynamic with listing functions\n");

        DisplayChangelog(
            "2024-01-24",
            "- Added GetActiveWeatherID()\n" +
            "- Added a section in the help menu to decipher weather IDs.\n");

        DisplayChangelog(
            "2024-01-23",
            "- Added new <list.listIndex> modifier. Used for /target where you're searching for targets with the same name.\n");

        DisplayChangelog(
            "2024-01-22",
            "- Added ARAnyWaitingToBeProcessed()\n" +
            "- Added ARRetainersWaitingToBeProcessed()\n" +
            "- Added ARSubsWaitingToBeProcessed()\n");

        DisplayChangelog(
            "2024-01-21",
            "- Added GetInventoryFreeSlotCount()\n");

        DisplayChangelog(
          "2024-01-18",
          "- Added GetTargetRawXPos()\n" +
          "- Added GetTargetRawYPos()\n" +
          "- Added GetTargetRawZPos()\n" +
          "- Added GetDistanceToTarget()\n" +
          "- Added GetFlagXCoord()\n" +
          "- Added GetFlagYCoord()\n");

        DisplayChangelog(
          "2024-01-04",
          "- Added IsNodeVisible().\n");

        DisplayChangelog(
           "2023-12-22",
           "- Updated the GetRaw coordinate functions to take in an object name or party member position.\n");

        DisplayChangelog(
           "2023-12-12",
           "- Added IsQuestAccepted()\n" +
           "- Added IsQuestComplete()\n" +
           "- Added GetQuestSequence()\n" +
           "- Added GetQuestIDByName()\n" +
           "- Added GetQuestNameByID()\n" +
           "- Added GetNodeListCount()\n" +
           "- Added GetTargetName()\n");

        DisplayChangelog(
           "2023-11-06",
           "- Added GetLevel()\n" +
           "- Added \"Game Data\" tab to the help menu.\n" +
           "- Added GetGp() and GetMaxGp() (thanks nihilistzsche)\n" +
           "- Added GetFCRank()\n");

        DisplayChangelog(
           "2023-11-23",
           "- Added GetPlayerRawXPos()\n" +
           "- Added GetPlayerRawZPos()\n" +
           "- Added GetPlayerRawYPos()\n" +
           "- Added GetDistanceToPoint()\n");

        DisplayChangelog(
           "2023-11-23",
           "- Fix for IsMoving() to detect forms of automated movement.\n");

        DisplayChangelog(
           "2023-11-21",
           "- Added IsMoving()\n");

        DisplayChangelog(
           "2023-11-20",
           "- Macros will now automatically prefix non-command text lines with /echo. To send a message to chat now requires you to prefix it with the appropiate chat channel command.\n");

        DisplayChangelog(
           "2023-11-17",
           "- Added /hold\n" +
           "- Added /release.\n" +
           "- Updated help documentation for lua commands.\n");

        DisplayChangelog(
           "2023-11-15",
           "- Added GetClassJobId()\n");

        DisplayChangelog(
           "2023-11-14",
           "- Fixed the targeting system to ignore untargetable objects.\n" +
           "- Fixed the targeting system to prefer closest matches.\n" +
           "- Added an option to not use SND's targeting system.\n" +
           "- Added an option to not stop the macro if a target is not found.\n");

        DisplayChangelog(
           "2023-11-11",
           "- The main command is now /somethingneeddoing. The aliases are /snd and /pcraft.\n" +
           "- Changed how the /send command works internally for compatibility with XIVAlexander.\n");

        DisplayChangelog(
           "2023-11-08",
           "- Added GetGil()\n");

        DisplayChangelog(
           "2023-11-06",
           "- Added IsLocalPlayerNull()\n" +
           "- Added IsPlayerDead()\n" +
           "- Added IsPlayerCasting()\n");

        DisplayChangelog(
           "2023-11-05",
           "- Added LeaveDuty().\n");

        DisplayChangelog(
           "2023-11-04",
           "- Added GetProgressIncrease(uint actionID). Returns numerical amount of progress increase a given action will cause.\n" +
           "- Added GetQualityIncrease(uint actionID). Returns numerical amount of quality increase a given action will cause.\n");

        DisplayChangelog(
           "2023-10-24",
           "- Changed GetCharacterCondition() to take in an int instead of a string.\n" +
           "- Added a list of conditions to the help menu.\n");

        DisplayChangelog(
           "2023-10-21",
           "- Added an optional bool to pass to GetCharacterName to return the world name in addition.\n");

        DisplayChangelog(
           "2023-10-20",
           "- Changed GetItemCount() to support HQ items. Default behaviour includes both HQ and NQ. Pass false to the function to do only NQ.\n");

        DisplayChangelog(
            "2023-10-17",
            "- Added a Deliveroo IPC along with the DeliverooIsTurnInRunning() lua command.\n");

        DisplayChangelog(
            "2023-10-13",
            "- Added a small delay to /loop so that very short looping macros will not crash the client.\n" +
            "- Added a lock icon to the window bar to the lock the window position.\n");

        DisplayChangelog(
            "2023-10-10",
            "- Added IsInZone() lua command. Pass the zoneID, returns a bool.\n" +
            "- Added GetZoneID() lua command. Gets the zoneID of the current zone.\n" +
            "- Added GetCharacterName() lua command.\n" +
            "- Added GetItemCount() lua command. Pass the itemID, get count.\n");

        DisplayChangelog(
            "2023-05-31",
            "- Added the index modifier\n");

        DisplayChangelog(
            "2022-08-22",
            "- Added use item command.\n" +
            "- Updated Lua method GetNodeText to get nested nodes.\n");

        DisplayChangelog(
            "2022-07-23",
            "- Fixed Lua methods (oops).\n" +
            "- Add Lua methods to get SelectString and SelectIconString text.\n");

        DisplayChangelog(
            "2022-06-10",
            "- Updated the Send command to allow for '+' delimited modifiers.\n" +
            "- Added a CraftLoop template feature to allow for customization of the loop capability.\n" +
            "- Added an option to customize the error/notification beeps.\n" +
            "- Added Lua scripting available as a button next to the CraftLoop buttons.\n" +
            "- Updated the help window options tab to use collapsing headers.\n");

        DisplayChangelog(
            "2022-05-13",
            "- Added a /requirequality command to require a certain amount of quality before synthesizing.\n" +
            "- Added a /requirerepair command to pause when an equipped item is broken.\n" +
            "- Added a /requirespiritbond command to pause when an item can have materia extracted.");

        DisplayChangelog(
            "2022-04-26",
            "- Added a max retries option for when an action command does not receive a response within the alloted limit, typically due to lag.\n" +
            "- Added a noisy errors option to play some beeps when a detectable error occurs.");

        DisplayChangelog(
            "2022-04-25",
            "- Added a /recipe command to open the recipe book to a specific recipe (ty marimelon).\n");

        DisplayChangelog(
            "2022-04-18",
            "- Added a /craft command to act as a gate at the start of a macro, rather than specifying the number of loops at the end.\n" +
            "- Removed the \"Loop Total\" option, use the /craft or /gate command instead of this jank.");

        DisplayChangelog(
            "2022-04-04",
            "- Added macro CraftLoop loop UI options to remove /loop boilerplate (ty darkarchon).\n");

        DisplayChangelog(
            "2022-04-03",
            "- Fixed condition modifier to work with non-English letters/characters.\n" +
            "- Added an option to disable monospaced font for JP users.\n");

        DisplayChangelog(
            "2022-03-03",
            "- Added an intelligent wait option that waits until your crafting action is complete, rather than what is in the <wait> modifier.\n" +
            "- Updated the <condition> modifier to accept a comma delimited list of names.\n");

        DisplayChangelog(
            "2022-02-02",
            "- Added /send help pane.\n" +
            "- Fixed /loop echo commands not being sent to the echo channel.\n");

        DisplayChangelog(
            "2022-01-30",
            "- Added a \"Step\" button to the control bar that lets you skip to the next step when a macro is paused.\n");

        DisplayChangelog(
            "2022-01-25",
            "- The help menu now has an options pane.\n" +
            "- Added an option to disable skipping craft actions when not crafting or at max progress.\n" +
            "- Added an option to disable the automatic quality increasing action skip, when at max quality.\n" +
            "- Added an option to treat /loop as the total iterations, rather than the amount to repeat.\n" +
            "- Added an option to always treat /loop commands as having an <echo> modifier.\n");

        DisplayChangelog(
            "2022-01-16",
            "- The help menu now has a /click listing.\n" +
            "- Various quality increasing skills are skipped when at max quality. Please open an issue if you encounter issues with this.\n" +
            "- /loop # will reset after reaching the desired amount of loops. This allows for nested looping. You can test this with the following:\n" +
            "    /echo 111 <wait.1>\n" +
            "    /loop 1\n" +
            "    /echo 222 <wait.1>\n" +
            "    /loop 1\n" +
            "    /echo 333 <wait.1>\n");

        DisplayChangelog(
            "2022-01-01",
            "- Various /pcraft commands have been added. View the help menu for more details.\n" +
            "- There is also a help menu.\n",
            false);
    }
}
