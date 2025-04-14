//using Dalamud.Interface;
//using Dalamud.Interface.Colors;
//using Dalamud.Interface.Utility;
//using Dalamud.Interface.Utility.Raii;
//using Dalamud.Interface.Windowing;
//using ECommons.Reflection;
//using ECommons.SimpleGui;
//using ImGuiNET;
//using SomethingNeedDoing.Macros.Exceptions;
//using SomethingNeedDoing.Misc;
//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Numerics;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace SomethingNeedDoing.Interface;

//internal class MacroWindow : Window
//{
//    private readonly Regex incrementalName = new(@"(?<all> \((?<index>\d+)\))$", RegexOptions.Compiled);

//    private INode? draggedNode = null;
//    private MacroNode? activeMacroNode = null;
//    private string? activeFile = null;
//    private static TitleBarButton LockButton = null!;

//    public MacroWindow() : base($"Something Need Doing {Service.Plugin.GetType().Assembly.GetName().Version}###SomethingNeedDoing")
//    {
//        Size = new Vector2(525, 600);
//        SizeCondition = ImGuiCond.FirstUseEver;
//        RespectCloseHotkey = false;
//        LockButton = new()
//        {
//            Click = OnLockButtonClick,
//            Icon = C.LockWindow ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen,
//            IconOffset = new(3, 2),
//            ShowTooltip = () => ImGui.SetTooltip("Lock window position and size"),
//        };
//        TitleBarButtons.Add(LockButton);
//        //C.BuildDirectory(C.RootFolderPath);
//    }

//    private void OnLockButtonClick(ImGuiMouseButton m)
//    {
//        if (m == ImGuiMouseButton.Left)
//        {
//            C.LockWindow = !C.LockWindow;
//            LockButton.Icon = C.LockWindow ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen;
//        }
//    }

//    private static FolderNode RootFolder => C.RootFolder;

//    public override void Update() => EzConfigGui.Window.Flags = C.LockWindow ? ImGuiWindowFlags.NoMove : 0;
//    public override void PreDraw() => ImGui.PushStyleColor(ImGuiCol.ResizeGrip, 0);
//    public override void PostDraw() => ImGui.PopStyleColor();

//    public override void Draw()
//    {
//        ImGui.Columns(2);
//        DisplayNodeTree();
//        ImGui.NextColumn();
//        DisplayMacroControls();
//        DisplayRunningMacros();
//        DisplayMacroEdit();

//        ImGui.Columns(1);
//    }

//    private void DrawHeader()
//    {
//        if (ImGuiX.IconButton(FontAwesomeIcon.Plus, "Add macro"))
//        {
//            File.Create(Path.Combine(C.RootFolderPath, GetUniqueFileName()));
//            //var newNode = new MacroNode { Name = GetUniqueNodeName("Untitled macro") };
//            //RootFolder.Children.Add(newNode);
//            //C.Save();
//        }

//        ImGui.SameLine();
//        if (ImGuiX.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
//        {
//            Directory.CreateDirectory(Path.Combine(C.RootFolderPath, GetUniqueFolderName()));
//            //var newNode = new FolderNode { Name = GetUniqueNodeName("Untitled folder") };
//            //RootFolder.Children.Add(newNode);
//            //C.Save();
//        }

//        ImGui.SameLine();
//        if (ImGuiX.IconButton(FontAwesomeIcon.FileImport, "Import macro from clipboard"))
//        {
//            var text = Utils.ConvertClipboardToSafeString();
//            var node = new MacroNode { Name = GetUniqueNodeName("Untitled macro") };
//            RootFolder.Children.Add(node);

//            if (Utils.IsLuaCode(text))
//                node.Language = Language.Lua;

//            node.Contents = text;
//            C.Save();
//        }
//    }

//    private void DisplayNodeTree()
//    {
//        DrawHeader();
//        DisplayDirectory(C.RootFolderPath);
//        //DisplayNode(RootFolder);
//    }

