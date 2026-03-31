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
    public static Block Pebbles;
    public static Block Stick;
    
    public static void Init(){
        Air = Registry.Register(new Block("Air"));
        
        Grass = Registry.Register(new Block("Grass"));
        Dirt = Registry.Register(new Block("Dirt"));
        Stone = Registry.Register(new Block("Stone"));
        OakLog = Registry.Register(new Block("OakLog"));
        OakLeaves = Registry.Register(new Block("OakLeaves"));
        Pebbles = Registry.Register(new LitterBlock("Pebbles"));
        Stick = Registry.Register(new LitterBlock("Stick"));
    }

}