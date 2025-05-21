using Dalamud.Interface.Colors;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Utility;
using ImGuiNET;
using System;
using System.Numerics;
using System.Threading.Tasks;
using SomethingNeedDoing.Scheduler;
using System.Linq;
using Dalamud.Interface.Components;
using SomethingNeedDoing.Core.Github;
using SomethingNeedDoing.Framework.Interfaces;
using SomethingNeedDoing.Managers;
using System.Collections.Generic;
using SomethingNeedDoing.Gui;

namespace SomethingNeedDoing.Gui;

public class MainWindow : Window
{
    private readonly RunningMacrosPanel _runningPanel;
    private readonly MacroEditor _macroEditor;
    private readonly HelpUI _helpUI;
    private readonly WindowSystem _ws;
    private readonly IMacroScheduler _scheduler;
    private readonly GitMacroManager _gitManager;
    private readonly MacroStatusIndicator _statusIndicator;

    // Macro selection state
    private string _selectedMacroId = string.Empty;
    private string _searchText = string.Empty;

    // Add these fields to the MainWindow class
    private string _renameMacroBuffer = string.Empty;
    private bool _showRenamePopup = false;
    private string _macroToRename = string.Empty;
    
    // Fields for the new macro creation popup
    private string _newMacroName = "New Macro";
    private bool _showCreateMacroPopup = false;
    private int _newMacroType = 0; // 0 = Native, 1 = Lua

    // Add these fields for folder management
    private string _selectedFolderId = "General";  // Default to General folder instead of Root
    private string _newFolderName = string.Empty;
    private bool _showCreateFolderPopup = false;
    private bool _isDraggingMacro = false;
    private string _draggedMacroId = string.Empty;

    // Add this field to store custom folders that might be empty
    private HashSet<string> _customFolders = new HashSet<string>();
    
    // Default folder name constant
    private const string DEFAULT_FOLDER = "General";

    public MainWindow(WindowSystem ws, IMacroScheduler scheduler, GitMacroManager gitManager, RunningMacrosPanel runningPanel, MacroEditor macroEditor, HelpUI helpUI)
        : base("Something Need Doing", ImGuiWindowFlags.NoScrollbar)
    {
        _ws = ws;
        _scheduler = scheduler;
        _gitManager = gitManager;
        _runningPanel = runningPanel;
        _macroEditor = macroEditor;
        _helpUI = helpUI;
        _statusIndicator = new MacroStatusIndicator(scheduler);

        Size = new Vector2(1000, 600);
        SizeCondition = ImGuiCond.FirstUseEver;
        
        // Allow ImGui to save window position between plugin sessions
        RespectCloseHotkey = true;
        
        // Explicitly ensure we have a title bar with window controls
        Flags &= ~ImGuiWindowFlags.NoTitleBar;
    }

