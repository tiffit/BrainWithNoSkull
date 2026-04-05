namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbRemoveEntityPacket : IPacket {

    public uint Id;
    
    public CbRemoveEntityPacket(){}
    
    public CbRemoveEntityPacket(uint id){
        Id = id;
    }
    
    public PacketType GetPacketType(){
        return PacketType.RemoveEntity;
    }

    public void Write(PacketWriter writer){
        writer.Write(Id);
    }

    public void Read(PacketReader reader){
        Id = reader.ReadUInt32();
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandleRemoveEntity(this);
    }
}