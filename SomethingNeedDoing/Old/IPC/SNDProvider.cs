using ECommons.EzIpcManager;
using SomethingNeedDoing.Old.Misc;

namespace SomethingNeedDoing.Old.IPC;
public class SNDProvider
{
    public SNDProvider() => EzIPC.Init(this);

    [EzIPC] public bool IsRunning => Service.MacroManager.State == LoopState.Running;
    [EzIPC] public void Pause() => Service.MacroManager.Pause();
    [EzIPC] public void Resume() => Service.MacroManager.Resume();
    [EzIPC] public void Stop() => Service.MacroManager.Stop();
    [EzIPC] public void RunByName(string macroName) => C.GetAllNodes().OfType<MacroNode>().FirstOrDefault(macro => macro?.Name == macroName, null)?.Run();
}
