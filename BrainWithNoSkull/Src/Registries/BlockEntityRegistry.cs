using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.World.BlockEntities;
using BrainWithNoSkull.World.Recipes;

#pragma warning disable CS8618

namespace BrainWithNoSkull.Registries;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public class BlockEntityRegistry {
    public static readonly Registry<BlockEntityType> Registry = new Registry<BlockEntityType>();

    public static BlockEntityType CrudeFurnace;

    public static void Init(){
        CrudeFurnace = Registry.Register(new BlockEntityTypeBuilder(typeof(MachineBlockEntity), "CrudeFurnace",
                type => new MachineBlockEntity(type, RecipeType.Smelting, FuelType.Solid, MachineTier.Crude, true))
            .Build());
    }
}