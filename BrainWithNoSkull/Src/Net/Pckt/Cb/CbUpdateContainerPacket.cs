namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbUpdateContainerPacket : IPacket {

    public uint Id;
    public byte[] SyncVariables = [];
    
    public CbUpdateContainerPacket(){}
    
    public CbUpdateContainerPacket(uint id, byte[] syncVariables){
        Id = id;
        SyncVariables = syncVariables;
    }
    
    public PacketType GetPacketType(){
        return PacketType.UpdateContainer;
    }

    public void Write(PacketWriter writer){
        writer.Write(Id);
        writer.Write((ushort)SyncVariables.Length);
        writer.Write(SyncVariables);
    }

    public void Read(PacketReader reader){
        Id = reader.ReadUInt32();
        ushort size = reader.ReadUInt16();
        SyncVariables = reader.ReadBytes(size);
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandleUpdateContainer(this);
    }
}