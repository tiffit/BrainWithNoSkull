using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks.BlockConfigs;

namespace BrainWithNoSkull.World.Blocks.Specific;

public class LitterBlock: NonFullBlock {
    private static readonly AABB CollisionBox = new AABB(0, 0, 0, 1, 2/8f, 1);
    
    public LitterBlock(string id) : base(id){
        ConfigContainer.With(HorizontalFacingBlockConfigType.Instance);
    }

    public override AABB GetCollisionBox(){
        return CollisionBox;
    }
}