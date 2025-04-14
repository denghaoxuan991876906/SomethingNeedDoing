using System.Collections;

namespace SomethingNeedDoing.Old.Misc;

public unsafe class ListClipper : IEnumerable<(int, int)>, IDisposable
{
    private readonly ImGuiListClipperPtr Clipper;
    private readonly int CurrentRows;
    private readonly int CurrentColumns;
    private readonly bool TwoDimensional;
    private readonly int ItemRemainder;

    public int FirstRow { get; private set; } = -1;
    public int CurrentRow { get; private set; }
    public int DisplayEnd => Clipper.DisplayEnd;

    public IEnumerable<int> Rows
    {
        get
        {
            while (Clipper.Step()) // Supposedly this calls End()
            {
                if (Clipper.ItemsHeight > 0 && FirstRow < 0)
                    FirstRow = (int)(ImGui.GetScrollY() / Clipper.ItemsHeight);
                for (var i = Clipper.DisplayStart; i < Clipper.DisplayEnd; i++)
                {
                    CurrentRow = i;
                    yield return TwoDimensional ? i : i * CurrentColumns;
                }
            }
        }
    }

    public IEnumerable<int> Columns
    {
        get
        {
            var cols = ItemRemainder == 0 || CurrentRows != DisplayEnd || CurrentRow != DisplayEnd - 1 ? CurrentColumns : ItemRemainder;
            for (var j = 0; j < cols; j++)
                yield return j;
        }
    }

    public ListClipper(int items, int cols = 1, bool twoD = false, float itemHeight = 0)
    {
        TwoDimensional = twoD;
        CurrentColumns = cols;
        CurrentRows = TwoDimensional ? items : (int)MathF.Ceiling((float)items / CurrentColumns);
        ItemRemainder = !TwoDimensional ? items % CurrentColumns : 0;
        Clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
        Clipper.Begin(CurrentRows, itemHeight);
    }

    public IEnumerator<(int, int)> GetEnumerator() => (from i in Rows from j in Columns select (i, j)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        Clipper.Destroy(); // This also calls End() but I'm calling it anyway just in case
        GC.SuppressFinalize(this);
    }
}
