using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using NLua;

namespace SomethingNeedDoing.MacroFeatures.LuaModules;
public unsafe class AddonModule : LuaModuleBase
{
    public override string ModuleName => "Addons";

    [LuaFunction] public AddonWrapper GetAddon(string name) => new(name);

    public class AddonWrapper(string name)
    {
        private AtkUnitBase* Addon => (AtkUnitBase*)Svc.GameGui.GetAddonByName(name);
        private Span<Pointer<AtkResNode>> NodeList => Addon->UldManager.Nodes;
        private Span<AtkValue> AtkValuesList => Addon->AtkValuesSpan;

        [LuaWrapper] public bool Exists => (nint)Addon != IntPtr.Zero;
        [LuaWrapper] public bool Ready => IsAddonReady(Addon);

        [LuaWrapper] public AtkValueWrapper GetAtkValue(int index) => new(Addon->AtkValues[index]);

        [LuaWrapper]
        public unsafe IEnumerable<AtkValueWrapper> AtkValues
        {
            get
            {
                foreach (var v in AtkValuesList)
                    yield return new AtkValueWrapper(v);
            }
        }

        [LuaWrapper] public NodeWrapper GetNode(params int[] nodeIds) => new(Addon, nodeIds);

        [LuaWrapper]
        public unsafe IEnumerable<NodeWrapper> Nodes
        {
            get
            {
                foreach (var node in NodeList)
                    yield return new NodeWrapper(node);
            }
        }
    }

    public class NodeWrapper
    {
        public NodeWrapper(AtkUnitBase* addon, params int[] nodeIds) => Node = GetNodeByIDChain(addon->RootNode, nodeIds);
        public NodeWrapper(Pointer<AtkResNode> node) => Node = node.Value;
        private AtkResNode* Node { get; set; }

        [LuaWrapper] public uint Id => Node->NodeId;
        [LuaWrapper] public bool IsVisible => Node->IsVisible();
        [LuaWrapper] public string Text { get => Node->GetAsAtkTextNode()->NodeText.ToString(); set => Node->GetAsAtkTextNode()->NodeText.SetString(value); }
        [LuaWrapper] public NodeType NodeType => Node->Type;
    }

    public class AtkValueWrapper(AtkValue value)
    {
        private AtkValue Value = value;

        [LuaWrapper] public string ValueString => Value.GetValueAsString();
    }
}
