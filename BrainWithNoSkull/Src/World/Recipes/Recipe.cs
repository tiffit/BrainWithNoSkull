using BrainWithNoSkull.Registries;
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
    public readonly IReadOnlyCollection<ItemStack> Inputs;
    public readonly IReadOnlyCollection<ItemStack> Outputs;
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
}