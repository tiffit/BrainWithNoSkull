using BrainWithNoSkull.World.Blocks;

namespace BrainWithNoSkull.World.Items;

public class BlockItem : Item {

    public readonly Block Block;
    
    public BlockItem(Block block) : base(block.GetId()){
        Block = block;
    }
    
}