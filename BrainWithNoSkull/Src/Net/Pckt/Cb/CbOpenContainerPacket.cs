using BrainWithNoSkull.Registries;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbOpenContainerPacket : IPacket {

    public uint Id;
    public int Size;
    public ContainerSyncType Type = null!;
    
    public CbOpenContainerPacket(){}
    
    public CbOpenContainerPacket(uint id, int size, ContainerSyncType type){
        Id = id;
        Size = size;
        Type = type;
    }
    
    public PacketType GetPacketType(){
        return PacketType.OpenContainer;
    }

    public void Write(PacketWriter writer){
        writer.Write(Id);
        writer.Write(Size);
        writer.Write(Type.Index);
    }

    public void Read(PacketReader reader){
        Id = reader.ReadUInt32();
        Size = reader.ReadInt32();
        Type = ContainerSyncRegistry.Registry.GetEntry(reader.ReadUInt16())!;
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandleOpenContainer(this);
    }
}