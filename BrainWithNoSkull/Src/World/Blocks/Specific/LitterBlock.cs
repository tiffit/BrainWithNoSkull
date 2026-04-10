using BrainWithNoSkull.Server;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Blocks.Specific;

public class LitterBlock: NonFullBlock {
    private static readonly AABB CollisionBox = new AABB(0, 0, 0, 1, 2/8f, 1);
    
    public LitterBlock(string id) : base(id){
        ConfigContainer.With(HorizontalFacingBlockConfigType.Instance);
    }

    public override AABB GetCollisionBox(){
        return CollisionBox;
    }

    public override void BlockUpdate(ServerWorld world, Vector3D<int> pos, uint config, Vector3D<int> updated){
        base.BlockUpdate(world, pos, config, updated);
        if (!world.GetBlock(pos - Vector3D<int>.UnitY).IsFullBlock()) {
            world.RemoveBlock(pos);
        }
    }
}