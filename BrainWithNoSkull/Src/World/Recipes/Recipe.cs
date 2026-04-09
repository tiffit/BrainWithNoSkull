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

    public bool CanCraft(IItemContainer inv){
        Dictionary<Item, int> counts = inv.GetCounts();
        return Inputs.All(stack => {
            counts.TryGetValue(stack.Item, out int count);
            return stack.Count <= count;
        });
    }

    public bool CanOutput(IItemContainer inv){
        HashSet<int> usedSlots = [];
        foreach (ItemStack o in Outputs) {
            ItemStack? remaining = o.Clone();
            for (int i = 0; i < inv.GetInventorySize(); i++) {
                if(usedSlots.Contains(i))
                    continue;
                if (inv.CombineStacks(i, ref remaining, false, true)) {
                    usedSlots.Add(i);
                }
            }

            if (!ItemStack.IsNull(remaining)) {
                return false;
            }
        }

        return true;
    }

    public void ConsumeIngredients(IItemContainer container){
        foreach (ItemStack input in Inputs) {
            container.Remove(input.Item, input.Count);
        }
    }

    public void ProduceOutput(IItemContainer container){
        foreach (ItemStack o in Outputs) {
            ItemStack? remaining = o.Clone();
            container.AddItem(ref remaining);
        }
    }
}