using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;

namespace SomethingNeedDoing.Gui;
public class MacroUI : Window
{
    private string selectedMacroId = string.Empty;
    private string selectedFolderPath = "/";
    private string searchText = string.Empty;
    private bool isEditing = false;
    private string editingContent = string.Empty;
    private MigrationPreviewWindow? _wnd;
    private readonly HashSet<string> collapsedFolders = [];

    public MacroUI() : base("Macro Manager", ImGuiWindowFlags.NoScrollbar)
    {
        Size = new Vector2(800, 600);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        if (ImGui.Button("Import Old Config"))
        {
            var clipboard = ImGui.GetClipboardText();
            if (!string.IsNullOrEmpty(clipboard))
            {
                _wnd = new MigrationPreviewWindow(clipboard)
                {
                    IsOpen = true
                };
                P._ws.AddWindow(_wnd);
            }
            else
            {
                Svc.Chat.PrintError("No configuration data in clipboard");
            }
        }

        // Draw the running macros panel at the top
        RunningMacrosPanel.Draw();

        // Split window into sidebar and main content
        var sidebarWidth = 250f;
        var mainWidth = ImGui.GetWindowWidth() - sidebarWidth;

        using (var sidebar = ImRaii.Child("Sidebar", new(sidebarWidth, -1)))
            DrawSidebar();

        ImGui.SameLine();

        using var mainContent = ImRaii.Child("MainContent", new(mainWidth, -1));
        DrawMainContent();
    }

    private void DrawSidebar()
    {
        // Search bar
        if (ImGui.InputText("Search", ref searchText, 100))
        {
            // Filter macros based on search
        }

        // Folder tree with drag and drop
        using var _ = ImRaii.Child("FolderTree", new(-1, -1));
        DrawFolderTree();
    }

    private void DrawFolderTree()
    {
        var folders = C.GetFolderTreeWithCounts().ToList();

        foreach (var ((path, depth, count), idx) in folders.WithIndex())
        {
            // Skip the root folder itself
            if (path == "/")
                continue;

            var folderName = path.Split('/').Last();
            var indent = new string(' ', depth * 2);
            var isCollapsed = collapsedFolders.Contains(path);
            var hasChildren = C.GetMacrosInFolderRecursive(path).Any() || C.GetSubfolders(path).Any();

            // Draw folder with collapse arrow
            if (hasChildren)
            {
                using var col = ImRaii.PushColor(ImGuiCol.Text, EzColor.OrangeBright.U32);
                using var tree = ImRaii.TreeNode($"{indent} {folderName} ({count})", ImGuiTreeNodeFlags.DefaultOpen);
                col.Pop();
                if (tree)
                {
                    ImGuiUtils.ContextMenu($"{folderName}_{idx}", ("Delete Folder", () => DeleteFolder(path)));

                    // Show macros in this folder
                    var macros = C.GetMacrosInFolder(path);
                    foreach (var macro in macros)
                    {
                        var macroIndent = new string(' ', (depth + 1) * 2);

                        // Macro node
                        if (ImGui.Selectable($"{macroIndent} {macro.Name}", macro.Id == selectedMacroId))
                        {
                            selectedMacroId = macro.Id;
                            selectedFolderPath = path;
                        }

                        ImGuiUtils.ContextMenu($"{macro.Name}_{idx}", ("Delete", () => { macro.Delete(); if (selectedMacroId == macro.Id) selectedMacroId = string.Empty; }));
                    }
                }
                else
                    ImGuiUtils.ContextMenu($"{folderName}_{idx}", ("Delete Folder", () => DeleteFolder(path)));
            }
            else
            {
                // Folder without children
                if (ImGui.Selectable($"{indent} {folderName} ({count})", path == selectedFolderPath))
                {
                    selectedFolderPath = path;
                    selectedMacroId = string.Empty;
                }

                ImGuiUtils.ContextMenu($"{folderName}_{idx}", ("Delete Folder", () => DeleteFolder(path)));
            }
        }

        // Show macros in the root folder
        var rootMacros = C.GetMacrosInFolder("/");
        foreach (var (macro, idx) in rootMacros.WithIndex())
        {
            // Macro node at root level
            if (ImGui.Selectable($"{macro.Name}", macro.Id == selectedMacroId))
            {
                selectedMacroId = macro.Id;
                selectedFolderPath = "/";
            }

            ImGuiUtils.ContextMenu($"{macro.Name}_{idx}", ("Delete", () => { macro.Delete(); if (selectedMacroId == macro.Id) selectedMacroId = string.Empty; }));
        }
    }

