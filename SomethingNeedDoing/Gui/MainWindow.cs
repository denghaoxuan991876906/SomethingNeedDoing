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

    public MainWindow(IMacroScheduler scheduler, MacroEditor macroEditor, MacrosSettingsSection macroSettings, HelpTab helpTab) : base("Something Need Doing", ImGuiWindowFlags.NoScrollbar)
    {
        _helpTab = helpTab;
        _macrosTab = new MacrosTab(scheduler, macroSettings, macroEditor);

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
