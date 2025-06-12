using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using SomethingNeedDoing.Core.Interfaces;
using SomethingNeedDoing.Gui.Modals;
using SomethingNeedDoing.Gui.Tabs;

namespace SomethingNeedDoing.Gui;

public class MainWindow : Window
{
    private readonly HelpTab _helpTab;
    private readonly MacrosTab _macrosTab;
    private readonly VersionHistoryModal _versionHistoryModal;

    public MainWindow(IMacroScheduler scheduler, MacroEditor macroEditor, MacroSettingsSection macroSettings, HelpTab helpTab, VersionHistoryModal versionHistoryModal) : base($"{nameof(MainWindow)}###{P.Name}", ImGuiWindowFlags.NoScrollbar)
    {
        _helpTab = helpTab;
        _macrosTab = new MacrosTab(scheduler, macroSettings, macroEditor);
        _versionHistoryModal = versionHistoryModal;

        Size = new Vector2(1000, 600);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void PreDraw() => ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));

    public override void PostDraw() => ImGui.PopStyleVar();

    public override void Draw()
    {
        CreateMacroModal.DrawModal();
        CreateFolderModal.DrawModal();
        RenameModal.DrawModal();
        MigrationModal.DrawModal();
        _versionHistoryModal.Draw();

        using var _ = ImRaii.TabBar("Tabs");
        using (var tab = ImRaii.TabItem("MacrosLibrary"))
            if (tab)
                _macrosTab.Draw();

        using (var tab = ImRaii.TabItem("Help"))
            if (tab)
                _helpTab.Draw();

        using (var tab = ImRaii.TabItem("Settings"))
            if (tab)
                SettingsTab.DrawTab();
    }
}
