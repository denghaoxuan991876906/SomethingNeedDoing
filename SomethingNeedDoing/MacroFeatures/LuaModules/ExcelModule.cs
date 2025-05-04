using Lumina.Excel.Sheets;

namespace SomethingNeedDoing.MacroFeatures.LuaModules;
public class ExcelModule : LuaModuleBase
{
    public override string ModuleName => "Excel";

    [LuaFunction]
    public TerritoryType TerritoryType(uint rowId)
    {
        return GetRow<TerritoryType>(rowId) ?? throw new KeyNotFoundException();
    }
}
