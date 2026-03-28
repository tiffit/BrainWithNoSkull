using BrainWithNoSkull.World.Entities.Player;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Sb;

public class SbPlayerMovePacket : IPacket {

    public Vector3D<float> NewPos;
    
    public SbPlayerMovePacket(){ }
    
    public SbPlayerMovePacket(Vector3D<float> newPos){
        NewPos = newPos;
    }
    
    public PacketType GetPacketType(){
        return PacketType.PlayerMove;
    }
    
    public void Write(PacketWriter writer){
        writer.Write(NewPos);
    }

    public void Read(PacketReader reader){
        NewPos = reader.Read3DFloatVec();
    }

    public void Handle(PacketContext ctx){
        ServerPlayerEntity? player = ctx.Client!.Entity;

        player?.SetPosition(NewPos);
    }
}