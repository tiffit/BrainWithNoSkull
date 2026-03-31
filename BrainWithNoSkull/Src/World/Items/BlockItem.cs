using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Items;

public class BlockItem : Item {

    public readonly Block Block;
    
    public BlockItem(Block block) : base(block.GetId()){
        Block = block;
    }

    public override void Interact(WorldRayResult result, WorldBase world, PlayerEntity player, ItemStack stack){
        Vector3D<int> placePos = result.Pos + result.Direction.Offset;
        world.SetBlock(placePos, Block);
        stack.Count -= 1;
        if (world.OnServer) {
            player.Inventory.MarkHeldStackDirty();
        }
    }
}