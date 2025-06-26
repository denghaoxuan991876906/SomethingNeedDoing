using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;

namespace SomethingNeedDoing.Gui.Modals;
public static class CreateMacroModal
{
    private static Vector2 Size = new(400, 200);
    private static bool IsOpen = false;

    private static string _newMacroName = "New Macro";
    private static MacroType _newMacroType = MacroType.Native;

    public static void Open()
    {
        IsOpen = true;
        _newMacroName = "New Macro";
        _newMacroType = MacroType.Native;
    }

    public static void Close()
    {
        IsOpen = false;
        ImGui.CloseCurrentPopup();
    }

    public static void DrawModal()
    {
        if (!IsOpen) return;

        ImGui.OpenPopup($"CreateMacroPopup##{nameof(CreateMacroModal)}");

        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(Size);

        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
        using var popup = ImRaii.PopupModal($"CreateMacroPopup##{nameof(CreateMacroModal)}", ref IsOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoTitleBar);
        if (!popup) return;

        ImGuiEx.Icon(FontAwesomeHelper.IconNew);
        ImGui.SameLine();
        ImGui.Text("Create New Macro");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Name:");
        ImGui.SameLine();
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputText("##MacroName", ref _newMacroName, 100);
        ImGui.PopItemWidth();

        ImGui.Spacing();

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Type:");
        ImGui.SameLine();

        _newMacroType = ImGuiUtils.EnumRadioButtons(_newMacroType);

        ImGui.Spacing();
        ImGui.Spacing();

        ImGuiUtils.CenteredButtons(("Create", () =>
        {
            var uniqueName = C.GetUniqueMacroName(_newMacroName);
            var newMacro = new ConfigMacro
            {
                Name = uniqueName,
                Type = _newMacroType == 0 ? MacroType.Native : MacroType.Lua,
                Content = string.Empty,
                FolderPath = ConfigMacro.Root
            };

            C.Macros.Add(newMacro);
            C.Save();
            Close();
        }
        ), ("Cancel", Close));
    }
}
