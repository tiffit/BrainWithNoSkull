using BrainWithNoSkull.Util;
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
        AABB? aabb = Block.GetCollisionBox();
        if (aabb == null || !world.GetEntitiesInBox(aabb + placePos).Any()) {
            world.PlaceBlock(placePos, Block);
            stack.Count -= 1;
            if (world.OnServer) {
                player.Inventory.MarkHeldStackDirty();
            }
        }
        else {
            if (world.OnServer) {
                //TODO: Send block on server to make sure client is synced up properly
            }
        }
    }
}