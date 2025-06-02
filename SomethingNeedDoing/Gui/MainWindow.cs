using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;
using SomethingNeedDoing.Framework.Interfaces;
using SomethingNeedDoing.Gui.Modals;
using SomethingNeedDoing.Gui.Tabs;
using SomethingNeedDoing.Managers;

namespace SomethingNeedDoing.Gui;

public class MainWindow : Window
{
    private readonly RunningMacrosPanel _runningPanel;
    private readonly MacroEditor _macroEditor;
    private readonly HelpUI _helpUI;
    private readonly WindowSystem _ws;
    private readonly IMacroScheduler _scheduler;
    private readonly GitMacroManager _gitManager;
    private readonly MacroStatusWindow _statusWindow;

    // Macro selection
    private string _selectedMacroId = string.Empty;
    private string _searchText = string.Empty;

    // Folder management
    private string _selectedFolderId = "General";  // Default to General folder instead of Root
    private string _newFolderName = string.Empty;
    private bool _showCreateFolderPopup = false;

    // Storage for custom folders that might be empty
    private HashSet<string> _customFolders = [];

    // Track which folders are expanded
    private HashSet<string> _expandedFolders = [];

    // Panel resizing
    private float _leftPanelWidth = 250f;
    private float _minLeftPanelWidth = 180f;
    private float _maxLeftPanelWidth = 400f;

    // Default folder
    private const string DEFAULT_FOLDER = "General";

    // UI state
    private bool _isFolderSectionCollapsed = false;

    public MainWindow(WindowSystem ws, IMacroScheduler scheduler, GitMacroManager gitManager, RunningMacrosPanel runningPanel, MacroEditor macroEditor, HelpUI helpUI, MacroStatusWindow statusWindow)
        : base("Something Need Doing", ImGuiWindowFlags.NoScrollbar)
    {
        _ws = ws;
        _scheduler = scheduler;
        _gitManager = gitManager;
        _runningPanel = runningPanel;
        _macroEditor = macroEditor;
        _helpUI = helpUI;
        _statusWindow = statusWindow;

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
                DrawMacrosTab();

        using (var tab = ImRaii.TabItem("Help"))
            if (tab)
                _helpUI.Draw();

        using (var tab = ImRaii.TabItem("Settings"))
            if (tab)
                SettingsTab.DrawTab();
    }

