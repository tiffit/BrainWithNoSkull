using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Entities;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Sb;

public class SbPlayerBreakBlockPacket : IPacket {

    public Vector3D<int> BlockPos;
    
    public SbPlayerBreakBlockPacket(){ }
    
    public SbPlayerBreakBlockPacket(Vector3D<int> blockPos){
        BlockPos = blockPos;
    }
    
    public PacketType GetPacketType(){
        return PacketType.BreakBlock;
    }
    
    public void Write(PacketWriter writer){
        writer.Write(BlockPos);
    }

    public void Read(PacketReader reader){
        BlockPos = reader.Read3DIntVec();
    }

    public void Handle(PacketContext ctx){
        ServerPlayerEntity? player = ctx.Client!.Entity;

        if (player?.World != null) {
            Block currentBlock = player.World.GetBlock(BlockPos);
            ItemStack stack = new ItemStack(ItemRegistry.BlockItems[currentBlock], 1);
            player.World.RemoveBlock(BlockPos);

            if (!ItemStack.IsNull(stack)) {
                ItemStackEntity entity = new ItemStackEntity(player.World){Stack = stack};
                entity.AddVelocity(new Vector3D<float>(0, 4f, 0));
                player.World.AddEntity(entity, new Vector3D<float>(BlockPos.X + 0.5f, BlockPos.Y + 0.5f, BlockPos.Z + 0.5f));
            }
        }
    }
}