//    private void DisplayDirectory(string path)
//    {
//        if (!Directory.Exists(path))
//        {
//            ImGui.TextUnformatted($"{path} not found!");
//            return;
//        }

//        foreach (var d in Directory.GetDirectories(path))
//        {
//            using var tree = ImRaii.TreeNode($"{Path.GetFileName(d)}##tree");
//            if (tree)
//            {
//                NodeContextMenu(GetNodeFromPath(d));
//                DisplayDirectory(d);
//            }
//        }

//        DisplayFiles(path);

//        void DisplayFiles(string path)
//        {
//            foreach (var f in Directory.GetFiles(path))
//            {
//                var flags = ImGuiTreeNodeFlags.Leaf;
//                if (f == activeFile)
//                    flags |= ImGuiTreeNodeFlags.Selected;
//                using var tree = ImRaii.TreeNode($"{Path.GetFileNameWithoutExtension(f)}##tree", flags);
//                if (tree)
//                {
//                    NodeContextMenu(GetNodeFromPath(f));
//                    if (ImGui.IsItemClicked())
//                        activeFile = f;
//                }
//            }
//        }
//    }

//    private INode GetNodeFromPath(string path)
//        => C.GetAllNodes().ToList().FirstOrDefault(n => n.FilePath == path) ?? null!;

//    private void DisplayNode(INode node)
//    {
//        using var _ = ImRaii.PushId(node.Name);
//        if (node is FolderNode folderNode)
//            DisplayFolderNode(folderNode);
//        else if (node is MacroNode macroNode)
//            DisplayMacroNode(macroNode);
//    }

//    private void DisplayMacroNode(MacroNode node)
//    {
//        var flags = ImGuiTreeNodeFlags.Leaf;
//        if (node == activeMacroNode)
//            flags |= ImGuiTreeNodeFlags.Selected;

//        ImGui.TreeNodeEx($"{node.Name}##tree", flags);

//        NodeContextMenu(node);
//        NodeDragDrop(node);

//        if (ImGui.IsItemClicked())
//            activeMacroNode = node;

//        ImGui.TreePop();
//    }

//    private void DisplayFolderNode(FolderNode node)
//    {
//        if (node == RootFolder)
//        {
//            ImGui.SetNextItemOpen(true, ImGuiCond.FirstUseEver);
//        }

//        var expanded = ImGui.TreeNodeEx($"{node.Name}##tree");

//        NodeContextMenu(node);
//        NodeDragDrop(node);

//        if (expanded)
//        {
//            foreach (var childNode in node.Children.ToArray())
//            {
//                DisplayNode(childNode);
//            }
//            ImGui.TreePop();
//        }
//    }

//    private void NodeContextMenu(INode node)
//    {
//        if (node == null) return;
//        ImGui.OpenPopupOnItemClick($"{node.Name}ContextMenu", ImGuiPopupFlags.MouseButtonRight);
//        using var ctx = ImRaii.ContextPopupItem($"{node.Name}ContextMenu");
//        if (ctx)
//        {
//            var name = node.FileName;
//            if (ImGui.InputText($"##rename", ref name, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
//            {
//                if (File.Exists(Path.GetDirectoryName(node.FilePath) + name))
//                    Svc.Chat.Print($"File {name} already exists in {Path.GetDirectoryName(node.FilePath)}");
//                else
//                    node.Rename(name);
//                //node.Name = GetUniqueNodeName(name);
//                //C.Save();
//            }

//            if (node is MacroNode macroNode)
//                if (ImGuiX.IconButton(FontAwesomeIcon.Play, "Run"))
//                    RunMacro(macroNode);

//            if (node is FolderNode folderNode)
//            {
//                if (ImGuiX.IconButton(FontAwesomeIcon.Plus, "Add macro"))
//                {
//                    File.Create(folderNode.FilePath + $"Untitled macro.txt");
//                    //var newNode = new MacroNode { Name = GetUniqueNodeName("Untitled macro") };
//                    //folderNode.Children.Add(newNode);
//                    //C.Save();
//                }

