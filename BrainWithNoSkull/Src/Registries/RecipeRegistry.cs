using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.World.ItemStacks;
using BrainWithNoSkull.World.Recipes;

#pragma warning disable CS8618

namespace BrainWithNoSkull.Registries;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class RecipeRegistry {
    public static readonly Registry<Recipe> Registry = new Registry<Recipe>();

    public static Recipe StonePickaxe;
    public static Recipe CrudeFurnace;

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
    }
}