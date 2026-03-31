using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;

namespace BrainWithNoSkull.World.Blocks;

public class Block : IRegistryEntry {

    private static readonly AABB CollisionBox = new AABB(0, 0, 0, 1, 1, 1);
    
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

    public virtual AABB? GetCollisionBox(){
        return IsAir() ? null : CollisionBox;
    }

    public override string ToString(){
        return _id;
    }

    public virtual bool ShouldCullFace(Direction direction){
        return !IsAir();
    }
}