//                ImGui.SameLine();
//                if (ImGuiX.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
//                {
//                    Directory.CreateDirectory(folderNode.FilePath + $"Untitled folder");
//                    //var newNode = new FolderNode { Name = GetUniqueNodeName("Untitled folder") };
//                    //folderNode.Children.Add(newNode);
//                    //C.Save();
//                }
//            }

//            if (node != RootFolder)
//            {
//                ImGui.SameLine();
//                if (ImGuiX.IconButton(FontAwesomeIcon.Copy, "Copy Name"))
//                    ImGui.SetClipboardText(node.Name);

//                ImGui.SameLine();
//                if (ImGuiX.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
//                {
//                    if (activeMacroNode is INode am && am.IsFileOrInFolder(node.FilePath))
//                        activeFile = null;
//                    if (node is MacroNode macro)
//                        File.Delete(macro.FilePath);
//                    if (node is FolderNode folder)
//                        Directory.Delete(folder.FilePath, true);
//                    //if (C.TryFindParent(node, out var parentNode))
//                    //{
//                    //    parentNode!.Children.Remove(node);
//                    //    C.Save();
//                    //}
//                }

//                //ImGui.SameLine();
//            }
//            ImGui.NewLine();
//            ImGui.TextUnformatted($"Path:      {node.FilePath}");
//            ImGui.TextUnformatted($"Directory: {Path.GetDirectoryName(node.FilePath)}");
//            ImGui.TextUnformatted($"File Name: {Path.GetFileName(node.FilePath)}");
//            ImGui.TextUnformatted($"Extension: {Path.GetExtension(node.FilePath)}");
//            ImGui.TextUnformatted($"Full Path: {Path.GetFullPath(node.FilePath)}");
//            ImGui.TextUnformatted($"Path Root: {Path.GetPathRoot(node.FilePath)}");
//            ImGui.TextUnformatted($"Rel. Path: {Path.GetRelativePath(C.RootFolderPath, node.FilePath)}");
//            ImGui.TextUnformatted($"Renamed:   {Path.Combine(Path.GetDirectoryName(node.FilePath), "untitled.txt")}");
//        }
//    }

//    private void DisplayMacroControls()
//    {
//        ImGui.Text("Macro Queue");

//        var state = Service.MacroManager.State;

//        var stateName = state switch
//        {
//            LoopState.NotLoggedIn => "Not Logged In",
//            LoopState.Running when Service.MacroManager.PauseAtLoop => "Pausing Soon",
//            LoopState.Running when Service.MacroManager.StopAtLoop => "Stopping Soon",
//            _ => Enum.GetName(state),
//        };

//        var buttonCol = ImGuiX.GetStyleColorVec4(ImGuiCol.Button);
//        using (var _ = ImRaii.PushColor(ImGuiCol.ButtonActive, buttonCol).Push(ImGuiCol.ButtonHovered, buttonCol))
//            ImGui.Button($"{stateName}##LoopState", new Vector2(100, 0));

//        ImGui.SameLine();
//        if (ImGuiX.IconButton(FontAwesomeIcon.QuestionCircle, "Help"))
//            EzConfigGui.GetWindow<HelpWindow>()!.Toggle();
//        ImGui.SameLine();
//        if (ImGuiX.IconButton(FontAwesomeIcon.FileExcel, "Excel Browser"))
//            EzConfigGui.GetWindow<ExcelWindow>()!.Toggle();

//        if (Service.MacroManager.State == LoopState.NotLoggedIn) { /* Nothing to do */ }
//        else if (Service.MacroManager.State == LoopState.Stopped) { /* Nothing to do */ }
//        else if (Service.MacroManager.State == LoopState.Waiting) { /* Nothing to do */ }
//        else if (Service.MacroManager.State == LoopState.Paused)
//        {
//            ImGui.SameLine();
//            if (ImGuiX.IconButton(FontAwesomeIcon.Play, "Resume"))
//                Service.MacroManager.Resume();

