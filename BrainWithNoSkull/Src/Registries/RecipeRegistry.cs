using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.World.ItemStacks;
using BrainWithNoSkull.World.Recipes;

#pragma warning disable CS8618

namespace BrainWithNoSkull.Registries;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class RecipeRegistry {
    public static readonly Registry<Recipe> Registry = new Registry<Recipe>();
    public static readonly Dictionary<RecipeType, RecipeLookup> Lookups = new Dictionary<RecipeType, RecipeLookup>();

    public static Recipe StonePickaxe;
    public static Recipe CrudeFurnace;
    
    public static Recipe IronBarSmelt;
    public static Recipe IronScrew;

    public static void Init(){
        StonePickaxe = Registry.Register(new Recipe("StonePickaxe", RecipeType.Crafting,
            [new ItemStack(BlockRegistry.Stick, 3), new ItemStack(BlockRegistry.Pebbles, 5)],
            new ItemStack(ItemRegistry.StonePickaxe, 1),
            5f
        ));
        StonePickaxe = Registry.Register(new Recipe("CrudeFurnace", RecipeType.Crafting,
            [new ItemStack(BlockRegistry.Stone, 10), new ItemStack(BlockRegistry.Stick, 5)],
            new ItemStack(BlockRegistry.CrudeFurnace, 1),
            5f
        ));
        
        IronBarSmelt = Registry.Register(new Recipe("IronBarSmelt", RecipeType.Smelting,
            new ItemStack(BlockRegistry.Magnetite, 1), new ItemStack(ItemRegistry.IronBar, 1), 5f));
        IronScrew = Registry.Register(new Recipe("IronScrew", RecipeType.Crafting,
            new ItemStack(ItemRegistry.IronBar, 1), new ItemStack(ItemRegistry.IronScrew, 2), 1f
        ));

        Lookups[RecipeType.Crafting] = new RecipeLookup();
        Lookups[RecipeType.Smelting] = new RecipeLookup();
        
        foreach (Recipe recipe in Registry.GetEntries()) {
            RecipeLookup lookup = Lookups[recipe.RecipeType];
            lookup.AddRecipe(recipe);
        }
    }
}