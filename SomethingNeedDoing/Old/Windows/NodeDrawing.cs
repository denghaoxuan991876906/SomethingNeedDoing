//using Dalamud.Interface;
//using Dalamud.Interface.Colors;
//using Dalamud.Interface.Utility;
//using Dalamud.Interface.Utility.Raii;
//using ECommons.ImGuiMethods;
//using SomethingNeedDoing.Old.Misc;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace SomethingNeedDoing.Old.Windows;
//internal class NodeDrawing
//{
//    private readonly Regex incrementalName = new(@"(?<all> \((?<index>\d+)\))$", RegexOptions.Compiled);
//    private MacroNode? Selected = null;
//    private INode? draggedNode = null;

//    public void DisplayNodeTree()
//    {
//        DrawHeader();
//        DisplayNode(C.RootFolder);
//    }

//    public void DrawSelected()
//    {
//        using var child = ImRaii.Child("##Panel", -Vector2.One, true);
//        if (!child || Selected == null) return;
//        ImGui.TextUnformatted("Macro Editor");

//        using var disabled = ImRaii.Disabled(Service.MacroManager.State == LoopState.Running);

//        if (ImGuiEx.IconButton(FontAwesomeIcon.Play, "Run"))
//            Selected.Run();

//        ImGui.SameLine();
//        ImGui.SetNextItemWidth(150);
//        var lang = Selected.Language;
//        if (ImGuiEx.EnumCombo("Language", ref lang, l => l != Language.CSharp))
//        {
//            Selected.Language = lang;
//            C.Save();
//        }
//        if (Selected.Language == Language.Native)
//        {
//            var sb = new StringBuilder("Toggle CraftLoop");

//            if (Selected.CraftingLoop)
//            {
//                sb.AppendLine(" (0=disabled, -1=infinite)");
//                sb.AppendLine($"When enabled, your macro is modified as follows:");
//                sb.AppendLine(
//                    ActiveMacro.ModifyMacroForCraftLoop("[YourMacro]", true, Selected.CraftLoopCount)
//                    .Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
//                    .Select(line => $"- {line}")
//                    .Aggregate(string.Empty, (s1, s2) => $"{s1}\n{s2}"));
//            }
//            using (var craftLoopEnabled = ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.HealerGreen, Selected.CraftingLoop)
//                .Push(ImGuiCol.ButtonHovered, ImGuiColors.HealerGreen, Selected.CraftingLoop)
//                .Push(ImGuiCol.ButtonActive, ImGuiColors.ParsedGreen, Selected.CraftingLoop))
//            {
//                ImGui.SameLine();
//                if (ImGuiX.IconButton(FontAwesomeIcon.Sync, sb.ToString()))
//                {
//                    Selected.CraftingLoop ^= true;
//                    C.Save();
//                }
//            }

//            if (Selected.CraftingLoop)
//            {
//                ImGui.SameLine();
//                ImGui.SetNextItemWidth(50);

//                var v_min = -1;
//                var v_max = 999;
//                var loops = Selected.CraftLoopCount;
//                if (ImGui.InputInt("##CraftLoopCount", ref loops, 0) || MouseWheelInput(ref loops))
//                {
//                    if (loops < v_min)
//                        loops = v_min;

//                    if (loops > v_max)
//                        loops = v_max;

//                    Selected.CraftLoopCount = loops;
//                    C.Save();
//                }
//            }
//        }

//        ImGui.SameLine();
//        var buttonSize = ImGuiHelpers.GetButtonSize(FontAwesomeIcon.FileImport.ToIconString());
//        ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - buttonSize.X - ImGui.GetStyle().WindowPadding.X);
//        if (ImGuiX.IconButton(FontAwesomeIcon.FileImport, "Import from clipboard"))
//        {
//            var text = Utils.ConvertClipboardToSafeString();

//            if (Utils.IsLuaCode(text))
//                Selected.Language = Language.Lua;

//            Selected.Contents = text;
//            C.Save();
//        }

//        ImGui.SetNextItemWidth(-1);
//        using var font = ImRaii.PushFont(UiBuilder.MonoFont, !C.DisableMonospaced);

