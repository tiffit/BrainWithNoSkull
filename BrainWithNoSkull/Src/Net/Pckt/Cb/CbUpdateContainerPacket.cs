using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbUpdateContainerPacket : IPacket {

    public uint Id;
    public List<Tuple<int, ItemStack?>> UpdatedItems = [];
    public byte[] SyncVariables = [];
    
    public CbUpdateContainerPacket(){}
    
    public CbUpdateContainerPacket(uint id, byte[] syncVariables, List<Tuple<int, ItemStack?>> updatedItems){
        Id = id;
        SyncVariables = syncVariables;
        UpdatedItems = updatedItems;
    }
    
    public PacketType GetPacketType(){
        return PacketType.UpdateContainer;
    }

    public void Write(PacketWriter writer){
        writer.Write(Id);
        writer.Write((ushort)SyncVariables.Length);
        writer.Write(SyncVariables);
        writer.Write((ushort)UpdatedItems.Count);
        foreach ((int slot, ItemStack? stack) in UpdatedItems) {
            writer.Write((ushort)slot);
            writer.Write(stack);
        }
    }

    public void Read(PacketReader reader){
        Id = reader.ReadUInt32();
        ushort variableSize = reader.ReadUInt16();
        SyncVariables = reader.ReadBytes(variableSize);
        ushort itemCount = reader.ReadUInt16();
        UpdatedItems = new List<Tuple<int, ItemStack?>>(itemCount);
        for (int i = 0; i < itemCount; i++) {
            int slot = reader.ReadUInt16();
            ItemStack? stack = reader.ReadItemStack();
            UpdatedItems.Add(new Tuple<int, ItemStack?>(slot, stack));
        }
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandleUpdateContainer(this);
    }
}