using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.Items;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World.Recipes;


public enum RecipeType {
    Crafting,
    Smelting
}

public class Recipe: IRegistryEntry {
    private static ushort _nextIndex;
    private readonly ushort _index;
    private readonly string _id;

    public readonly RecipeType RecipeType;
    public readonly IReadOnlyList<ItemStack> Inputs;
    public readonly IReadOnlyList<ItemStack> Outputs;
    public readonly float Time;
    
    public Recipe(string id, RecipeType type, List<ItemStack> inputs, List<ItemStack> outputs, float time){
        _id = id;
        _index = _nextIndex++;

        RecipeType = type;
        Inputs = inputs.AsReadOnly();
        Outputs = outputs.AsReadOnly();
        Time = time;
    }
    
    public Recipe(string id, RecipeType type, List<ItemStack> inputs, ItemStack output, float time): this(id, type, inputs, [output], time){}
    public Recipe(string id, RecipeType type, ItemStack input, ItemStack output, float time): this(id, type, [input], [output], time){}
    
    public string GetId(){
        return _id;
    }

    public ushort GetIndex(){
        return _index;
    }

    public bool CanCraft(PlayerInventory inv){
        Dictionary<Item, int> counts = inv.GetCounts();
        return Inputs.All(stack => {
            counts.TryGetValue(stack.Item, out int count);
            return stack.Count <= count;
        });
    }
}