using SomethingNeedDoing.Core.Events;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.Utils;
public static class ConfigMacroExtensions
{
    public static void Rename(this ConfigMacro macro, string newName)
    {
        macro.Name = newName;
        C.Save();
    }

    public static void Duplicate(this ConfigMacro macro, string? newName = null)
    {
        var duplicate = new ConfigMacro
        {
            Name = newName ?? $"{macro.Name} (Copy)",
            Type = macro.Type,
            Content = macro.Content,
            FolderPath = macro.FolderPath
        };
        C.Macros.Add(duplicate);
    }
    public static void Delete(this ConfigMacro macro) => C.Macros.RemoveAll(m => m.Id == macro.Id);
    public static void Move(this ConfigMacro macro, string folderPath) => macro.FolderPath = folderPath;
    public static void Start(this IMacro macro, IMacroScheduler scheduler, TriggerEventArgs? args = null)
        => _ = scheduler.StartMacro(macro, args);
    public static void Stop(this IMacro macro, IMacroScheduler scheduler)
        => scheduler.StopMacro(macro.Id);
    public static void Pause(this IMacro macro, IMacroScheduler scheduler)
        => scheduler.PauseMacro(macro.Id);
    public static void Resume(this IMacro macro, IMacroScheduler scheduler)
        => scheduler.ResumeMacro(macro.Id);
    public static void UpdateLastModified(this IMacro macro) => macro.Metadata.LastModified = DateTime.Now;
}
