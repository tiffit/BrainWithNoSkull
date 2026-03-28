using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Entities.Player;
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

        player?.World?.SetBlock(BlockPos, BlockRegistry.Air);
    }
}