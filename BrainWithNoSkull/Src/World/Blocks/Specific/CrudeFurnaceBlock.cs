using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Blocks.Specific;

public class CrudeFurnaceBlock: NonFullBlock, IMultiblock {
    
    private static readonly AABB CollisionBox = new AABB(1/8f, 0, 1/8f, 7/8f, 1 + 6/8f, 7/8f);
    
    public CrudeFurnaceBlock(string id) : base(id){
        ConfigContainer.With(MultiblockBlockConfigType.Instance);
    }

    public Vector3D<int> GetMultiblockSize(){
        return new Vector3D<int>(1, 2, 1);
    }

    public override AABB GetCollisionBox(){
        return CollisionBox;
    }
}