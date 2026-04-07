using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.BlockEntities;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbNewBlockEntityPacket : IPacket {

    public BlockEntityType BlockEntityType;
    public Vector3D<int> Position;
    public byte[] ExtraData = null!;
    
    public CbNewBlockEntityPacket(){}
    
    public CbNewBlockEntityPacket(BlockEntity blockEntity){
        BlockEntityType = blockEntity.Type;
        Position = blockEntity.BlockPosition;
        
        using MemoryStream stream = new MemoryStream();
        using PacketWriter writer = new PacketWriter(stream);
        blockEntity.WriteToPacket(writer);
        if (stream.TryGetBuffer(out ArraySegment<byte> segment)) {
            ExtraData = new byte[segment.Count];
            segment.CopyTo(ExtraData);
        }
    }
    
    public PacketType GetPacketType(){
        return PacketType.NewBlockEntity;
    }

    public void Write(PacketWriter writer){
        writer.Write(BlockEntityType.Index);
        writer.Write(Position);
        writer.Write((short)ExtraData.Length);
        writer.Write(ExtraData);
    }

    public void Read(PacketReader reader){
        BlockEntityType = BlockEntityRegistry.Registry.GetEntry(reader.ReadUInt16())!;
        Position = reader.Read3DIntVec();
        int extraDatLength = reader.ReadInt16();
        ExtraData = reader.ReadBytes(extraDatLength);
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandleNewBlockEntity(this);
    }
}