    public override void PreDraw()
    {
        // Make sure we are showing standard window controls - try different flags if needed
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));
    }
    
    public override void PostDraw()
    {
        ImGui.PopStyleVar();
    }

    public override void OnOpen()
    {
        base.OnOpen();
        // Ensure we have at least the default folder
        EnsureDefaultFolder();
    }

    private void EnsureDefaultFolder()
    {
        // Check if we have any folders other than Root
        var folders = C.GetFolderPaths();
        bool hasValidFolders = folders.Any(f => f != "Root" && !string.IsNullOrEmpty(f));
        
        // Create default folder if none exist
        if (!hasValidFolders)
        {
            _customFolders.Add(DEFAULT_FOLDER);
            
            // Create a template macro in the default folder
            var templateMacro = new ConfigMacro
            {
                Name = "Template Macro",
                Content = "// Add your macro commands here",
                Type = MacroType.Native,
                FolderPath = DEFAULT_FOLDER
            };
            C.Macros.Add(templateMacro);
            C.Save();
        }
        
        // Move any macros from Root to the default folder
        MoveRootMacrosToDefault();
    }
    
    private void MoveRootMacrosToDefault()
    {
        // Get all macros in Root
        var rootMacros = C.GetMacrosInFolder("Root").ToList();
        
        // Move each one to the default folder
        foreach (var macro in rootMacros)
        {
            if (macro is ConfigMacro configMacro)
            {
                configMacro.FolderPath = DEFAULT_FOLDER;
            }
        }
        
        if (rootMacros.Any())
        {
            C.Save();
        }
    }

    public override void Draw()
    {
        // Main tabs for the window
        ImGui.BeginTabBar("Tabs", ImGuiTabBarFlags.None);

        if (ImGui.BeginTabItem("Macros Library"))
        {
            DrawMacrosTab();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Running Macros"))
        {
            _runningPanel.Draw();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Help"))
        {
            _helpUI.Draw();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Settings"))
        {
            DrawSettingsTab();
            ImGui.EndTabItem();
        }
        
        // Status indicator placed at the right edge of the tab bar
        float indicatorWidth = 150f;
        float windowWidth = ImGui.GetWindowWidth();
        float tabBarHeight = ImGui.GetItemRectSize().Y; // Get the height of the tab bar
        
        // Position indicator at the right edge, aligned with tabs
        ImGui.SameLine(windowWidth - indicatorWidth - 10);
        _statusIndicator.Draw(indicatorWidth, tabBarHeight - 4); // Slightly smaller than tab height
        
        // Show rename popup if active
        ShowRenamePopup();
    }

    private void DrawSettingsTab()
    {
        ImGui.Text("General Settings");
        ImGui.Separator();
        
        // Add settings controls here
        var useCraftLoopTemplate = C.UseCraftLoopTemplate;
        if (ImGui.Checkbox("Enable automatic updates for Git macros", ref useCraftLoopTemplate))
        {
            C.UseCraftLoopTemplate = useCraftLoopTemplate;
            C.Save();
        }

        var craftSkip = C.CraftSkip;
        if (ImGui.Checkbox("Enable unsafe actions during crafting", ref craftSkip))
        {
            C.CraftSkip = craftSkip;
            C.Save();
        }
    }

    private void DrawMacrosTab()
    {
        // Split layout between folder/macro tree (left) and editor (right)
        float leftPanelWidth = 250 * ImGuiHelpers.GlobalScale;
        float windowPadding = ImGui.GetStyle().WindowPadding.X * 2;
        
        // Remove the status indicator from here since it's now in the tab bar
        
        // Remove the buttons from top toolbar
        
        // Separator after toolbar
        ImGui.Separator();
        
        // Two-column layout
        ImGui.BeginTable("MainLayout", 2, ImGuiTableFlags.BordersInnerV);
        
        // First column - Folders/Macros tree (left side)
        ImGui.TableSetupColumn("Tree", ImGuiTableColumnFlags.WidthFixed, leftPanelWidth);
        ImGui.TableSetupColumn("Editor", ImGuiTableColumnFlags.WidthStretch);
        
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        
        // Search box now only appears above the folder/macro tree
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##Search", "Search Folders & Macros...", ref _searchText, 100);
        
        ImGui.Separator();
        
        // Begin left panel for folders and macros - full height (no status bar at bottom)
        ImGui.BeginChild("LeftPanel", new Vector2(0, -1), true);
        
        // Create combined tree view for folders and macros
        DrawFolderMacroTree();
        
        ImGui.EndChild(); // End LeftPanel
        
        // Second column - Editor and Macro settings
        ImGui.TableNextColumn();
        
        // Right side for macro editor and settings - full height (no status bar at bottom)
        ImGui.BeginChild("RightPanel", new Vector2(0, -1), false);
        
        // Show selected macro
        var selectedMacro = GetSelectedMacro();
        if (selectedMacro != null)
        {
            // Remove the duplicate toolbar buttons
            _macroEditor.Draw(selectedMacro);
        }
        else
        {
            DrawEmptyState();
        }
        
        ImGui.EndChild(); // End RightPanel
        
        ImGui.EndTable(); // End MainLayout table
    }
    
    private void DrawFolderMacroTree()
    {
        // Section for folder/macro tree with better visual hierarchy
        ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.3f, 0.3f, 0.4f, 0.7f));
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.4f, 0.4f, 0.5f, 0.8f));
        
        // If there's no search, display the hierarchical tree
        if (string.IsNullOrEmpty(_searchText))
        {
            // FOLDERS SECTION - more compact header without New Folder button
            ImGui.BeginGroup();
            ImGui.TextColored(ImGuiColors.DalamudViolet, "FOLDERS");
            ImGui.EndGroup();
            
            // Make a scrollable area for the folder tree to maximize available space
            ImGui.BeginChild("FolderTreeArea", new Vector2(-1, ImGui.GetContentRegionAvail().Y * 0.6f), false);
            
            // Root/All Macros node
            bool isRootSelected = _selectedFolderId == "Root";
            int rootCount = C.Macros.Count; // Total macro count
            
            ImGuiX.Icon(FontAwesomeHelper.IconHome);
            ImGui.SameLine();
            
            if (ImGui.TreeNodeEx($"All Macros ({rootCount})##root", 
                isRootSelected ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None))
            {
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
                    bool isSelected = _selectedFolderId == folderPath;
                    int folderCount = C.GetMacroCount(folderPath);
                    
                    // Display folder as a tree node with macro count
                    ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
                    if (isSelected) flags |= ImGuiTreeNodeFlags.Selected;
                    
                    ImGuiX.Icon(FontAwesomeHelper.IconFolder);
                    ImGui.SameLine();
                    
                    bool folderOpen = ImGui.TreeNodeEx($"{folderPath} ({folderCount})##folder_{folderPath}", flags);
                    
                    if (ImGui.IsItemClicked())
                    {
                        _selectedFolderId = folderPath;
                        _selectedMacroId = string.Empty; // Clear macro selection
                    }
                    
                    // Handle drop target for folders
                    if (ImGui.BeginDragDropTarget())
                    {
                        if (_isDraggingMacro && !string.IsNullOrEmpty(_draggedMacroId))
                        {
                            ImGui.Text($"Drop to move to {folderPath}");
                            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                            {
                                MoveMacroToFolder(_draggedMacroId, folderPath);
                                _isDraggingMacro = false;
                                _draggedMacroId = string.Empty;
                            }
                        }
                        ImGui.EndDragDropTarget();
                    }
                    
                    // Context menu for folder operations
                    if (ImGui.BeginPopupContextItem())
                    {
                        if (ImGui.MenuItem("Delete Folder"))
                        {
                            DeleteFolder(folderPath);
                        }
                        ImGui.EndPopup();
                    }
                    
                    // If folder is open, show macros within this folder
                    if (folderOpen)
                    {
                        // Add indent for macros in the folder
                        ImGui.Indent(10);
                        
                        // List macros in this folder
                        var macrosInFolder = C.GetMacrosInFolder(folderPath);
                        foreach (var macro in macrosInFolder)
                        {
                            DrawMacroTreeNode(macro, false);
                        }
                        
                        ImGui.Unindent(10);
                        ImGui.TreePop();
                    }
                }
            }
            
            ImGui.EndChild(); // End FolderTreeArea
            
            // Place all buttons in a row above the MACRO SETTINGS header
            
            // New Macro button
            if (ImGuiX.IconButton(FontAwesomeHelper.IconNew, "New Macro"))
            {
                // Reset popup fields
                _newMacroName = "New Macro";
                _newMacroType = 0; // Default to Native
                _showCreateMacroPopup = true;
                ImGui.OpenPopup("Create New Macro##Popup");
            }
            
            // Draw the create macro popup here to ensure it opens properly
            ShowCreateMacroPopup();
            
            // Import button with icon
            ImGui.SameLine(0, 15);
            if (ImGuiX.IconButton(FontAwesomeHelper.IconImport, "Import"))
            {
                var clipboard = ImGui.GetClipboardText();
                if (!string.IsNullOrEmpty(clipboard))
                {
                    try
                    {
                        // Import macro logic - use existing methods in your codebase
                        var importedMacro = new ConfigMacro
                        {
                            Name = "Imported Macro",
                            Content = clipboard,
                            Type = MacroType.Native
                        };
                        
                        if (_selectedFolderId != "Root")
                        {
                            importedMacro.FolderPath = _selectedFolderId;
                        }
                        
                        C.Macros.Add(importedMacro);
                    }
                    catch (Exception e)
                    {
                        // Log error using your logging mechanism
                        Console.WriteLine($"Failed to import macro: {e.Message}");
                    }
                }
            }
            
            // New Folder button with icon
            ImGui.SameLine(0, 15);
            if (ImGuiX.IconButton(FontAwesomeHelper.IconFolder, "New Folder"))
            {
                // Reset and show folder creation popup
                _newFolderName = "New Folder";
                _showCreateFolderPopup = true;
                ImGui.OpenPopup("Create New Folder##Popup");
            }
            
            // Show create folder popup
            ShowCreateFolderPopup();
            
            // Separator between folders and macro settings
            ImGui.Separator();
            
            // MACRO SETTINGS section with no buttons
            ImGui.TextColored(ImGuiColors.DalamudViolet, "MACRO SETTINGS");
            
            // Use triangle for collapsible header like in the screenshot
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.2f, 0.2f, 0.3f, 0.7f));
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.25f, 0.25f, 0.35f, 0.8f));
            
            if (ImGui.CollapsingHeader("MACRO SETTINGS", ImGuiTreeNodeFlags.DefaultOpen))
            {
                // Create a scrollable area for settings content that fills remaining space
                ImGui.BeginChild("SettingsScrollArea", new Vector2(-1, -1), false);
                
                var selectedMacro = GetSelectedMacro();
                if (selectedMacro != null)
                {
                    // Show selected macro name and type with proper styling
                    string macroName = selectedMacro.Name;
                    string macroTypeStr = selectedMacro.Type == MacroType.Lua ? "Lua" : "Native";
                    
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudWhite);
                    ImGui.Text($"{macroName} ({macroTypeStr})");
                    ImGui.PopStyleColor();
                    
                    // Create tabs similar to the screenshot
                    ImGui.BeginTabBar("MacroSettingsTabs", ImGuiTabBarFlags.None);
                    
                    // General tab
                    if (ImGui.BeginTabItem("General"))
                    {
                        // Create a scrollable area for the tab content
                        ImGui.BeginChild("GeneralTabContent", new Vector2(-1, -1), false);
                        
                        ImGui.TextColored(ImGuiColors.DalamudViolet, "General Information");
                        ImGui.Separator();
                        ImGui.Spacing();
                        
                        // Author field
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("Author:");
                        ImGui.SameLine(100);
                        
                        string author = selectedMacro.Metadata.Author ?? string.Empty;
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        if (ImGui.InputText("##Author", ref author, 100))
                        {
                            selectedMacro.Metadata.Author = author;
                            C.Save();
                        }
                        
                        // Version field
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("Version:");
                        ImGui.SameLine(100);
                        
                        string version = selectedMacro.Metadata.Version ?? string.Empty;
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        if (ImGui.InputText("##Version", ref version, 50))
                        {
                            selectedMacro.Metadata.Version = version;
                            C.Save();
                        }
                        
                        // Description field
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text("Description:");
                        
                        string description = selectedMacro.Metadata.Description ?? string.Empty;
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        if (ImGui.InputTextMultiline("##Description", ref description, 1000, new Vector2(-1, 100)))
                        {
                            selectedMacro.Metadata.Description = description;
                            C.Save();
                        }
                        
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }
                    
                    // Crafting tab
                    if (ImGui.BeginTabItem("Crafting"))
                    {
                        ImGui.BeginChild("CraftingTabContent", new Vector2(-1, -1), false);
                        
                        ImGui.TextColored(ImGuiColors.DalamudViolet, "Crafting Settings");
                        ImGui.Separator();
                        ImGui.Spacing();
                        
                        // Crafting loop toggle
                        bool craftingLoop = selectedMacro.Metadata.CraftingLoop;
                        if (ImGui.Checkbox("Enable Crafting Loop", ref craftingLoop))
                        {
                            selectedMacro.Metadata.CraftingLoop = craftingLoop;
                            C.Save();
                        }
                        
                        if (craftingLoop)
                        {
                            ImGui.Indent(20);
                            
                            // Loop count setting
                            int loopCount = selectedMacro.Metadata.CraftLoopCount;
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
                        
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }
                    
                    // Triggers tab
                    if (ImGui.BeginTabItem("Triggers"))
                    {
                        ImGui.BeginChild("TriggersTabContent", new Vector2(-1, -1), false);
                        
                        ImGui.TextColored(ImGuiColors.DalamudViolet, "Trigger Events");
                        ImGui.Separator();
                        ImGui.Spacing();
                        
                        // Auto-Retainer checkbox
                        bool arPostProcess = selectedMacro.Metadata.TriggerEvents.Contains(TriggerEvent.OnAutoRetainerCharacterPostProcess);
                        if (ImGui.Checkbox("Run after Auto Retainer completes", ref arPostProcess))
                        {
                            if (arPostProcess)
                                selectedMacro.Metadata.TriggerEvents.Add(TriggerEvent.OnAutoRetainerCharacterPostProcess);
                            else
                                selectedMacro.Metadata.TriggerEvents.Remove(TriggerEvent.OnAutoRetainerCharacterPostProcess);
                            
                            C.Save();
                        }
                        
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }
                    
                    ImGui.EndTabBar();
                }
                else
                {
                    // No macro selected
                    ImGui.TextColored(ImGuiColors.DalamudGrey, "Select a macro to view and edit its settings");
                }
                
                ImGui.EndChild(); // End SettingsScrollArea
            }
            
            ImGui.PopStyleColor(2); // Pop the header colors for MACRO SETTINGS section
        }
        else
        {
            // SEARCH RESULTS
            ImGui.TextColored(ImGuiColors.DalamudViolet, "SEARCH RESULTS");
            
            // Show matching folders first
            var allFolders = new HashSet<string>(C.GetFolderPaths());
            bool foundAnyFolders = false;
            
            // Show folders that match the search
            foreach (var folderPath in allFolders)
            {
                if ((folderPath == "Root" && "All Macros".Contains(_searchText, StringComparison.OrdinalIgnoreCase)) ||
                    (folderPath != "Root" && !string.IsNullOrEmpty(folderPath) && 
                     folderPath.Contains(_searchText, StringComparison.OrdinalIgnoreCase)))
                {
                    foundAnyFolders = true;
                    
                    string displayName = folderPath == "Root" ? "All Macros" : folderPath;
                    int folderCount = folderPath == "Root" ? C.Macros.Count : C.GetMacroCount(folderPath);
                    
                    // Display folder as selectable
                    bool isSelected = _selectedFolderId == folderPath;
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
            var matchingMacros = C.SearchMacros(_searchText);
            bool foundAnyMacros = false;
            
            foreach (var macro in matchingMacros)
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
    
    private void DrawMacroTreeNode(ConfigMacro macro, bool showFolder)
    {
        // Get icon based on macro type
        FontAwesomeIcon icon;
        if (macro is GitMacro)
            icon = FontAwesomeHelper.IconGitMacro;
        else
            icon = macro.Type == MacroType.Lua ? FontAwesomeHelper.IconLuaMacro : FontAwesomeHelper.IconNativeMacro;
        
        // Show icon
        ImGuiX.Icon(icon);
        ImGui.SameLine();
        
        // Build display name
        string displayName = showFolder 
            ? $"{macro.Name} [{macro.FolderPath}]"
            : macro.Name;
            
        // Macro type indicator
        string typeIndicator = macro.Type == MacroType.Lua ? " (Lua)" : "";
        displayName += typeIndicator;
        
        // Set selection state
        bool isSelected = macro.Id == _selectedMacroId;
        
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
        
        // Handle drag source for macros
        if (ImGui.IsItemActive() && !_isDraggingMacro)
        {
            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                _isDraggingMacro = true;
                _draggedMacroId = macro.Id;
            }
        }
        
        // Context menu
        if (ImGui.BeginPopupContextItem())
        {
            if (ImGuiX.IconMenuItem(FontAwesomeHelper.IconPlay, "Run"))
            {
                _scheduler.StartMacro(macro);
            }

            if (ImGuiX.IconMenuItem(FontAwesomeHelper.IconCopy, "Copy Content"))
            {
                ImGui.SetClipboardText(macro.Content);
            }
            
            // Add Type selector to the context menu (only for ConfigMacro, not GitMacro)
            if (macro is ConfigMacro configMacro)
            {
                if (ImGui.BeginMenu(ImGuiX.GetIconString(FontAwesomeHelper.IconNativeMacro) + " Set Type"))
                {
                    bool isNative = macro.Type == MacroType.Native;
                    bool isLua = macro.Type == MacroType.Lua;
                    
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

            if (ImGui.BeginMenu(ImGuiX.GetIconString(FontAwesomeHelper.IconEdit) + " Actions"))
            {
                if (ImGuiX.IconMenuItem(FontAwesomeHelper.IconRename, "Rename"))
                {
                    _renameMacroBuffer = macro.Name;
                    _showRenamePopup = true;
                    _macroToRename = macro.Id;
                }

                if (ImGuiX.IconMenuItem(FontAwesomeHelper.IconDelete, "Delete"))
                {
                    macro.Delete();
                    C.Save();
                }

                // Add folder move options
                if (ImGui.BeginMenu(ImGuiX.GetIconString(FontAwesomeHelper.IconFolder) + " Move to folder"))
                {
                    // Option to move to the default folder
                    if (ImGui.MenuItem("Default"))
                    {
                        MoveMacroToFolder(macro.Id, DEFAULT_FOLDER);
                    }

                    // Show other available folders
                    var folders = new List<string>();
                    foreach (var m in C.Macros)
                    {
                        if (!string.IsNullOrEmpty(m.FolderPath) && 
                            !folders.Contains(m.FolderPath) && 
                            m.FolderPath != DEFAULT_FOLDER &&
                            m.FolderPath != "Root")
                        {
                            folders.Add(m.FolderPath);
                        }
                    }
                    
                    foreach (var folder in folders)
                    {
                        if (ImGui.MenuItem(folder))
                        {
                            MoveMacroToFolder(macro.Id, folder);
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
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

    // Implementation for the rename and duplicate buttons
    private void HandleRenameButton(IMacro macro)
    {
        _macroToRename = macro.Id;
        _renameMacroBuffer = macro.Name;
        _showRenamePopup = true;
        ImGui.OpenPopup("RenameMacroPopup");
    }

    private void ShowRenamePopup()
    {
        if (!_showRenamePopup) return;

        // Center the popup
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        // Use a fixed size popup with better styling
        ImGui.SetNextWindowSize(new Vector2(350, 130));
        
        // Use specific flags to make popup work properly
        bool isOpen = _showRenamePopup;
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
        
        if (ImGui.BeginPopupModal("RenameMacroPopup", ref isOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings))
        {
            ImGui.Text("Enter new name:");
            ImGui.SetNextItemWidth(-1);
            
            // Set keyboard focus to the input when opening the popup
            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetKeyboardFocusHere();
            }
            
            // Check for Enter key press - using IsKeyPressed instead of KeysDownDuration
            bool enterPressed = ImGui.IsKeyPressed(ImGuiKey.Enter) && ImGui.IsWindowFocused();
            
            // Input field with better styling
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.15f, 0.15f, 0.15f, 1.0f));
            ImGui.InputText("##RenameMacroInput", ref _renameMacroBuffer, 100);
            ImGui.PopStyleColor();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Confirm button with clear styling
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.5f, 0.3f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.6f, 0.4f, 1.0f));
            
            bool confirmed = ImGui.Button("Confirm", new Vector2(150, 0)) || enterPressed;
            
            ImGui.PopStyleColor(2);
            
            if (confirmed && !string.IsNullOrWhiteSpace(_renameMacroBuffer))
            {
                ApplyRename();
                ImGui.CloseCurrentPopup();
                _showRenamePopup = false;
            }

            ImGui.SameLine();
            
            // Cancel button
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.3f, 0.3f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.6f, 0.4f, 0.4f, 1.0f));
            
            if (ImGui.Button("Cancel", new Vector2(150, 0)) || (ImGui.IsKeyPressed(ImGuiKey.Escape) && ImGui.IsWindowFocused()))
            {
                ImGui.CloseCurrentPopup();
                _showRenamePopup = false;
            }
            
            ImGui.PopStyleColor(2);

            ImGui.EndPopup();
        }
        
        ImGui.PopStyleVar();
        
        // Update the flag based on the popup state
        _showRenamePopup = isOpen;
    }
    
    private void ApplyRename()
    {
        var macro = C.GetMacro(_macroToRename);
        if (macro != null)
        {
            // Convert ConfigMacro or GitMacro to their base type (IMacro) first
            // This allows us to set the name property which is declared in MacroBase
            if (macro is MacroBase macroBase)
            {
                macroBase.Name = _renameMacroBuffer;
                C.Save();
            }
        }
    }

    private void DuplicateMacro(IMacro macro)
    {
        if (macro is ConfigMacro configMacro)
        {
            var newMacro = new ConfigMacro
            {
                Name = $"{configMacro.Name} (Copy)",
                Content = configMacro.Content,
                Type = configMacro.Type,
                FolderPath = configMacro.FolderPath
            };
            
            // Copy metadata if it exists
            if (configMacro.Metadata != null)
            {
                newMacro.Metadata.Author = configMacro.Metadata.Author;
                newMacro.Metadata.Version = configMacro.Metadata.Version;
                newMacro.Metadata.Description = configMacro.Metadata.Description;
                newMacro.Metadata.CraftingLoop = configMacro.Metadata.CraftingLoop;
                newMacro.Metadata.CraftLoopCount = configMacro.Metadata.CraftLoopCount;
            }
            
            C.Macros.Add(newMacro);
            C.Save();
            _selectedMacroId = newMacro.Id;
        }
    }

    private void ShowCreateMacroPopup()
    {
        if (!_showCreateMacroPopup) return;

        // Center the popup
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        // Use a fixed size popup with better styling
        ImGui.SetNextWindowSize(new Vector2(400, 200));
        
        // Use specific flags to make popup work properly
        bool isOpen = _showCreateMacroPopup;
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
        
        if (ImGui.BeginPopupModal("Create New Macro##Popup", ref isOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings))
        {
            // Header with icon
            ImGuiX.Icon(FontAwesomeHelper.IconNew);
            ImGui.SameLine();
            ImGui.Text("Create New Macro");
            ImGui.Separator();
            ImGui.Spacing();
            
            // Macro name input
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Name:");
            ImGui.SameLine();
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.InputText("##MacroName", ref _newMacroName, 100);
            ImGui.PopItemWidth();
            
            ImGui.Spacing();
            
            // Macro type selection with better UI
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Type:");
            ImGui.SameLine();
            
            // Radio buttons for macro type
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(15, 0));
            
            bool isNative = _newMacroType == 0;
            if (ImGui.RadioButton("Native##MacroType", isNative))
            {
                _newMacroType = 0;
            }
            ImGui.SameLine();
            
            bool isLua = _newMacroType == 1;
            if (ImGui.RadioButton("Lua##MacroType", isLua))
            {
                _newMacroType = 1;
            }
            ImGui.PopStyleVar();
            
            ImGui.Spacing();
            ImGui.Spacing();
            
            // Buttons at the bottom
            float buttonWidth = 120;
            float windowWidth = ImGui.GetWindowWidth();
            float totalButtonsWidth = buttonWidth * 2 + 10;  // Two buttons + spacing
            float startPosX = (windowWidth - totalButtonsWidth) / 2;
            
            ImGui.SetCursorPosX(startPosX);
            
            // Create button
            if (ImGui.Button("Create", new Vector2(buttonWidth, 0)))
            {
                MacroType selectedType = _newMacroType == 0 ? MacroType.Native : MacroType.Lua;
                
                var newMacro = new ConfigMacro
                {
                    Name = _newMacroName,
                    Type = selectedType,
                    Content = string.Empty
                };
                
                if (_selectedFolderId != "Root")
                {
                    newMacro.FolderPath = _selectedFolderId;
                }
                
                C.Macros.Add(newMacro);
                _selectedMacroId = newMacro.Id;
                
                _showCreateMacroPopup = false;
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.SameLine();
            
            // Cancel button
            if (ImGui.Button("Cancel", new Vector2(buttonWidth, 0)))
            {
                _showCreateMacroPopup = false;
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.EndPopup();
        }
        
        ImGui.PopStyleVar();
        
        if (!isOpen)
        {
            _showCreateMacroPopup = false;
        }
    }
    
    private void ShowCreateFolderPopup()
    {
        if (!_showCreateFolderPopup) return;

        // Center the popup
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        // Use a fixed size popup with better styling
        ImGui.SetNextWindowSize(new Vector2(400, 170));
        
        // Use specific flags to make popup work properly
        bool isOpen = _showCreateFolderPopup;
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(15, 15));
        
        if (ImGui.BeginPopupModal("Create New Folder##Popup", ref isOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings))
        {
            // Header with icon
            ImGuiX.Icon(FontAwesomeHelper.IconFolder);
            ImGui.SameLine();
            ImGui.Text("Create New Folder");
            ImGui.Separator();
            ImGui.Spacing();
            
            // Folder name input
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Name:");
            ImGui.SameLine();
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.InputText("##FolderName", ref _newFolderName, 100);
            ImGui.PopItemWidth();
            
            ImGui.Spacing();
            ImGui.Spacing();
            
            // Buttons at the bottom
            float buttonWidth = 120;
            float windowWidth = ImGui.GetWindowWidth();
            float totalButtonsWidth = buttonWidth * 2 + 10;  // Two buttons + spacing
            float startPosX = (windowWidth - totalButtonsWidth) / 2;
            
            ImGui.SetCursorPosX(startPosX);
            
            // Create button
            if (ImGui.Button("Create", new Vector2(buttonWidth, 0)))
            {
                // Check if folder exists already
                bool folderExists = false;
                foreach (var macro in C.Macros)
                {
                    if (macro.FolderPath == _newFolderName)
                    {
                        folderExists = true;
                        break;
                    }
                }
                
                if (!folderExists && !string.IsNullOrWhiteSpace(_newFolderName))
                {
                    // Create a dummy macro in the folder to ensure it exists
                    var dummyMacro = new ConfigMacro
                    {
                        Name = $"{_newFolderName} Template",
                        Content = "// Add your macro commands here",
                        Type = MacroType.Native,
                        FolderPath = _newFolderName
                    };
                    
                    C.Macros.Add(dummyMacro);
                    _selectedFolderId = _newFolderName;
                    
                    _showCreateFolderPopup = false;
                    ImGui.CloseCurrentPopup();
                }
            }
            
            ImGui.SameLine();
            
            // Cancel button
            if (ImGui.Button("Cancel", new Vector2(buttonWidth, 0)))
            {
                _showCreateFolderPopup = false;
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.EndPopup();
        }
        
        ImGui.PopStyleVar();
        
        if (!isOpen)
        {
            _showCreateFolderPopup = false;
        }
    }

    private void MoveMacroToFolder(string macroId, string folderPath)
    {
        // Never allow moving to Root directly
        if (folderPath == "Root")
        {
            folderPath = DEFAULT_FOLDER;
        }
        
        var macro = C.GetMacro(macroId);
        if (macro != null && macro is ConfigMacro configMacro)
        {
            configMacro.FolderPath = folderPath;
            C.Save();
        }
    }
    
    private void DeleteFolder(string folderPath)
    {
        // Don't delete the default folder
        if (folderPath == DEFAULT_FOLDER)
        {
            return;
        }
        
        // Move all macros in this folder to the default folder
        foreach (var macro in C.GetMacrosInFolder(folderPath))
        {
            if (macro is ConfigMacro configMacro)
            {
                configMacro.FolderPath = DEFAULT_FOLDER;
            }
        }
        
        C.Save();
        
        // If the deleted folder was selected, switch to default
        if (_selectedFolderId == folderPath)
        {
            _selectedFolderId = DEFAULT_FOLDER;
        }
    }
}
