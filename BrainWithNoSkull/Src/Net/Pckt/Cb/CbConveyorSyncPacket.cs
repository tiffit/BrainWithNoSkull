using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public record ConveyorSyncEntry(int Location, ItemStack? Stack, float Progress){
    
}

public class CbConveyorSyncPacket : IPacket {

    public Vector2D<int> ChunkCoords;
    public List<ConveyorSyncEntry> Entries = null!;
    
    public CbConveyorSyncPacket(){}
    
    public CbConveyorSyncPacket(Vector2D<int> chunkCoords, List<ConveyorSyncEntry> entries){
        ChunkCoords = chunkCoords;
        Entries = entries;
    }
    
    public PacketType GetPacketType(){
        return PacketType.ConveyorSync;
    }

    public void Write(PacketWriter writer){
        writer.Write(ChunkCoords);
        writer.Write((short)Entries.Count);
        foreach (ConveyorSyncEntry entry in Entries) {
            writer.Write(entry.Location);
            writer.Write(entry.Stack);
            writer.Write(entry.Progress);
        }
    }

    public void Read(PacketReader reader){
        ChunkCoords = reader.Read2DIntVec();
        short count = reader.ReadInt16();
        Entries = new List<ConveyorSyncEntry>(count);
        for (int i = 0; i < count; i++) {
            ConveyorSyncEntry entry = new ConveyorSyncEntry(reader.ReadInt32(), reader.ReadItemStack(), reader.ReadSingle());
            Entries.Add(entry);
        }
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandleConveyorSync(this);
    }
}