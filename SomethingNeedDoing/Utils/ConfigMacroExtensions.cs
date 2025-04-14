namespace SomethingNeedDoing.Utils;
public static class ConfigMacroExtensions
{
    public static void Rename(this ConfigMacro macro, string newName) => macro.Name = newName;
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
    public static void Start(this ConfigMacro macro) => _ = Service.MacroScheduler.StartMacro(macro);
    public static void Stop(this ConfigMacro macro) => _ = Service.MacroScheduler.StopMacro(macro.Id);
}
