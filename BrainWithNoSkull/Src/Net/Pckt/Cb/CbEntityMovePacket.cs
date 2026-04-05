using BrainWithNoSkull.World.Entities;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbEntityMovePacket : IPacket {

    public uint Id;
    public Vector3D<float> Position;
    
    public CbEntityMovePacket(){}
    
    public CbEntityMovePacket(Entity entity){
        Id = entity.Id;
        Position = entity.Position;
    }
    
    public PacketType GetPacketType(){
        return PacketType.EntityMove;
    }

    public void Write(PacketWriter writer){
        writer.Write(Position);
        writer.Write(Id);
    }

    public void Read(PacketReader reader){
        Position = reader.Read3DFloatVec();
        Id = reader.ReadUInt32();
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandleEntityMove(this);
    }
}