//            ImGui.SameLine();
//            if (ImGuiX.IconButton(FontAwesomeIcon.StepForward, "Step"))
//                Service.MacroManager.NextStep();

//            ImGui.SameLine();
//            if (ImGuiX.IconButton(FontAwesomeIcon.TrashAlt, "Clear"))
//                Service.MacroManager.Stop();
//        }
//        else if (Service.MacroManager.State == LoopState.Running)
//        {
//            ImGui.SameLine();
//            if (ImGuiX.IconButton(FontAwesomeIcon.Pause, "Pause (hold control to pause at next /loop)"))
//            {
//                var io = ImGui.GetIO();
//                var ctrlHeld = io.KeyCtrl;

//                Service.MacroManager.Pause(ctrlHeld);
//            }

//            ImGui.SameLine();
//            if (ImGuiX.IconButton(FontAwesomeIcon.Stop, "Stop (hold control to stop at next /loop)"))
//            {
//                var io = ImGui.GetIO();
//                var ctrlHeld = io.KeyCtrl;

//                Service.MacroManager.Stop(ctrlHeld);
//            }
//        }
//    }

//    private void DisplayRunningMacros()
//    {
//        ImGui.PushItemWidth(-1);

//        var style = ImGui.GetStyle();
//        var runningHeight = (ImGui.CalcTextSize("CalcTextSize").Y * ImGuiHelpers.GlobalScale * 3) + (style.FramePadding.Y * 2) + (style.ItemSpacing.Y * 2);
//        if (ImGui.BeginListBox("##running-macros", new Vector2(-1, runningHeight)))
//        {
//            var macroStatus = Service.MacroManager.MacroStatus;
//            for (var i = 0; i < macroStatus.Length; i++)
//            {
//                var (name, stepIndex) = macroStatus[i];
//                var text = name;
//                if (i == 0 || stepIndex > 1)
//                    text += $" (step {stepIndex})";
//                ImGui.Selectable($"{text}##{Guid.NewGuid()}", i == 0);
//            }

//            ImGui.EndListBox();
//        }

//        var contentHeight = (ImGui.CalcTextSize("CalcTextSize").Y * ImGuiHelpers.GlobalScale * 5) + (style.FramePadding.Y * 2) + (style.ItemSpacing.Y * 4);
//        var macroContent = Service.MacroManager.CurrentMacroContent();
//        if (ImGui.BeginListBox("##current-macro", new Vector2(-1, contentHeight)))
//        {
//            var stepIndex = Service.MacroManager.CurrentMacroStep();
//            if (stepIndex == -1)
//            {
//                ImGui.Selectable("Looping", true);
//            }
//            else
//            {
//                for (var i = stepIndex; i < macroContent.Length; i++)
//                {
//                    var step = macroContent[i];
//                    var isCurrentStep = i == stepIndex;
//                    ImGui.Selectable(step, isCurrentStep);
//                }
//            }

//            ImGui.EndListBox();
//        }

//        ImGui.PopItemWidth();
//    }

//    private void DisplayMacroEdit()
//    {
//        if (activeFile is null)
//            return;

//        activeMacroNode = C.GetAllNodes().FirstOrDefault(n => n is MacroNode m && m.FilePath == activeFile, null) as MacroNode
//            ?? new MacroNode { Name = Path.GetFileName(activeFile), FilePath = activeFile, Language = Path.GetExtension(activeFile).FileExtensionToLanguage() };
//        ImGui.TextUnformatted("Macro Editor");
//        ImGui.TextUnformatted($"{activeMacroNode.FilePath.Replace(Svc.PluginInterface.ConfigDirectory.FullName, string.Empty)}");
//        ImGui.SameLine();
//        if (ImGui.Button("open"))
//        {
//            if (File.Exists(activeMacroNode.FilePath))
//            {
//                try
//                {
//                    Process.Start(new ProcessStartInfo(activeMacroNode.FilePath) { UseShellExecute = true });
//                }
//                catch (Exception e)
//                {
//                    Svc.Log.Error($"Error opening file {activeMacroNode.FilePath}: {e}");
//                }
//            }
//        }
//        if (ImGuiX.IconButton(FontAwesomeIcon.Play, "Run"))
//            RunMacro(activeMacroNode);

