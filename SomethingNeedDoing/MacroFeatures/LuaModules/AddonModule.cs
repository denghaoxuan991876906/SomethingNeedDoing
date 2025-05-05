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

        public bool Exists => (nint)Addon != IntPtr.Zero;
        public bool Ready => IsAddonReady(Addon);

        public AtkValueWrapper GetAtkValue(int index) => new(Addon->AtkValues[index]);
        public unsafe IEnumerable<AtkValueWrapper> AtkValues
        {
            get
            {
                foreach (var v in AtkValuesList)
                    yield return new AtkValueWrapper(v);
            }
        }

        public NodeWrapper GetNode(params int[] nodeIds) => new(Addon, nodeIds);
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

        public uint Id => Node->NodeId;
        public bool IsVisible => Node->IsVisible();
        public string Text { get => Node->GetAsAtkTextNode()->NodeText.ToString(); set => Node->GetAsAtkTextNode()->NodeText.SetString(value); }
        public NodeType NodeType => Node->Type;
    }

    public class AtkValueWrapper(AtkValue value)
    {
        private AtkValue Value = value;

        public string ValueString => Value.GetValueAsString();
    }
}