    private void DeleteFolder(string folderPath)
    {
        // Get all macros in this folder and subfolders
        var macrosToDelete = C.GetMacrosInFolderRecursive(folderPath).ToList();

        // Delete each macro
        macrosToDelete.ForEach(m => m.Delete());

        // Clear selection if the selected macro was in this folder
        if (selectedMacroId != string.Empty && macrosToDelete.Any(m => m.Id == selectedMacroId))
            selectedMacroId = string.Empty;

        // Update selected folder if it was the deleted one
        if (selectedFolderPath == folderPath || selectedFolderPath.StartsWith(folderPath + "/"))
            selectedFolderPath = "/";

        Svc.Chat.Print($"Deleted folder {folderPath} and {macrosToDelete.Count} macros");
    }

    //private void DrawMacroTypePopup()
    //{
    //    if (macroToChangeType == null) return;

    //    var macro = C.GetMacro(macroToChangeType);
    //    if (macro == null)
    //    {
    //        showMacroTypePopup = false;
    //        macroToChangeType = null;
    //        return;
    //    }

    //    ImGui.OpenPopup("Change Macro Type");

    //    if (ImGui.BeginPopupModal("Change Macro Type", ref showMacroTypePopup, ImGuiWindowFlags.AlwaysAutoResize))
    //    {
    //        ImGui.Text($"Change type for macro: {macro.Name}");
    //        ImGui.Separator();

    //        var currentType = macro.Type;
    //        var newType = currentType;

    //        if (ImGui.RadioButton("Native", newType == MacroType.Native))
    //            newType = MacroType.Native;

    //        ImGui.SameLine();

    //        if (ImGui.RadioButton("Lua", newType == MacroType.Lua))
    //            newType = MacroType.Lua;

    //        ImGui.Separator();

    //        if (ImGui.Button("Apply"))
    //        {
    //            macro.Type = newType;
    //            showMacroTypePopup = false;
    //            macroToChangeType = null;
    //        }

    //        ImGui.SameLine();

    //        if (ImGui.Button("Cancel"))
    //        {
    //            showMacroTypePopup = false;
    //            macroToChangeType = null;
    //        }

    //        ImGui.EndPopup();
    //    }
    //}

    private void DrawMainContent()
    {
        if (string.IsNullOrEmpty(selectedMacroId))
        {
            DrawEmptyState();
            return;
        }

        var macro = C.GetMacro(selectedMacroId);
        if (macro == null) return;

        // Top bar with macro info and controls
        DrawMacroHeader(macro);
        // Bottom bar with execution controls
        DrawMacroControls(macro);
        // Macro content editor
        DrawMacroContent(macro);
    }

    private void DrawMacroHeader(ConfigMacro macro)
    {
        using var _ = ImRaii.Child("MacroHeader", new(-1, 60));

        // Macro name and type
        ImGui.Text($"Name: {macro.Name}");
        ImGui.SameLine();
        ImGui.Text($"Type: {macro.Type}");

        var lang = macro.Type;
        if (ImGuiEx.EnumCombo("Language", ref lang))
        {
            macro.Type = lang;
            C.Save();
        }
        ImGui.SameLine();

        // Action buttons
        if (ImGui.Button("Rename"))
        {
            // Show rename dialog
        }
        ImGui.SameLine();
        if (ImGui.Button("Duplicate"))
            macro.Duplicate();
        ImGui.SameLine();
        if (ImGui.Button("Delete"))
        {
            macro.Delete();
            selectedMacroId = string.Empty;
        }
    }

    private void DrawMacroControls(ConfigMacro macro)
    {
        using var controls = ImRaii.Child("MacroControls", new Vector2(-1, 40));

        if (ImGui.Button("Edit"))
        {
            isEditing = !isEditing;
            if (isEditing)
            {
                editingContent = macro.Content;
            }
        }
        ImGui.SameLine();

        if (ImGui.Button("Copy"))
            ImGui.SetClipboardText(macro.Content);
        ImGui.SameLine();
        if (ImGui.Button("Run"))
            macro.Start();
        ImGui.SameLine();

        if (ImGui.Button("Stop"))
            macro.Stop();
    }

    private void DrawMacroContent(ConfigMacro macro)
    {
        using var _ = ImRaii.Child("MacroContent", new(-1, -1));
        var contents = macro.Content;
        if (ImGui.InputTextMultiline($"##{macro.Name}-editor", ref contents, 1_000_000, new Vector2(-1, -1)))
        {
            macro.Content = contents;
            C.Save();
        }
    }

    private void DrawEmptyState()
    {
        var center = ImGui.GetWindowSize() / 2;
        ImGui.SetCursorPos(center - new Vector2(100, 20));
        ImGui.Text("Select a macro or create a new one");
    }
}