//        ImGui.SameLine();
//        if (ImGuiX.IconButton(FontAwesomeIcon.TimesCircle, "Close"))
//            activeMacroNode = null;

//        ImGui.SameLine();
//        var lang = activeMacroNode.Language;
//        if (ImGuiX.Enum("Language", ref lang))
//        {
//            Path.ChangeExtension(activeMacroNode.FilePath, lang.LanguageToFileExtension());
//        }
//        if (activeMacroNode.Language == Language.Native)
//        {
//            var sb = new StringBuilder("Toggle CraftLoop");
//            var craftLoopEnabled = activeMacroNode.CraftingLoop;

//            if (craftLoopEnabled)
//            {
//                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.HealerGreen);
//                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGuiColors.HealerGreen);
//                ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGuiColors.ParsedGreen);

//                sb.AppendLine(" (0=disabled, -1=infinite)");
//                sb.AppendLine($"When enabled, your macro is modified as follows:");
//                sb.AppendLine(
//                    ActiveMacro.ModifyMacroForCraftLoop("[YourMacro]", true, activeMacroNode.CraftLoopCount)
//                    .Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
//                    .Select(line => $"- {line}")
//                    .Aggregate(string.Empty, (s1, s2) => $"{s1}\n{s2}"));
//            }

//            ImGui.SameLine();
//            if (ImGuiX.IconButton(FontAwesomeIcon.Sync, sb.ToString()))
//            {
//                activeMacroNode.CraftingLoop ^= true;
//                C.Save();
//            }

//            if (craftLoopEnabled)
//                ImGui.PopStyleColor(3);

//            if (activeMacroNode.CraftingLoop)
//            {
//                ImGui.SameLine();
//                ImGui.PushItemWidth(50);

//                var v_min = -1;
//                var v_max = 999;
//                var loops = activeMacroNode.CraftLoopCount;
//                if (ImGui.InputInt("##CraftLoopCount", ref loops, 0) || MouseWheelInput(ref loops))
//                {
//                    if (loops < v_min)
//                        loops = v_min;

//                    if (loops > v_max)
//                        loops = v_max;

//                    activeMacroNode.CraftLoopCount = loops;
//                    C.Save();
//                }

//                ImGui.PopItemWidth();
//            }
//        }

//        ImGui.SameLine();
//        var buttonSize = ImGuiHelpers.GetButtonSize(FontAwesomeIcon.FileImport.ToIconString());
//        ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - buttonSize.X - ImGui.GetStyle().WindowPadding.X);
//        if (ImGuiX.IconButton(FontAwesomeIcon.FileImport, "Import from clipboard"))
//        {
//            var text = Utils.ConvertClipboardToSafeString();

//            if (Utils.IsLuaCode(text))
//                activeMacroNode.Language = Language.Lua;

//            File.WriteAllText(activeMacroNode.FilePath, text);
//            //activeMacroNode.Contents = text;
//            //C.Save();
//        }

//        ImGui.SetNextItemWidth(-1);
//        var useMono = !C.DisableMonospaced;
//        using var font = ImRaii.PushFont(UiBuilder.MonoFont, useMono);

//        var contents = File.ReadAllText(activeMacroNode.FilePath); // FIX: if activefile is opened and deleted this will throw
//        if (ImGui.InputTextMultiline($"##{activeMacroNode.Name}-editor", ref contents, 100_000, new Vector2(-1, -1)))
//            File.WriteAllText(activeMacroNode.FilePath, contents);
//    }

