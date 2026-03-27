using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.World.Blocks;

#pragma warning disable CS8618

namespace BrainWithNoSkull.Registries;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class BlockRegistry {

    public static readonly Registry<Block> Registry = new Registry<Block>();

    public static Block Air;
    public static Block Grass;
    public static Block Dirt;
    public static Block Stone;
    
    public static void Init(){
        Air = Registry.Register(new Block("Air"));
        
        Grass = Registry.Register(new Block("Grass"));
        Dirt = Registry.Register(new Block("Dirt"));
        Stone = Registry.Register(new Block("Stone"));
    }

}