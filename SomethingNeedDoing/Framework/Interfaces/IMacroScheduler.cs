using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Framework;
/// <summary>
/// Interface for macro scheduling and control.
/// </summary>
public interface IMacroScheduler
{
    public Task StartMacro(IMacro macro);
    public Task PauseMacro(string macroId);
    public Task ResumeMacro(string macroId);
    public Task StopMacro(string macroId);
    public void CheckLoopPause(string macroId) { }
    public void CheckLoopStop(string macroId) { }
}
