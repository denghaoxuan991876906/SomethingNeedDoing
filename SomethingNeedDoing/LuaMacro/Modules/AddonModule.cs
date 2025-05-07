using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using NLua;

namespace SomethingNeedDoing.LuaMacro.Modules;
public unsafe class AddonModule : LuaModuleBase
{
    public override string ModuleName => "Addons";

    [LuaFunction] public AddonWrapper GetAddon(string name) => new(name);

    public class AddonWrapper(string name)
    {
        private AtkUnitBase* Addon => (AtkUnitBase*)Svc.GameGui.GetAddonByName(name);
        private Pointer<AtkResNode>[] NodeList => Addon->UldManager.Nodes.ToArray();
        private AtkValue[] AtkValuesList => Addon->AtkValuesSpan.ToArray();

        [LuaDocs] public bool Exists => (nint)Addon != nint.Zero;
        [LuaDocs] public bool Ready => IsAddonReady(Addon);

        [LuaDocs] public AtkValueWrapper GetAtkValue(int index) => new(Addon->AtkValues[index]);

        [LuaDocs]
        public unsafe IEnumerable<AtkValueWrapper> AtkValues
        {
            get
            {
                foreach (var v in AtkValuesList)
                    yield return new AtkValueWrapper(v);
            }
        }

        [LuaDocs] public NodeWrapper GetNode(params int[] nodeIds) => new(Addon, nodeIds);

        [LuaDocs]
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

        [LuaDocs] public uint Id => Node->NodeId;
        [LuaDocs] public bool IsVisible => Node->IsVisible();
        [LuaDocs] public string Text { get => Node->GetAsAtkTextNode()->NodeText.ToString(); set => Node->GetAsAtkTextNode()->NodeText.SetString(value); }
        [LuaDocs] public NodeType NodeType => Node->Type;
    }

    public class AtkValueWrapper(AtkValue value)
    {
        private AtkValue Value = value;

        [LuaDocs] public string ValueString => Value.GetValueAsString();
    }
}
