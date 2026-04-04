using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Entities;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbNewEntityPacket : IPacket {

    public EntityType EntityType;
    public Vector3D<float> Position;
    public uint Id;
    public byte[] ExtraData;
    
    public CbNewEntityPacket(){}
    
    public CbNewEntityPacket(Entity entity){
        EntityType = entity.Type;
        Position = entity.Position;
        Id = entity.Id;
        using MemoryStream stream = new MemoryStream();
        using PacketWriter writer = new PacketWriter(stream);
        entity.WriteToPacket(writer);
        if (stream.TryGetBuffer(out ArraySegment<byte> segment)) {
            ExtraData = new byte[segment.Count];
            segment.CopyTo(ExtraData);
        }
    }
    
    public PacketType GetPacketType(){
        return PacketType.NewEntity;
    }

    public void Write(PacketWriter writer){
        writer.Write(EntityType.Index);
        writer.Write(Position);
        writer.Write(Id);
        writer.Write((short)ExtraData.Length);
        writer.Write(ExtraData);
    }

    public void Read(PacketReader reader){
        EntityType = EntityTypeRegistry.Registry.GetEntry(reader.ReadUInt16())!;
        Position = reader.Read3DFloatVec();
        Id = reader.ReadUInt32();
        int extraDatLength = reader.ReadInt16();
        ExtraData = reader.ReadBytes(extraDatLength);
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandleNewEntity(this);
    }
}