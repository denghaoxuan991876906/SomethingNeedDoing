using FFXIVClientStructs.FFXIV.Client.UI.Info;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.LuaMacro.Wrappers;
public unsafe class FreeCompanyWrapper : IWrapper
{
    private InfoProxyFreeCompany* FreeCompanyProxy => (InfoProxyFreeCompany*)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->UIModule->GetInfoModule()->GetInfoProxyById(InfoProxyId.FreeCompany);

    [LuaDocs] public FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany GrandCompany => FreeCompanyProxy->GrandCompany;
    [LuaDocs] public byte Rank => FreeCompanyProxy->Rank;
    [LuaDocs] public int OnlineMemebers => FreeCompanyProxy->OnlineMembers;
    [LuaDocs] public int TotalMembers => FreeCompanyProxy->TotalMembers;
    [LuaDocs] public string Name => FreeCompanyProxy->Name.ToString();
    [LuaDocs] public ulong Id => FreeCompanyProxy->Id;
}