//    private string GetUniqueFileName(int count = 0)
//    {
//        var fileName = count == 0 ? $"Untitled macro.txt" : $"Untitled macro ({count}).txt";
//        return File.Exists(Path.Combine(C.RootFolderPath, fileName)) ? GetUniqueFileName(count + 1) : fileName;
//    }

//    private string GetUniqueFolderName(int count = 0)
//    {
//        var dirName = count == 0 ? $"Untitled folder" : $"Untitled Folder ({count})";
//        return Directory.Exists(Path.Combine(C.RootFolderPath, dirName)) ? GetUniqueFolderName(count + 1) : dirName;
//    }

//    private string GetUniqueNodeName(string name)
//    {
//        var nodeNames = C.GetAllNodes()
//            .Select(node => node.Name)
//            .ToList();

//        while (nodeNames.Contains(name))
//        {
//            var match = incrementalName.Match(name);
//            if (match.Success)
//            {
//                var all = match.Groups["all"].Value;
//                var index = int.Parse(match.Groups["index"].Value) + 1;
//                name = name[..^all.Length];
//                name = $"{name} ({index})";
//            }
//            else
//            {
//                name = $"{name} (1)";
//            }
//        }

//        return name.Trim();
//    }

//    private void NodeDragDrop(INode node)
//    {
//        if (node != RootFolder)
//        {
//            if (ImGui.BeginDragDropSource())
//            {
//                draggedNode = node;
//                ImGui.Text(node.Name);
//                ImGui.SetDragDropPayload("NodePayload", IntPtr.Zero, 0);
//                ImGui.EndDragDropSource();
//            }
//        }

//        if (ImGui.BeginDragDropTarget())
//        {
//            var payload = ImGui.AcceptDragDropPayload("NodePayload");

//            bool nullPtr;
//            unsafe
//            {
//                nullPtr = payload.NativePtr == null;
//            }

//            var targetNode = node;
//            if (!nullPtr && payload.IsDelivery() && draggedNode != null)
//            {
//                if (!C.TryFindParent(draggedNode, out var draggedNodeParent))
//                    throw new Exception($"Could not find parent of node \"{draggedNode.Name}\"");

//                if (targetNode is FolderNode targetFolderNode)
//                {
//                    draggedNodeParent!.Children.Remove(draggedNode);
//                    targetFolderNode.Children.Add(draggedNode);
//                    C.Save();
//                }
//                else
//                {
//                    if (!C.TryFindParent(targetNode, out var targetNodeParent))
//                        throw new Exception($"Could not find parent of node \"{targetNode.Name}\"");

//                    var targetNodeIndex = targetNodeParent!.Children.IndexOf(targetNode);
//                    if (targetNodeParent == draggedNodeParent)
//                    {
//                        var draggedNodeIndex = targetNodeParent.Children.IndexOf(draggedNode);
//                        if (draggedNodeIndex < targetNodeIndex)
//                        {
//                            targetNodeIndex -= 1;
//                        }
//                    }

//                    draggedNodeParent!.Children.Remove(draggedNode);
//                    targetNodeParent.Children.Insert(targetNodeIndex, draggedNode);
//                    C.Save();
//                }

//                draggedNode = null;
//            }

//            ImGui.EndDragDropTarget();
//        }
//    }

//    private void RunMacro(MacroNode node)
//    {
//        try
//        {
//            Service.MacroManager.EnqueueMacro(node);
//        }
//        catch (MacroSyntaxError ex)
//        {
//            Old.Service.ChatManager.PrintError($"{ex.Message}");
//        }
//        catch (Exception ex)
//        {
//            Old.Service.ChatManager.PrintError($"Unexpected error");
//            Svc.Log.Error(ex, "Unexpected error");
//        }
//    }

//    private bool MouseWheelInput(ref int iv)
//    {
//        if (ImGui.IsItemHovered())
//        {
//            var mouseDelta = (int)ImGui.GetIO().MouseWheel;  // -1, 0, 1
//            if (mouseDelta != 0)
//            {
//                iv += mouseDelta;
//                return true;
//            }
//        }

//        return false;
//    }
//}
