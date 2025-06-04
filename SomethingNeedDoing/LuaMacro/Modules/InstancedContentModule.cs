using FFXIVClientStructs.FFXIV.Client.Game.Event;
using SomethingNeedDoing.LuaMacro.Wrappers;

namespace SomethingNeedDoing.LuaMacro.Modules;
/// <summary>
/// Module for deep dungeons and forays
/// </summary>
public unsafe class InstancedContentModule : LuaModuleBase
{
    public override string ModuleName => "InstancedContent";

    [LuaFunction] public float ContentTimeLeft => EventFramework.Instance()->GetInstanceContentDirector()->ContentDirector.ContentTimeLeft;

    [LuaFunction] public OceanFishingWrapper OceanFishing => new();

    public class OccultCrescentWrapper
    {
        //public List<DynamicEventWrapper> Events
        //    => PublicContentOccultCrescent.GetInstance()->DynamicEventContainer.Events.ToArray()
        //        .Where(ce => ce.State != DynamicEventState.Inactive)
        //        .Select(ce => ce.EventType)
        //        .ToList();
    }

    public class DynamicEventWrapper
    {

    }
}
