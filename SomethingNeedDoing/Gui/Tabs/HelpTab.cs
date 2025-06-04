using ECommons.ImGuiMethods;

namespace SomethingNeedDoing.Gui.Tabs;
public class HelpTab(HelpLuaTab _luaTab)
{
    public void Draw()
    {
        ImGui.Dummy(new Vector2(0, 5)); // padding

        ImGuiEx.EzTabBar("Tabs",
            ("General", HelpGeneralTab.DrawTab, null, false),
            ("Lua", _luaTab.DrawTab, null, false),
            ("Cli", HelpCliTab.DrawTab, null, false),
            ("Clicks", HelpClicksTab.DrawTab, null, false),
            ("Keys & Sends", HelpKeysTab.DrawTab, null, false),
            ("Conditions", HelpConditionsTab.DrawTab, null, false));
    }
}
