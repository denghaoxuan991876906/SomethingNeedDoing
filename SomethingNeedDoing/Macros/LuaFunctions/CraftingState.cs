using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using SomethingNeedDoing.Macros.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SomethingNeedDoing.Macros.Lua;

public class CraftingState()
{
    internal static CraftingState Instance { get; } = new();

    public List<string> ListAllFunctions()
    {
        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var list = new List<string>();
        foreach (var method in methods.Where(x => x.Name != nameof(ListAllFunctions) && x.DeclaringType != typeof(object)))
        {
            var parameterList = method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue ?? "null") : "")}");
            list.Add($"{method.ReturnType.Name} {method.Name}({string.Join(", ", parameterList)})");
        }
        return list;
    }
    public bool IsCrafting() => Svc.Condition[ConditionFlag.Crafting] && !Svc.Condition[ConditionFlag.PreparingToCraft];

    public bool IsNotCrafting() => !IsCrafting();

    private unsafe int GetNodeTextAsInt(AtkTextNode* node, string error)
    {
        try
        {
            if (node == null)
                throw new NullReferenceException("TextNode is null");

            var text = node->NodeText.ToString();
            var value = int.Parse(text);
            return value;
        }
        catch (Exception ex)
        {
            throw new MacroCommandError(error, ex);
        }
    }

    private unsafe AddonSynthesis* GetSynthesisAddon()
    {
        var ptr = Svc.GameGui.GetAddonByName("Synthesis", 1);
        return ptr == nint.Zero ? throw new MacroCommandError("Could not find Synthesis addon") : (AddonSynthesis*)ptr;
    }

    public unsafe bool IsCollectable()
    {
        var addon = GetSynthesisAddon();
        return addon->AtkUnitBase.UldManager.NodeList[34]->IsVisible();
    }

    public unsafe string GetCondition(bool lower = true)
    {
        var addon = GetSynthesisAddon();
        var text = addon->Condition->NodeText.ToString();

        if (lower)
            text = text.ToLowerInvariant();

        return text;
    }

    public bool HasCondition(string condition, bool lower = true) => condition == GetCondition(lower);

    public unsafe int GetProgress()
    {
        var addon = GetSynthesisAddon();
        return GetNodeTextAsInt(addon->CurrentProgress, "Could not parse current progress number in the Synthesis addon");
    }

    public unsafe int GetMaxProgress()
    {
        var addon = GetSynthesisAddon();
        return GetNodeTextAsInt(addon->MaxProgress, "Could not parse max progress number in the Synthesis addon");
    }

    public bool HasMaxProgress()
    {
        var current = GetProgress();
        var max = GetMaxProgress();
        return current == max;
    }

    public unsafe int GetQuality()
    {
        var addon = GetSynthesisAddon();
        return GetNodeTextAsInt(addon->CurrentQuality, "Could not parse current quality number in the Synthesis addon");
    }

    public unsafe int GetMaxQuality()
    {
        var addon = GetSynthesisAddon();
        return GetNodeTextAsInt(addon->MaxQuality, "Could not parse max quality number in the Synthesis addon");
    }

    public bool HasMaxQuality()
    {
        var step = GetStep();

        if (step <= 1)
            return false;

        if (IsCollectable())
        {
            var current = GetQuality();
            var max = GetMaxQuality();
            return current == max;
        }
        else
        {
            var percentHq = GetPercentHQ();
            return percentHq == 100;
        }
    }

    public unsafe int GetDurability()
    {
        var addon = GetSynthesisAddon();
        return GetNodeTextAsInt(addon->CurrentDurability, "Could not parse current durability number in the Synthesis addon");
    }

    public unsafe int GetMaxDurability()
    {
        var addon = GetSynthesisAddon();
        return GetNodeTextAsInt(addon->StartingDurability, "Could not parse max durability number in the Synthesis addon");
    }

    public int GetCp()
    {
        var cp = Svc.ClientState.LocalPlayer?.CurrentCp ?? 0;
        return (int)cp;
    }

    public int GetMaxCp()
    {
        var cp = Svc.ClientState.LocalPlayer?.MaxCp ?? 0;
        return (int)cp;
    }

    public int GetGp()
    {
        var gp = Svc.ClientState.LocalPlayer?.CurrentGp ?? 0;
        return (int)gp;
    }

    public int GetMaxGp()
    {
        var gp = Svc.ClientState.LocalPlayer?.MaxGp ?? 0;
        return (int)gp;
    }

    public unsafe int GetStep()
    {
        var addon = GetSynthesisAddon();
        var step = GetNodeTextAsInt(addon->StepNumber, "Could not parse current step number in the Synthesis addon");
        return step;
    }

    public unsafe int GetPercentHQ()
    {
        var addon = GetSynthesisAddon();
        var step = GetNodeTextAsInt(addon->HQPercentage, "Could not parse percent hq number in the Synthesis addon");
        return step;
    }

    public unsafe bool NeedsRepair(float below = 0)
    {
        var im = InventoryManager.Instance();
        if (im == null)
        {
            Svc.Log.Error("InventoryManager was null");
            return false;
        }

        var equipped = im->GetInventoryContainer(InventoryType.EquippedItems);
        if (equipped == null)
        {
            Svc.Log.Error("InventoryContainer was null");
            return false;
        }

        if (!equipped->IsLoaded)
        {
            Svc.Log.Error($"InventoryContainer is not loaded");
            return false;
        }

        for (var i = 0; i < equipped->Size; i++)
        {
            var item = equipped->GetInventorySlot(i);
            if (item == null)
                continue;

            var itemCondition = Convert.ToInt32(Convert.ToDouble(item->Condition) / 30000.0 * 100.0);

            if (itemCondition <= below)
                return true;
        }

        return false;
    }

    public unsafe bool CanExtractMateria(float within = 100)
    {
        var im = InventoryManager.Instance();
        if (im == null)
        {
            Svc.Log.Error("InventoryManager was null");
            return false;
        }

        var equipped = im->GetInventoryContainer(InventoryType.EquippedItems);
        if (equipped == null)
        {
            Svc.Log.Error("InventoryContainer was null");
            return false;
        }

        if (!equipped->IsLoaded)
        {
            Svc.Log.Error("InventoryContainer is not loaded");
            return false;
        }

        var nextHighest = 0f;
        var canExtract = false;
        var allExtract = true;
        for (var i = 0; i < equipped->Size; i++)
        {
            var item = equipped->GetInventorySlot(i);
            if (item == null)
                continue;

            var spiritbond = item->SpiritbondOrCollectability / 100;
            if (spiritbond == 100f)
            {
                canExtract = true;
            }
            else
            {
                allExtract = false;
                nextHighest = Math.Max(spiritbond, nextHighest);
            }
        }

        if (allExtract)
        {
            Svc.Log.Debug("All items are spiritbound, pausing");
            return true;
        }

        if (canExtract)
        {
            // Don't wait, extract immediately
            if (within == 100)
            {
                Svc.Log.Debug("An item is spiritbound, pausing");
                return true;
            }

            // Keep going if the next highest spiritbonded item is within the allowed range
            // i.e. 100 and 99, do another craft to finish the 99.
            if (nextHighest >= within)
            {
                Svc.Log.Debug($"The next highest spiritbond is above ({nextHighest} >= {within}), keep going");
                return false;
            }
            else
            {
                Svc.Log.Debug($"The next highest spiritbond is below ({nextHighest} < {within}), pausing");
                return true;
            }
        }

        return false;
    }

    public unsafe bool HasStats(uint craftsmanship, uint control, uint cp)
    {
        var uiState = UIState.Instance();
        if (uiState == null)
        {
            Svc.Log.Error("UIState is null");
            return false;
        }

        var hasStats =
            uiState->PlayerState.Attributes[70] >= craftsmanship &&
            uiState->PlayerState.Attributes[71] >= control &&
            uiState->PlayerState.Attributes[11] >= cp;

        return hasStats;
    }

    public unsafe uint GetProgressIncrease(uint actionID) => GetActionResult(actionID).Progress;

    public unsafe uint GetQualityIncrease(uint actionID) => GetActionResult(actionID).Quality;

    private unsafe (uint Progress, uint Quality) GetActionResult(uint id)
    {
        var agent = AgentCraftActionSimulator.Instance();
        if (agent == null) return (0, 0);

        var progress = agent->Progress.FirstOrDefault(_p => _p.ActionId == id).ProgressIncrease;
        var quality = agent->Quality.FirstOrDefault(q => q.ActionId == id).QualityIncrease;

        return (progress, quality);
    }
}