//        var contents = Selected.Contents;
//        if (ImGui.InputTextMultiline($"##{Selected.Name}-editor", ref contents, 1_000_000, new Vector2(-1, -1)))
//        {
//            Selected.Contents = contents;
//            C.Save();
//        }
//    }

//    private void DrawHeader()
//    {
//        if (ImGuiX.IconButton(FontAwesomeIcon.Plus, "Add macro"))
//        {
//            var newNode = new MacroNode { Name = GetUniqueNodeName("Untitled macro") };
//            C.RootFolder.Children.Add(newNode);
//            C.Save();
//        }

//        ImGui.SameLine();
//        if (ImGuiX.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
//        {
//            var newNode = new FolderNode { Name = GetUniqueNodeName("Untitled folder") };
//            C.RootFolder.Children.Add(newNode);
//            C.Save();
//        }

//        ImGui.SameLine();
//        if (ImGuiX.IconButton(FontAwesomeIcon.FileImport, "Import macro from clipboard"))
//        {
//            var text = Utils.ConvertClipboardToSafeString();
//            var node = new MacroNode { Name = GetUniqueNodeName("Untitled macro") };
//            C.RootFolder.Children.Add(node);

//            //if (Utils.IsLuaCode(text))
//            //    node.Language = Language.Lua;

//            node.Contents = text;
//            C.Save();
//        }
//    }

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
//        if (node == Selected)
//            flags |= ImGuiTreeNodeFlags.Selected;

//        ImGui.TreeNodeEx($"{node.Name}##tree", flags);

//        NodeContextMenu(node);
//        NodeDragDrop(node);

//        if (ImGui.IsItemClicked())
//            Selected = node;

//        ImGui.TreePop();
//    }

//    private void DisplayFolderNode(FolderNode node)
//    {
//        if (node == C.RootFolder)
//            ImGui.SetNextItemOpen(true, ImGuiCond.FirstUseEver);

//        var expanded = ImGui.TreeNodeEx($"{node.Name}##tree");

//        NodeContextMenu(node);
//        NodeDragDrop(node);

//        if (expanded)
//        {
//            foreach (var childNode in node.Children.ToArray())
//                DisplayNode(childNode);
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
//            var name = node.Name;
//            if (ImGui.InputText($"##rename", ref name, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
//            {
//                node.Name = GetUniqueNodeName(name);
//                C.Save();
//            }

//            if (node is MacroNode macroNode)
//                if (ImGuiX.IconButton(FontAwesomeIcon.Play, "Run"))
//                    macroNode.Run();

//            if (node is FolderNode folderNode)
//            {
//                if (ImGuiX.IconButton(FontAwesomeIcon.Plus, "Add macro"))
//                {
//                    var newNode = new MacroNode { Name = GetUniqueNodeName("Untitled macro") };
//                    folderNode.Children.Add(newNode);
//                    C.Save();
//                }

//                ImGui.SameLine();
//                if (ImGuiX.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
//                {
//                    var newNode = new FolderNode { Name = GetUniqueNodeName("Untitled folder") };
//                    folderNode.Children.Add(newNode);
//                    C.Save();
//                }
//            }

//            if (node != C.RootFolder)
//            {
//                ImGui.SameLine();
//                if (ImGuiX.IconButton(FontAwesomeIcon.Copy, "Copy Name"))
//                    ImGui.SetClipboardText(node.Name);

//                ImGui.SameLine();
//                if (ImGuiX.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
//                {
//                    if (C.TryFindParent(node, out var parentNode))
//                    {
//                        parentNode!.Children.Remove(node);
//                        C.Save();
//                    }
//                }

//                ImGui.SameLine();
//            }
//        }
//    }

//    private string GetUniqueNodeName(string name)
//    {
//        var nodeNames = C.GetAllNodes().Select(node => node.Name).ToList();

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
//                name = $"{name} (1)";
//        }

//        return name.Trim();
//    }

//    private void NodeDragDrop(INode node)
//    {
//        if (node != C.RootFolder)
//        {
//            if (ImGui.BeginDragDropSource())
//            {
//                draggedNode = node;
//                ImGui.TextUnformatted(node.Name);
//                ImGui.SetDragDropPayload("NodePayload", nint.Zero, 0);
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
