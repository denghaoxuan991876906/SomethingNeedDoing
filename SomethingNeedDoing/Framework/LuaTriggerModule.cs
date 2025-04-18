namespace SomethingNeedDoing.Framework;
public class TriggerModule : LuaModuleBase
{
    private TriggerEventArgs? _currentTriggerArgs;

    public override string ModuleName => "Trigger";

    public void SetTriggerArgs(TriggerEventArgs? args) => _currentTriggerArgs = args;

    [LuaFunction]
    public string? GetEventType() => _currentTriggerArgs?.EventType.ToString();

    [LuaFunction]
    public object? GetEventData(string key) => _currentTriggerArgs?.EventData.TryGetValue(key, out var value) == true ? value : null;

    [LuaFunction]
    public Dictionary<string, object> GetAllEventData() => _currentTriggerArgs?.EventData ?? [];
}
