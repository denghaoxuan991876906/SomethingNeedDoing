using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Old.Windows.Excel;
public sealed class ExcelSheetList
{
    public event Action<int>? SelectedItemChanged;
    public int SelectedItem
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SelectedItemChanged?.Invoke(value);
            }
        }
    }

    internal readonly Dictionary<int, string> _sheets;
    private readonly List<int> _allSheets = []; // sorted by name
    private readonly List<int> _recents = [];
    private readonly List<int> _favourites = [];

    private List<int> _curSourceList;
    private readonly List<int> _curFilteredSheets = [];
    private string _curSearchFilter = "";

    public ExcelSheetList()
    {
        _sheets = Svc.Data.Excel.SheetNames.Select((item, index) => new { Index = index, Value = item }).ToDictionary(x => x.Index, x => x.Value);
        _allSheets = [.. _sheets.Keys.OrderBy(id => _sheets[id])];
        _curSourceList = _allSheets;
        Task.Run(ApplyFilterAsync);
    }

    public void Draw()
    {
        var spaceForButton = ImGui.GetStyle().ItemSpacing.X + 32 * ImGui.GetIO().FontGlobalScale;
        ImGui.SetNextItemWidth(-2 * spaceForButton);
        var filterDirty = ImGui.InputTextWithHint("###filter", "Search...", ref _curSearchFilter, 256);
        ImGui.SameLine();
        filterDirty |= DrawSourceSelectorButton(FontAwesomeIcon.History, _recents);
        ImGui.SameLine();
        filterDirty |= DrawSourceSelectorButton(FontAwesomeIcon.Star, _favourites);

        if (filterDirty)
            Task.Run(ApplyFilterAsync);

        ImGui.Separator();
        DrawFilteredSheets();
    }

    private void DrawFilteredSheets()
    {
        using var list = ImRaii.Child("list", default, false, ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.AlwaysHorizontalScrollbar);
        if (!list) return;

        Func<Task>? postIteration = null;
        using var style = ImRaii.PushIndent(0.5f);
        foreach (var id in _curFilteredSheets.ToArray())
        {
            if (ImGui.Selectable(_sheets[id], SelectedItem == id))
            {
                SelectedItem = SelectedItem != id ? id : default;
                if (_curSourceList != _recents)
                {
                    _recents.Remove(id);
                    _recents.Insert(0, id);
                }
            }

            ImGui.OpenPopupOnItemClick($"itemContextMenu{id}", ImGuiPopupFlags.MouseButtonRight);
            using var ctx = ImRaii.ContextPopupItem($"itemContextMenu{id}");
            if (ctx)
            {
                if (_favourites.Contains(id))
                {
                    if (ImGui.MenuItem("Remove from favourites"))
                    {
                        _favourites.Remove(id);
                        if (_curSourceList == _favourites)
                            postIteration += ApplyFilterAsync;
                    }
                }
                else
                {
                    if (ImGui.MenuItem("Add to favourites"))
                    {
                        _favourites.Add(id);
                        if (_curSourceList == _favourites)
                            postIteration += ApplyFilterAsync;
                    }
                }
            }
        }
        Task.Run(() => postIteration?.Invoke());
    }

    private bool DrawSourceSelectorButton(FontAwesomeIcon icon, List<int> source)
    {
        var active = _curSourceList == source;
        using var font = ImRaii.PushFont(UiBuilder.IconFont);
        using var c1 = ImRaii.PushColor(ImGuiCol.Button, 0xFF5CB85C, active);
        using var c2 = ImRaii.PushColor(ImGuiCol.ButtonHovered, 0x885CB85C, active);
        if (!ImGui.Button(icon.ToIconString(), new(32 * ImGui.GetIO().FontGlobalScale, ImGui.GetItemRectSize().Y)))
            return false;
        _curSourceList = active ? _allSheets : source;
        return true;
    }

    private async Task ApplyFilterAsync()
    {
        _curFilteredSheets.Clear();
        var terms = _curSearchFilter.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        async Task<bool> match(string name) => await Task.FromResult(terms.All(name.Contains));
        _curFilteredSheets.AddRange(await FilterAsync(_curSourceList, (sheet) => match(_sheets[sheet].ToLowerInvariant())));
    }

    private async Task<T[]> FilterAsync<T>(IEnumerable<T> sourceEnumerable, Func<T, Task<bool>> predicateAsync)
        => (await Task.WhenAll(
            sourceEnumerable.Select(v => predicateAsync(v).ContinueWith(
                task => new { Predicate = task.Result, Value = v }))))
        .Where(a => a.Predicate).Select(a => a.Value).ToArray();
}
