using ECommons.ExcelServices;
using Lumina.Excel.Sheets;
using SomethingNeedDoing.Core.Interfaces;

namespace SomethingNeedDoing.LuaMacro.Wrappers;
public class JobWrapper(uint classJobId) : IWrapper
{
    [LuaDocs] public uint Id => classJobId;
    [LuaDocs] public string Name => GetRow<ClassJob>(Id)?.Name.ToString() ?? string.Empty;
    [LuaDocs] public string Abbreviation => GetRow<ClassJob>(Id)?.Abbreviation.ToString() ?? string.Empty;
    [LuaDocs] public bool IsCrafter => Id is >= 8 and <= 15;
    [LuaDocs] public bool IsGatherer => Id is >= 16 and <= 18;
    [LuaDocs] public bool IsMeleeDPS => Id is 2 or 4 or 20 or 22 or 29 or 30 or 34 or 39;
    [LuaDocs] public bool IsRangedDPS => Id is 5 or 23 or 31 or 38;
    [LuaDocs] public bool IsMagicDPS => Id is 7 or 25 or 26 or 27 or 35;
    [LuaDocs] public bool IsHealer => Id is 6 or 24 or 28 or 33 or 40;
    [LuaDocs] public bool IsTank => Id is 3 or 19 or 21 or 32 or 37;
    [LuaDocs] public bool IsDPS => IsMeleeDPS || IsRangedDPS || IsMagicDPS;
    [LuaDocs] public bool IsDiscipleOfWar => IsMeleeDPS || IsRangedDPS || IsTank;
    [LuaDocs] public bool IsDiscipleOfMagic => IsMagicDPS || IsHealer;
    [LuaDocs] public bool IsBlu => Id is 36;
    [LuaDocs] public bool IsLimited => IsBlu;
    [LuaDocs(description: "Unsynced level (only for LocalPlayer)")] public int Level => Player.GetUnsyncedLevel((Job)Id);
}
