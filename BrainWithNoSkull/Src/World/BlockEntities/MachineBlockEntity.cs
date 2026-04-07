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

public class MachineBlockEntity: BlockEntity, ITickingBlockEntity {

    public readonly RecipeType RecipeType;
    public readonly FuelType FuelType;
    public readonly MachineTier Tier;
    public readonly bool AutoDetect;
    
    public MachineBlockEntity(RecipeType recipeType, FuelType fuelType, MachineTier tier, bool autoDetect){
        RecipeType = recipeType;
        FuelType = fuelType;
        Tier = tier;
        AutoDetect = autoDetect;
    }
    
    public void Tick(){
    }
}