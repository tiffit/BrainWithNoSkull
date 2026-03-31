using BrainWithNoSkull.Util;

namespace BrainWithNoSkull.World.Blocks.Specific;

public class PebblesBlock: NonFullBlock {
    private static readonly AABB CollisionBox = new AABB(1/8f, 0, 1/8f, 7/8f, 1/8f, 7/8f);
    
    public PebblesBlock(string id) : base(id){
    }

    public override AABB GetCollisionBox(){
        return CollisionBox;
    }
}