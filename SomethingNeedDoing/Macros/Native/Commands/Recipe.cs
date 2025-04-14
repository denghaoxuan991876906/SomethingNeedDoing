using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Macros.Native.Modifiers;

namespace SomethingNeedDoing.Macros.Native.Commands;
/// <summary>
/// Opens the recipe window to a specific recipe.
/// </summary>
public class RecipeCommand(string text, string recipeName, WaitModifier? waitMod = null) : MacroCommandBase(text, waitMod?.WaitDuration ?? 0)
{
    /// <inheritdoc/>
    public override bool RequiresFrameworkThread => true;

    /// <inheritdoc/>
    public override async Task Execute(MacroContext context, CancellationToken token)
    {
        await context.RunOnFramework(() =>
        {
            unsafe
            {
                if (TryGetAddonByName<AtkUnitBase>("Synthesis", out _))
                    throw new MacroException("/recipe cannot be used while the Synthesis window is open.");

                var recipeId = SearchRecipeId(recipeName);
                Svc.Log.Debug($"Recipe found: {recipeId}");

                OpenRecipeNote(recipeId);
            }
        });

        await PerformWait(token);
    }

    private unsafe void OpenRecipeNote(uint recipeId)
    {
        var agent = AgentRecipeNote.Instance();
        if (agent == null)
            throw new MacroException("AgentRecipeNote not found");

        agent->OpenRecipeByRecipeId(recipeId);
    }

    private uint SearchRecipeId(string recipeName)
    {
        var sheet = Svc.Data.GetExcelSheet<Recipe>()!;
        var recipes = sheet.Where(r =>
            r.ItemResult.Value!.Name.ToString().Equals(recipeName, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        return recipes.Count switch
        {
            0 => throw new MacroException("Recipe not found"),
            1 => recipes.First().RowId,
            _ => FindRecipeForCurrentJob(recipes)
        };
    }

    private uint FindRecipeForCurrentJob(List<Recipe> recipes)
    {
        var jobId = Svc.ClientState.LocalPlayer?.ClassJob.RowId;
        var recipe = recipes.FirstOrDefault(r => GetClassJobId(r) == jobId);
        return recipe.RowId;
    }

    private static uint GetClassJobId(Recipe recipe) => recipe.CraftType.RowId + 8;

    /// <summary>
    /// Parses a recipe command from text.
    /// </summary>
    public static RecipeCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitMod);

        var match = Regex.Match(text, @"^/recipe\s+(?<name>.*?)\s*$", RegexOptions.Compiled);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var recipeName = match.Groups["name"].Value.Trim('"');
        return new(text, recipeName, waitMod as WaitModifier);
    }
}
