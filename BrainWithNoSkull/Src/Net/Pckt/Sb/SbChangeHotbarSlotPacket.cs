using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.Net.Pckt.Sb;

public class SbChangeHotbarSlotPacket : IPacket {

    public int Slot;
    
    public SbChangeHotbarSlotPacket(){ }
    
    public SbChangeHotbarSlotPacket(int slot){
        Slot = slot;
    }
    
    public PacketType GetPacketType(){
        return PacketType.ChangeHotbarSlot;
    }
    
    public void Write(PacketWriter writer){
        writer.Write((byte)Slot);
    }

    public void Read(PacketReader reader){
        Slot = reader.ReadByte();
    }

    public void Handle(PacketContext ctx){
        ServerPlayerEntity? player = ctx.Client!.Entity;
        if (player != null) 
            player.Inventory.HotbarSlot = Slot;
    }
}