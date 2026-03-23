using BrainWithNoSkull.Registries;

namespace BrainWithNoSkull.World.Blocks;

public class Block : IRegistryEntry {
    
    public static ushort NextIndex;
    private readonly ushort _index;
    private readonly string _id;
    
    public Block(string id){
        _id = id;
        _index = NextIndex++;
    }
    
    public string GetId(){
        return _id;
    }

    public ushort GetIndex(){
        return _index;
    }

    public bool IsAir(){
        return this == BlockRegistry.Air;
    }
}