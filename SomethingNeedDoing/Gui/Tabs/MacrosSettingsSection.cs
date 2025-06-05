using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.Gui.Tabs;

public class MacrosSettingsSection(IMacroScheduler scheduler)
{
    public void Draw(ConfigMacro? selectedMacro)
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Header, new Vector4(0.2f, 0.2f, 0.3f, 0.7f))
            .Push(ImGuiCol.HeaderHovered, new Vector4(0.25f, 0.25f, 0.35f, 0.8f));

        if (ImGui.CollapsingHeader("MACRO SETTINGS", ImGuiTreeNodeFlags.DefaultOpen))
        {
            using var child = ImRaii.Child("SettingsChild", new(-1, ImGui.GetContentRegionAvail().Y), false);
            if (!child) return;

            if (selectedMacro != null)
            {
                ImGui.Spacing();

                if (ImGui.CollapsingHeader("General Information", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Spacing();

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Author:");
                    ImGui.SameLine(100);

                    var author = selectedMacro.Metadata.Author ?? string.Empty;
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    if (ImGui.InputText("##Author", ref author, 100))
                    {
                        selectedMacro.Metadata.Author = author;
                        C.Save();
                    }

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Version:");
                    ImGui.SameLine(100);

                    var version = selectedMacro.Metadata.Version ?? string.Empty;
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    if (ImGui.InputText("##Version", ref version, 50))
                    {
                        selectedMacro.Metadata.Version = version;
                        C.Save();
                    }

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Description:");

                    var description = selectedMacro.Metadata.Description ?? string.Empty;
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    if (ImGui.InputTextMultiline("##Description", ref description, 1000, new Vector2(-1, 100)))
                    {
                        selectedMacro.Metadata.Description = description;
                        C.Save();
                    }
                }

                ImGui.Spacing();

                if (selectedMacro.Type is MacroType.Native)
                {
                    if (ImGui.CollapsingHeader("Crafting Settings", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.Spacing();

                        var craftingLoop = selectedMacro.Metadata.CraftingLoop;
                        if (ImGui.Checkbox("Enable Crafting Loop", ref craftingLoop))
                        {
                            selectedMacro.Metadata.CraftingLoop = craftingLoop;
                            C.Save();
                        }

                        if (craftingLoop)
                        {
                            ImGui.Indent(20);

                            var loopCount = selectedMacro.Metadata.CraftLoopCount;
                            ImGui.SetNextItemWidth(100);
                            if (ImGui.InputInt("Loop Count", ref loopCount))
                            {
                                if (loopCount < -1)
                                    loopCount = -1;

                                selectedMacro.Metadata.CraftLoopCount = loopCount;
                                C.Save();
                            }

                            ImGui.SameLine();
                            ImGui.TextColored(ImGuiColors.DalamudGrey, "(-1 = infinite)");

                            ImGui.Unindent(20);
                        }
                    }
                }

                ImGui.Spacing();

                if (ImGui.CollapsingHeader("Trigger Events", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Spacing();

                    var events = new List<TriggerEvent>(selectedMacro.Metadata.TriggerEvents);
                    if (ImGuiUtils.EnumCheckboxes(ref events, [TriggerEvent.None]))
                        selectedMacro.SetTriggerEvents(scheduler, events);
                }
            }
            else
                ImGui.TextColored(ImGuiColors.DalamudGrey, "Select a macro to view and edit its settings");
        }
    }
}