    private void DrawMacrosTab()
    {
        // Get UI scale
        var scale = ImGuiHelpers.GlobalScale;

        // Calculate scaled widths
        var minWidth = _minLeftPanelWidth * scale;
        var maxWidth = _maxLeftPanelWidth * scale;
        var leftPanelWidth = _leftPanelWidth * scale;
        var windowPadding = ImGui.GetStyle().WindowPadding.X * 2;

        // Start resizable two-column layout
        ImGui.BeginTable("MainLayout", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable);

        // Setup columns
        ImGui.TableSetupColumn("Tree", ImGuiTableColumnFlags.WidthFixed, leftPanelWidth);
        ImGui.TableSetupColumn("Editor", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        // Search box
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##Search", "Search Folders & Macros...", ref _searchText, 100);

        ImGui.Separator();

        // Folders and macros panel
        ImGui.BeginChild("LeftPanel", new Vector2(0, -1), true);
        DrawFolderMacroTree();
        ImGui.EndChild();

        // Store user's panel resizing
        if (ImGui.TableGetColumnFlags(0).HasFlag(ImGuiTableColumnFlags.WidthFixed))
        {
            var currentWidth = ImGui.GetColumnWidth(0) / scale;
            currentWidth = Math.Clamp(currentWidth, _minLeftPanelWidth, _maxLeftPanelWidth);

            if (Math.Abs(_leftPanelWidth - currentWidth) > 1f)
            {
                _leftPanelWidth = currentWidth;
            }
        }

        // Editor panel
        ImGui.TableNextColumn();
        ImGui.BeginChild("RightPanel", new Vector2(0, -1), false);

        var selectedMacro = GetSelectedMacro();
        if (selectedMacro != null)
        {
            _macroEditor.Draw(selectedMacro);
        }
        else
        {
            DrawEmptyState();
        }

        ImGui.EndChild();
        ImGui.EndTable();
    }

    private void DrawFolderMacroTree()
    {
        // Section for folder/macro tree with better visual hierarchy
        ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.3f, 0.3f, 0.4f, 0.7f));
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.4f, 0.4f, 0.5f, 0.8f));

        // If there's no search, display the hierarchical tree
        if (string.IsNullOrEmpty(_searchText))
        {
            // FOLDERS SECTION HEADER WITH COLLAPSE TOGGLE
            ImGui.BeginGroup();

            // Draw the FOLDERS text with violet color
            ImGui.TextColored(ImGuiColors.DalamudViolet, "FOLDERS");

            // Add buttons next to the FOLDERS text
            var textWidth = ImGui.CalcTextSize("FOLDERS").X;
            ImGui.SameLine(textWidth + 15);

            if (ImGuiUtils.IconButton(_isFolderSectionCollapsed ? FontAwesomeIcon.AngleDown : FontAwesomeIcon.AngleUp, _isFolderSectionCollapsed ? "Expand folder tree" : "Collapse folder tree"))
                _isFolderSectionCollapsed ^= true;

            // New Macro button - with consistent size
            ImGui.SameLine(0, 5);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 4)); // Consistent padding

            if (ImGuiUtils.IconButton(FontAwesomeIcon.FileAlt, "Create a new macro"))
                CreateMacroModal.Open();

            // New Folder button - with consistent size
            ImGui.SameLine(0, 5);

            if (ImGuiUtils.IconButton(FontAwesomeIcon.FolderPlus, "Create a new folder"))
                CreateFolderModal.Open();

            ImGui.PopStyleVar();

            ImGui.EndGroup();

            // Only show folder tree if not collapsed
            if (!_isFolderSectionCollapsed)
            {
                // Make a scrollable area for the folder tree to maximize available space
                ImGui.BeginChild("FolderTreeArea", new Vector2(-1, ImGui.GetContentRegionAvail().Y * 0.6f), false);

                // Root/All Macros node - make it clearer
                var isRootSelected = _selectedFolderId == "Root";
                var rootCount = C.Macros.Count; // Total macro count

                using (var iconFont = ImRaii.PushFont(UiBuilder.IconFont))
                {
                    ImGui.TextColored(ImGuiColors.DalamudYellow, FontAwesomeIcon.Search.ToIconString());
                }
                ImGui.SameLine();

                if (ImGui.TreeNodeEx($"Show All Macros ({rootCount})##root",
                    isRootSelected ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None))
                {
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("View macros from all folders");

                    if (ImGui.IsItemClicked())
                    {
                        _selectedFolderId = "Root";
                        _selectedMacroId = string.Empty; // Clear macro selection
                    }

                    // Root is just a view, no sub-items
                    ImGui.TreePop();
                }

                // Get all real folders
                var allFolders = new HashSet<string>(C.GetFolderPaths());

                // Ensure our selected folder exists in the set
                if (!allFolders.Contains(_selectedFolderId) && _selectedFolderId != "Root")
                {
                    allFolders.Add(_selectedFolderId);
                }

                // Display all folders as tree nodes
                foreach (var folderPath in allFolders)
                {
                    if (folderPath != "Root" && !string.IsNullOrEmpty(folderPath))
                    {
                        var isSelected = _selectedFolderId == folderPath;
                        var folderCount = C.GetMacroCount(folderPath);

                        // Check if this folder should be expanded based on our tracking
                        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
                        if (isSelected) flags |= ImGuiTreeNodeFlags.Selected;

                        // Add DefaultOpen flag if this folder is in our tracked expanded folders set
                        if (_expandedFolders.Contains(folderPath))
                            flags |= ImGuiTreeNodeFlags.DefaultOpen;

                        ImGuiX.Icon(FontAwesomeHelper.IconFolder);
                        ImGui.SameLine();

                        var folderOpen = ImGui.TreeNodeEx($"{folderPath} ({folderCount})##folder_{folderPath}", flags);

                        // Update our expanded folders tracking
                        if (folderOpen && !_expandedFolders.Contains(folderPath))
                            _expandedFolders.Add(folderPath);
                        else if (!folderOpen && _expandedFolders.Contains(folderPath))
                            _expandedFolders.Remove(folderPath);

                        if (ImGui.IsItemClicked())
                        {
                            _selectedFolderId = folderPath;
                            _selectedMacroId = string.Empty; // Clear macro selection
                        }

                        // Context menu for folder operations - simplified
                        if (ImGui.BeginPopupContextItem())
                        {
                            // Show a more readable header
                            ImGui.TextColored(ImGuiColors.DalamudViolet, $"Folder: {folderPath}");
                            ImGui.Separator();

                            // Only show relevant operations
                            if (folderPath != DEFAULT_FOLDER) // Don't allow deleting the default folder
                            {
                                if (ImGui.MenuItem("Delete Folder"))
                                {
                                    DeleteFolder(folderPath);
                                    ImGui.CloseCurrentPopup();
                                }

                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.SetTooltip("Delete this folder and move all macros to Default folder");
                                }
                            }

                            ImGui.EndPopup();
                        }

                        // If folder is open, show macros within this folder
                        if (folderOpen)
                        {
                            // Add indent for macros in the folder
                            ImGui.Indent(10);

                            // List macros in this folder
                            foreach (var macro in C.GetMacrosInFolder(folderPath).ToList())
                                DrawMacroTreeNode(macro, false);

                            ImGui.Unindent(10);
                            ImGui.TreePop();
                        }
                    }
                }

                ImGui.EndChild(); // End FolderTreeArea
            }

            // Separator between folders and macro settings
            ImGui.Separator();

            DrawMacroSettings();
        }
        else
        {
            // SEARCH RESULTS
            ImGui.TextColored(ImGuiColors.DalamudViolet, "SEARCH RESULTS");

            // Show matching folders first
            var allFolders = new HashSet<string>(C.GetFolderPaths());
            var foundAnyFolders = false;

            // Show folders that match the search
            foreach (var folderPath in allFolders)
            {
                if ((folderPath == "Root" && "All Macros".Contains(_searchText, StringComparison.OrdinalIgnoreCase)) ||
                    (folderPath != "Root" && !string.IsNullOrEmpty(folderPath) &&
                     folderPath.Contains(_searchText, StringComparison.OrdinalIgnoreCase)))
                {
                    foundAnyFolders = true;

                    var displayName = folderPath == "Root" ? "All Macros" : folderPath;
                    var folderCount = folderPath == "Root" ? C.Macros.Count : C.GetMacroCount(folderPath);

                    // Display folder as selectable
                    var isSelected = _selectedFolderId == folderPath;
                    if (ImGui.Selectable($"📁 {displayName} ({folderCount})", isSelected))
                    {
                        _selectedFolderId = folderPath;
                        _selectedMacroId = string.Empty; // Clear macro selection
                    }
                }
            }

            // Show a separator between folders and macros if we found any folders
            if (foundAnyFolders)
            {
                ImGui.Separator();
                ImGui.TextColored(ImGuiColors.DalamudViolet, "MATCHING MACROS");
            }

            // Show matching macros
            var foundAnyMacros = false;

            foreach (var macro in C.SearchMacros(_searchText).ToList())
            {
                foundAnyMacros = true;
                DrawMacroTreeNode(macro, true);
            }

            if (!foundAnyFolders && !foundAnyMacros)
            {
                ImGui.TextColored(ImGuiColors.DalamudGrey, "No matching folders or macros");
            }
        }

        ImGui.PopStyleColor(2); // Pop the folder tree header colors
    }

    private void DrawMacroSettings()
    {
        using var _ = ImRaii.PushColor(ImGuiCol.Header, new Vector4(0.2f, 0.2f, 0.3f, 0.7f)).Push(ImGuiCol.HeaderHovered, new Vector4(0.25f, 0.25f, 0.35f, 0.8f));

        if (ImGui.CollapsingHeader("MACRO SETTINGS", ImGuiTreeNodeFlags.DefaultOpen))
        {
            using var child = ImRaii.Child("SettingsChild", new(-1, ImGui.GetContentRegionAvail().Y), false);

            var selectedMacro = GetSelectedMacro();
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

                    var events = selectedMacro.Metadata.TriggerEvents;
                    if (ImGuiUtils.EnumCheckboxes(ref events, [TriggerEvent.None]))
                    {
                        selectedMacro.Metadata.TriggerEvents = events;
                        C.Save();
                    }
                }
            }
            else
                ImGui.TextColored(ImGuiColors.DalamudGrey, "Select a macro to view and edit its settings");
        }
    }

    private void DrawMacroTreeNode(ConfigMacro macro, bool showFolder)
    {
        // Get icon based on macro type
        FontAwesomeIcon icon;
        if (macro.IsGitMacro)
            icon = FontAwesomeHelper.IconGitMacro;
        else
            icon = macro.Type == MacroType.Lua ? FontAwesomeHelper.IconLuaMacro : FontAwesomeHelper.IconNativeMacro;

        // Show icon
        ImGuiX.Icon(icon);
        ImGui.SameLine();

        // Build display name
        var displayName = showFolder ? $"{macro.Name} [{macro.FolderPath}]" : macro.Name;

        // Macro type indicator
        var typeIndicator = macro.Type == MacroType.Lua ? " (Lua)" : "";
        displayName += typeIndicator;

        // Set selection state
        var isSelected = macro.Id == _selectedMacroId;

        // Create selectable item
        if (isSelected)
        {
            ImGui.PushStyleColor(ImGuiCol.Header, ImGuiColors.ParsedPurple);
        }

        if (ImGui.Selectable(displayName, isSelected))
        {
            _selectedMacroId = macro.Id;

            // When selecting a macro from search or All Macros view, also switch to its folder
            if (showFolder)
            {
                _selectedFolderId = macro.FolderPath;
            }
        }

        if (isSelected)
        {
            ImGui.PopStyleColor();
        }

        // Move context menu handling to a separate method to avoid ImGui state issues
        HandleMacroContextMenu(macro);
    }

    // Separate method for handling context menu to improve ImGui stability
    private void HandleMacroContextMenu(ConfigMacro macro)
    {
        if (!ImGui.BeginPopupContextItem($"##ContextMenu_{macro.Id}"))
            return;

        // Show macro name in context menu
        ImGui.TextColored(ImGuiColors.DalamudViolet, macro.Name);
        ImGui.Separator();

        // Basic operations in main menu for clarity
        if (ImGuiX.IconMenuItem(FontAwesomeHelper.IconPlay, "Run"))
        {
            _scheduler.StartMacro(macro);
            ImGui.CloseCurrentPopup();
        }

        if (ImGuiX.IconMenuItem(FontAwesomeHelper.IconCopy, "Copy Content"))
        {
            ImGui.SetClipboardText(macro.Content);
            ImGui.CloseCurrentPopup();
        }

        if (ImGuiX.IconMenuItem(FontAwesomeHelper.IconRename, "Rename"))
            RenameModal.Open(macro);

        if (ImGuiX.IconMenuItem(FontAwesomeHelper.IconDelete, "Delete"))
        {
            // Store current folder ID to maintain selection
            var currentFolderId = _selectedFolderId;

            // Store the expanded folders state - we'll keep the same state
            var expandedFoldersCopy = new HashSet<string>(_expandedFolders);

            macro.Delete();

            // Clear selection if we just deleted the selected macro
            if (_selectedMacroId == macro.Id)
                _selectedMacroId = string.Empty;

            // Restore folder selection if possible
            if (!string.IsNullOrEmpty(currentFolderId))
                _selectedFolderId = currentFolderId;

            // Restore expanded folders state
            _expandedFolders = expandedFoldersCopy;

            ImGui.CloseCurrentPopup();
        }

        // Add Type selector to the context menu (only for ConfigMacro, not GitMacro)
        if (macro is ConfigMacro configMacro && !macro.IsGitMacro)
        {
            ImGui.Separator();

            if (ImGui.BeginMenu("Type"))
            {
                var isNative = macro.Type == MacroType.Native;
                var isLua = macro.Type == MacroType.Lua;

                if (ImGui.MenuItem("Native", null, isNative))
                {
                    configMacro.Type = MacroType.Native;
                    C.Save();
                }

                if (ImGui.MenuItem("Lua", null, isLua))
                {
                    configMacro.Type = MacroType.Lua;
                    C.Save();
                }

                ImGui.EndMenu();
            }
        }

        // Move to folder functionality - simplified and more stable
        ImGui.Separator();

        if (ImGui.BeginMenu("Move to folder"))
        {
            // Show folders in a more organized way
            ImGui.TextColored(ImGuiColors.DalamudViolet, "Select destination folder:");
            ImGui.Separator();

            // Option to move to the default folder
            var isInDefault = macro.FolderPath == DEFAULT_FOLDER;
            if (ImGui.MenuItem(DEFAULT_FOLDER, null, isInDefault))
            {
                if (!isInDefault)
                {
                    // Store the expanded folders state
                    var expandedFoldersCopy = new HashSet<string>(_expandedFolders);

                    MoveMacroToFolder(macro.Id, DEFAULT_FOLDER);

                    // Restore expanded folders state
                    _expandedFolders = expandedFoldersCopy;

                    // Ensure the destination folder is expanded
                    if (!_expandedFolders.Contains(DEFAULT_FOLDER))
                        _expandedFolders.Add(DEFAULT_FOLDER);
                }
                ImGui.CloseCurrentPopup();
            }

            // Get all folders for better organization
            var folders = new List<string>(C.GetFolderPaths());
            folders.Remove("Root"); // Don't include Root
            folders.Remove(DEFAULT_FOLDER); // Already listed above
            folders.Sort(); // Sort alphabetically

            if (folders.Any())
            {
                ImGui.Separator();

                foreach (var folder in folders)
                {
                    if (!string.IsNullOrEmpty(folder))
                    {
                        var isCurrentFolder = macro.FolderPath == folder;

                        // Show current folder with checkmark
                        if (ImGui.MenuItem($"{folder}{(isCurrentFolder ? " (current)" : "")}", null, isCurrentFolder))
                        {
                            if (!isCurrentFolder)
                            {
                                var expandedFoldersCopy = new HashSet<string>(_expandedFolders);
                                MoveMacroToFolder(macro.Id, folder);

                                _expandedFolders = expandedFoldersCopy;
                                _expandedFolders.Add(folder);
                            }
                            ImGui.CloseCurrentPopup();
                        }
                    }
                }
            }

            // Option to create a new folder
            ImGui.Separator();

            if (ImGui.MenuItem("Create new folder..."))
            {
                _newFolderName = "New Folder";
                _showCreateFolderPopup = true;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndMenu();
        }

        ImGui.EndPopup();
    }

    private void DrawEmptyState()
    {
        var center = ImGui.GetContentRegionAvail() / 2;
        var text = "Select a macro or create a new one";
        var textSize = ImGui.CalcTextSize(text);

        ImGui.SetCursorPos(ImGui.GetCursorPos() + center - textSize / 2);
        ImGui.TextColored(ImGuiColors.DalamudGrey, text);
    }

    private IMacro? GetSelectedMacro()
    {
        foreach (var macro in C.Macros)
        {
            if (macro.Id == _selectedMacroId)
                return macro;
        }
        return null;
    }

    // Improved MoveMacroToFolder method for better feedback
    private void MoveMacroToFolder(string macroId, string folderPath)
    {
        // Never allow moving to Root directly
        if (folderPath == "Root")
        {
            folderPath = DEFAULT_FOLDER;
        }

        if (C.GetMacro(macroId) is ConfigMacro configMacro)
        {
            // Don't do anything if it's already in this folder
            if (configMacro.FolderPath == folderPath)
                return;

            // Remember the old folder for notification
            var oldFolder = configMacro.FolderPath;

            // Save expanded folders state
            var expandedFoldersCopy = new HashSet<string>(_expandedFolders);

            // Update the folder path
            configMacro.FolderPath = folderPath;
            C.Save();

            // Update selection to reflect the move
            _selectedFolderId = folderPath;
            _selectedMacroId = macroId;

            // Restore expanded folders state
            _expandedFolders = expandedFoldersCopy;

            // Ensure the destination folder is expanded
            if (!_expandedFolders.Contains(folderPath))
                _expandedFolders.Add(folderPath);

            // Notify the user
            Svc.Chat.Print($"Moved macro '{configMacro.Name}' from '{oldFolder}' to '{folderPath}'");
        }
        else
        {
            Svc.Chat.PrintError($"Could not move macro: Not found or wrong type");
        }
    }

    private void DeleteFolder(string folderPath)
    {
        // Don't delete the default folder
        if (folderPath == DEFAULT_FOLDER)
        {
            Svc.Chat.PrintError("Cannot delete the Default folder");
            return;
        }

        // Save expanded folders state
        var expandedFoldersCopy = new HashSet<string>(_expandedFolders);

        // Get all macros in this folder
        var macrosInFolder = C.GetMacrosInFolder(folderPath).ToList();
        var macroCount = macrosInFolder.Count;

        if (macroCount > 0)
        {
            // Move all macros in this folder to the default folder
            foreach (var macro in macrosInFolder)
            {
                if (macro is ConfigMacro configMacro)
                {
                    configMacro.FolderPath = DEFAULT_FOLDER;
                }
            }
        }

        C.Save();

        // If the deleted folder was selected, switch to default
        if (_selectedFolderId == folderPath)
        {
            _selectedFolderId = DEFAULT_FOLDER;
        }

        // Restore expanded folders state
        _expandedFolders = expandedFoldersCopy;

        // Remove the deleted folder from expanded folders
        _expandedFolders.Remove(folderPath);

        // Ensure the default folder is expanded since macros moved there
        if (macroCount > 0 && !_expandedFolders.Contains(DEFAULT_FOLDER))
            _expandedFolders.Add(DEFAULT_FOLDER);

        // Notify the user
        if (macroCount > 0)
        {
            Svc.Chat.Print($"Deleted folder '{folderPath}' and moved {macroCount} macro(s) to Default folder");
        }
        else
        {
            Svc.Chat.Print($"Deleted empty folder '{folderPath}'");
        }
    }
}
