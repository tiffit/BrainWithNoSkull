using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbPlayerInfoPacket : IPacket {

    public Vector3D<float> PlayerPos;
    public uint Id;
    
    public CbPlayerInfoPacket(){}
    
    public CbPlayerInfoPacket(Vector3D<float> playerPos, uint id){
        PlayerPos = playerPos;
        id = id;
    }
    
    public PacketType GetPacketType(){
        return PacketType.PlayerInfo;
    }

    public void Write(PacketWriter writer){
        writer.Write(PlayerPos);
        writer.Write(Id);
    }

    public void Read(PacketReader reader){
        PlayerPos = reader.Read3DFloatVec();
        Id = reader.ReadUInt32();
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandlePlayerInfo(this);
    }
}