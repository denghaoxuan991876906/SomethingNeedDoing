using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SomethingNeedDoing.Old.Macros.Commands.Modifiers;
using SomethingNeedDoing.Old.Macros.Exceptions;
using SomethingNeedDoing.Old.Misc;

namespace SomethingNeedDoing.Old.Macros.Commands;

internal class RecipeCommand : MacroCommand
{
    public static string[] Commands => ["recipe"];
    public static string Description => "Open the recipe book to a specific recipe.";
    public static string[] Examples => ["/recipe \"Tsai tou Vounou\""];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly string recipeName;

    private RecipeCommand(string text, string recipeName, WaitModifier wait) : base(text, wait) => this.recipeName = recipeName.ToLowerInvariant();

    public static RecipeCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");

        return new RecipeCommand(text, nameValue, waitModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        if (AddonSynthesisIsOpen())
            throw new MacroCommandError("/recipe cannot be used while the Synthesis window is open.");

        var recipeId = SearchRecipeId(recipeName);
        Svc.Log.Debug($"Recipe found: {recipeId}");

        OpenRecipeNote(recipeId);

        await PerformWait(token);
    }

    private unsafe bool AddonSynthesisIsOpen() => TryGetAddonByName<AtkUnitBase>("Synthesis", out _);

    private unsafe void OpenRecipeNote(uint recipeID)
    {
        var agent = AgentRecipeNote.Instance();
        if (agent == null)
            throw new MacroCommandError("AgentRecipeNote not found");

        agent->OpenRecipeByRecipeId(recipeID);
    }

    private uint SearchRecipeId(string recipeName)
    {
        var sheet = Svc.Data.GetExcelSheet<Sheets.Recipe>()!;
        var recipes = sheet.Where(r => r.ItemResult.Value.Name.ToString().Equals(recipeName, System.StringComparison.InvariantCultureIgnoreCase)).ToList();

        switch (recipes.Count)
        {
            case 0: throw new MacroCommandError("Recipe not found");
            case 1: return recipes.First().RowId;
            default:
                var jobId = Svc.ClientState.LocalPlayer?.ClassJob.RowId;

                var recipe = recipes.FirstOrDefault(r => GetClassJobID(r) == jobId);
                return recipe.RowId;
        }
    }

    private uint GetClassJobID(Sheets.Recipe recipe)
        // Name           CraftType ClassJob
        // Carpenter      0         8
        // Blacksmith     1         9
        // Armorer        2         10
        // Goldsmith      3         11
        // Leatherworker  4         12
        // Weaver         5         13
        // Alchemist      6         14
        // Culinarian     7         15
        => recipe.CraftType.Value!.RowId + 8;
}
