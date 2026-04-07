using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.BlockEntities;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Blocks;

public class Block : IRegistryEntry {

    private static readonly AABB CollisionBox = new AABB(0, 0, 0, 1, 1, 1);
    
    public static ushort NextIndex;
    private readonly ushort _index;
    private readonly string _id;
    public readonly BlockConfigContainer ConfigContainer = new BlockConfigContainer();
    
    //Properties
    public float BreakTime = 0f;
    
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

public interface IMultiblock {
    public Vector3D<int> GetMultiblockSize();
}

public interface IBlockEntityProvider {
    public BlockEntity GetBlockEntity();
}