using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.NativeMacro.Commands;
/// <summary>
/// Opens the recipe window to a specific recipe.
/// </summary>
[GenericDoc(
    "Open the recipe window to a specific recipe",
    ["recipeName"],
    ["/recipe \"Tsai tou Vounou\"", "/recipe \"Tsai tou Vounou\" <errorif.addonnotfound>"]
)]
public class RecipeCommand(string text, string recipeName) : MacroCommandBase(text)
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

                var recipeId = Game.Crafting.GetRecipeIdByName(recipeName);
                FrameworkLogger.Debug($"Recipe found: {recipeId}");

                Game.Crafting.OpenRecipe(recipeId);
            }
        });

        await PerformWait(token);
    }
}
