using BrainWithNoSkull.World.Items;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World.Recipes;

public class RecipeLookup {
    private readonly Dictionary<Item, List<Recipe>> _recipeInputCache = new Dictionary<Item, List<Recipe>>();

    public void AddRecipe(Recipe recipe){
        foreach (ItemStack input in recipe.Inputs) {
            if (_recipeInputCache.TryGetValue(input.Item, out List<Recipe>? list)) {
                list.Add(recipe);
            }
            else {
                _recipeInputCache[input.Item] = [recipe];
            }
        }
    }

    public Recipe? Lookup(IItemContainer container){
        for (int i = 0; i < container.GetInventorySize(); i++) {
            ItemStack? stack = container.GetItem(i);
            if (!ItemStack.IsNull(stack)) {
                if (_recipeInputCache.TryGetValue(stack.Item, out List<Recipe>? list)) {
                    foreach (Recipe recipe in list) {
                        if (recipe.CanCraft(container)) {
                            return recipe;
                        }
                    }
                }
            }
        }
        return null;
    }
}