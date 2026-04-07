using BrainWithNoSkull.Net;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;
using BrainWithNoSkull.World.Recipes;

namespace BrainWithNoSkull.World.BlockEntities;

public enum FuelType {
    None,
    Solid,
    Electricity
}

public enum MachineTier : byte {
    None,
    Crude
}

public class MachineContainerSync: ContainerSync {
    
    public MachineContainerSync(uint id, IItemContainer container, PlayerEntity player) : base(id, container, player){
    }
    
}

public class MachineContainer: IItemContainer {

    public List<ItemStack?> Inputs = [];
    public List<ItemStack?> Outputs = [];
    
    public ItemStack? GetItem(int slot){
        return slot >= Inputs.Count ? Outputs[slot - Inputs.Count] : Inputs[slot];
    }

    public int GetInventorySize(){
        return Inputs.Count + Outputs.Count;
    }

    public void SetItemInSlot(ItemStack? stack, int slot){
        if (slot >= Inputs.Count) {
            Outputs[slot - Inputs.Count] = stack;
        }
        else {
            Inputs[slot] = stack;
        }
    }

    public void MarkSlotDirty(int slot){
        
    }
    
}

public class MachineBlockEntity: BlockEntity, ITickingBlockEntity {

    public readonly RecipeType RecipeType;
    public readonly FuelType FuelType;
    public readonly MachineTier Tier;
    public readonly bool AutoDetect;

    public int AutoInputSize = 1;
    public int AutoOutputSize = 1;

    public readonly MachineContainer Container = new MachineContainer();
    
    public MachineBlockEntity(BlockEntityType type, RecipeType recipeType, FuelType fuelType, MachineTier tier, bool autoDetect) : base(type){
        RecipeType = recipeType;
        FuelType = fuelType;
        Tier = tier;
        AutoDetect = autoDetect;

        if (autoDetect) {
            Container.Inputs.Clear();
            Container.Outputs.Clear();
            for (int i = 0; i < AutoInputSize; i++) Container.Inputs.Add(null);
            for (int i = 0; i < AutoOutputSize; i++) Container.Outputs.Add(null);
        }
    }
    
    public void Tick(){
    }
}