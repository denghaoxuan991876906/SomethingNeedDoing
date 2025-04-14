using ECommons.EzIpcManager;

namespace SomethingNeedDoing.Old.IPC;

#nullable disable
public class BossMod
{
    public BossMod() => EzIPC.Init(this, "BossMod");

    [EzIPC("Presets.%m", true)] public readonly Func<string, string> Get;
    [EzIPC("Presets.%m", true)] public readonly Func<string, bool, bool> Create;
    [EzIPC("Presets.%m", true)] public readonly Func<string, bool> Delete;
    [EzIPC("Presets.%m", true)] public readonly Func<string> GetActive;
    [EzIPC("Presets.%m", true)] public readonly Func<string, bool> SetActive;
    [EzIPC("Presets.%m", true)] public readonly Func<bool> ClearActive;
    [EzIPC("Presets.%m", true)] public readonly Func<bool> GetForceDisabled;
    [EzIPC("Presets.%m", true)] public readonly Func<bool> SetForceDisabled;
    [EzIPC("Presets.%m", true)] public readonly Func<string, string, string, string, bool> AddTransientStrategy;
    [EzIPC("Presets.%m", true)] public readonly Func<string, string, string, string, int, bool> AddTransientStrategyTargetEnemyOID;
}
