using BrainWithNoSkull.Registries;

namespace BrainWithNoSkull.Blocks;

public class Block : IRegistryEntry {

    private readonly string _id;
    
    public Block(string id){
        _id = id;
    }
    
    public string GetId(){
        return _id;
    }

    public bool IsAir(){
        return this == BlockRegistry.Air;
    }
}