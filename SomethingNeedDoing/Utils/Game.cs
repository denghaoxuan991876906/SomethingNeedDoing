using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace SomethingNeedDoing.Utils;
public static unsafe class Game
{
    public static bool CanUseAction(ActionType actionType, uint actionId) => ActionManager.Instance()->GetActionStatus(actionType, actionId) == 0;
    public static bool CanUseCraftAction(uint actionId) => CanUseAction(actionId >= 100000 ? ActionType.CraftAction : ActionType.Action, actionId);

    public static class Crafting
    {
        public static AddonMaster.Synthesis.Condition GetCondition()
            => TryGetAddonMaster<AddonMaster.Synthesis>(out var addon) ? addon.Reader.Condition : AddonMaster.Synthesis.Condition.Unknown;

        public static bool IsCrafting() => Svc.Condition[ConditionFlag.Crafting] && !Svc.Condition[ConditionFlag.PreparingToCraft];
        public static bool IsMaxProgress() => TryGetAddonMaster<AddonMaster.Synthesis>(out var am) && am.Reader.IsMaxProgress;
        public static bool IsMaxQuality() => TryGetAddonMaster<AddonMaster.Synthesis>(out var am) && am.Reader.IsMaxQuality;

        public static CraftState GetCraftState()
        {
            if (TryGetAddonMaster<AddonMaster.Synthesis>(out var addon))
                return new()
                {
                    Index = (int)addon.Reader.StepCount,
                    Progress = (int)addon.Reader.Progress,
                    Quality = (int)addon.Reader.Quality,
                    Durability = (int)addon.Reader.Durability,
                    RemainingCP = (int)Player.Object.CurrentCp,
                    Condition = addon.Reader.Condition,
                    IQStacks = Player.Status.FirstOrDefault(s => s.StatusId == (uint)Buffs.InnerQuiet)?.Param ?? 0,
                    WasteNotLeft = Player.Status.FirstOrDefault(s => s.StatusId == (uint)Buffs.WasteNot)?.Param ?? 0,
                    ManipulationLeft = Player.Status.FirstOrDefault(s => s.StatusId == (uint)Buffs.Manipulation)?.Param ?? 0,
                    GreatStridesLeft = Player.Status.FirstOrDefault(s => s.StatusId == (uint)Buffs.GreatStrides)?.Param ?? 0,
                    InnovationLeft = Player.Status.FirstOrDefault(s => s.StatusId == (uint)Buffs.Innovation)?.Param ?? 0,
                    VenerationLeft = Player.Status.FirstOrDefault(s => s.StatusId == (uint)Buffs.Veneration)?.Param ?? 0,
                    MuscleMemoryLeft = Player.Status.FirstOrDefault(s => s.StatusId == (uint)Buffs.MuscleMemory)?.Param ?? 0,
                    FinalAppraisalLeft = Player.Status.FirstOrDefault(s => s.StatusId == (uint)Buffs.FinalAppraisal)?.Param ?? 0,
                    //CarefulObservationLeft = CanUseCraftAction(),
                    HeartAndSoulActive = Player.Status.FirstOrDefault(s => s.StatusId == (uint)Buffs.HeartAndSoul) is not null,
                    //HeartAndSoulAvailable = CanUseCraftAction(),
                    ExpedienceLeft = Player.Status.FirstOrDefault(s => s.StatusId == (uint)Buffs.Expedience)?.Param ?? 0,
                    QuickInnoLeft = Player.Status.FirstOrDefault(s => s.StatusId == (uint)Buffs.Innovation)?.Param ?? 0,
                    //QuickInnoAvailable = CanUseCraftAction(),
                    //TrainedPerfectionAvailable = CanUseCraftAction(),
                    TrainedPerfectionActive = Player.Status.FirstOrDefault(s => s.StatusId == (uint)Buffs.TrainedPerfection) is not null,
                };
            throw new ArgumentNullException("Synthesis addon null");
        }

        public enum Buffs : uint
        {
            InnerQuiet = 251,
            Innovation = 2189,
            Veneration = 2226,
            GreatStrides = 254,
            Manipulation = 1164,
            WasteNot = 252,
            WasteNot2 = 257,
            FinalAppraisal = 2190,
            MuscleMemory = 2191,
            HeartAndSoul = 2665,
            Expedience = 3812,
            TrainedPerfection = 3813,
        }

        public record class CraftState
        {
            public int Index;
            public int Progress;
            public int Quality;
            public int Durability;
            public int RemainingCP;
            public AddonMaster.Synthesis.Condition Condition;
            public int IQStacks;
            public int WasteNotLeft;
            public int ManipulationLeft;
            public int GreatStridesLeft;
            public int InnovationLeft;
            public int VenerationLeft;
            public int MuscleMemoryLeft;
            public int FinalAppraisalLeft;
            //public int CarefulObservationLeft;
            public bool HeartAndSoulActive;
            //public bool HeartAndSoulAvailable;
            public int ExpedienceLeft;
            public int QuickInnoLeft;
            //public bool QuickInnoAvailable;
            //public bool TrainedPerfectionAvailable;
            public bool TrainedPerfectionActive;
        }
    }
}
