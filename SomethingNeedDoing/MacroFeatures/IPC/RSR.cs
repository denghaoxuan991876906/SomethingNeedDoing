using ECommons.EzIpcManager;

namespace SomethingNeedDoing.MacroFeatures.IPC;
#nullable disable
public class RSR
{
    public const string Name = "RotationSolverReborn";
    public RSR() => EzIPC.Init(this, Name, SafeWrapper.AnyException);

    [EzIPC] public Action<uint> AddPriorityNameID;
    [EzIPC] public Action<uint> RemovePriorityNameID;
    [EzIPC] public Action<uint> AddBlacklistNameID;
    [EzIPC] public Action<uint> RemoveBlacklistNameID;
    [EzIPC] public Action<StateCommandType> ChangeOperatingMode;
    [EzIPC] public Action<SpecialCommandType> TriggerSpecialState;

    public enum SpecialCommandType : byte
    {
        EndSpecial,
        HealArea,
        HealSingle,
        DefenseArea,
        DefenseSingle,
        DispelStancePositional,
        RaiseShirk,
        MoveForward,
        MoveBack,
        AntiKnockback,
        Burst,
        Speed,
        LimitBreak,
        NoCasting,
    }

    public enum StateCommandType : byte
    {
        Off,
        Auto,
        Manual,
    }

    public enum OtherCommandType : byte
    {
        Settings,
        Rotations,
        DoActions,
        ToggleActions,
        NextAction,
    }
}
