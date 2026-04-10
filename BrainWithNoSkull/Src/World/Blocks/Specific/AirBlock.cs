using BrainWithNoSkull.Util;

namespace BrainWithNoSkull.World.Blocks.Specific;

public class AirBlock: Block {
    
    public AirBlock(string id) : base(id){
    }

    public override bool IsFullBlock(){
        return false;
    }

    public override bool ShouldCullFace(Direction direction){
        return false;
    }
}