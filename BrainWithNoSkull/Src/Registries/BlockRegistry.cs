using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Blocks.Specific;

#pragma warning disable CS8618

namespace BrainWithNoSkull.Registries;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class BlockRegistry {

    public static readonly Registry<Block> Registry = new Registry<Block>();

    public static Block Air;
    public static Block Grass;
    public static Block Dirt;
    public static Block Stone;
    public static Block OakLog;
    public static Block OakLeaves;
    public static LitterBlock Pebbles;
    public static LitterBlock Stick;
    public static Block Magnetite;
    public static CrudeFurnaceBlock CrudeFurnace;
    public static ConveyorBlock Conveyor;
    
    public static void Init(){
        Air = Registry.Register(new AirBlock("Air"));
        
        Grass = Registry.Register(new Block("Grass"){BreakTime = 1f});
        Dirt = Registry.Register(new Block("Dirt"){BreakTime = 1f});
        Stone = Registry.Register(new Block("Stone"){BreakTime = 3f});
        OakLog = Registry.Register(new Block("OakLog"){BreakTime = 2f});
        OakLeaves = Registry.Register(new Block("OakLeaves"){BreakTime = 0.5f});
        Pebbles = Registry.Register(new LitterBlock("Pebbles"){BreakTime = 0.1f});
        Stick = Registry.Register(new LitterBlock("Stick"){BreakTime = 0.1f});
        Magnetite = Registry.Register(new Block("Magnetite"){BreakTime = 4.5f});
        CrudeFurnace = Registry.Register(new CrudeFurnaceBlock("CrudeFurnace"){BreakTime = 4.5f});
        Conveyor = Registry.Register(new ConveyorBlock("Conveyor"){BreakTime = 1.0f});
    }

}