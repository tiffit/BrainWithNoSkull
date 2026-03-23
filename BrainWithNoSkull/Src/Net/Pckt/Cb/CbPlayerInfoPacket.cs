using BrainWithNoSkull.Client.World;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbPlayerInfoPacket : IPacket {

    public Vector3D<float> PlayerPos;
    
    public CbPlayerInfoPacket(){}
    
    public CbPlayerInfoPacket(Vector3D<float> playerPos){
        PlayerPos = playerPos;
    }
    
    public PacketType GetPacketType(){
        return PacketType.PlayerInfo;
    }

    public void Write(PacketWriter writer){
        writer.Write(PlayerPos);
    }

    public void Read(PacketReader reader){
        PlayerPos = reader.Read3DFloatVec();
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandlePlayerInfo(this);
    }
}