using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.World.Blocks;

#pragma warning disable CS8618

namespace BrainWithNoSkull.Registries;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class BlockRegistry {

    public static readonly Registry<Block> Registry = new Registry<Block>();

    public static Block Air;
    public static Block Tile;
    
    public static void Init(){
        Tile = Registry.Register(new Block("tile"));
        Air = Registry.Register(new Block("air"));
    }

}