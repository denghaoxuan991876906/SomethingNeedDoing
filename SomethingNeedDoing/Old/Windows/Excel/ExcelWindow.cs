using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Data.Files.Excel;
using Lumina.Data.Structs.Excel;
using Lumina.Excel;
using System.IO;

namespace SomethingNeedDoing.Old.Windows.Excel;

public class ExcelWindow : Window
{
    public static new readonly string WindowName = "SND Excel Browser";

    private readonly ExcelSheetList _sheetList = new();
    private readonly ExcelSheetDisplay _sheetDisplay = new();

    public ExcelWindow() : base(WindowName)
    {
        Size = new(960, 540);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        using (var c = ImRaii.Child($"{nameof(ExcelSheetList)}", new(267 * ImGui.GetIO().FontGlobalScale, 0), true))
        {
            if (c)
                _sheetList.Draw();
        }
        ImGui.SameLine();

        using var ch = ImRaii.Child($"{nameof(ExcelSheetDisplay)}");
        if (ch)
        {
            var header = Svc.Data.GetFile<ExcelHeaderFile>($"exd/{_sheetList._sheets[_sheetList.SelectedItem]}.exh")!;
            var sheetType = header.Header.Variant switch
            {
                ExcelVariant.Default => typeof(RawRow),
                ExcelVariant.Subrows => typeof(RawSubrow),
                _ => throw new InvalidDataException("Invalid variant"),
            };
            var sheet = Svc.Data.Excel.GetBaseSheet(sheetType, null, _sheetList._sheets[_sheetList.SelectedItem]);
            if (_sheetList.SelectedItem != 0)
                _sheetDisplay.Draw(sheet);
        }
    }
}
