using BrainWithNoSkull.Util;

namespace BrainWithNoSkull.World.Blocks;

public class NonFullBlock: Block {
    public NonFullBlock(string id) : base(id){
    }

    public override bool ShouldCullFace(Direction direction){
        return false;
    }
}