using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.Net.Pckt.Cb;

public class CbPlayerInventoryPacket : IPacket {

    public List<Tuple<short, ItemStack?>> Slots = [];
    
    public CbPlayerInventoryPacket(){}
    
    public CbPlayerInventoryPacket(PlayerInventory inventory){
        Slots = new List<Tuple<short, ItemStack?>>(inventory.DirtySlots.Count);
        foreach (int slot in inventory.DirtySlots) {
            Slots.Add(new Tuple<short, ItemStack?>((short)slot, inventory.GetItem(slot)));
        }
    }
    
    public PacketType GetPacketType(){
        return PacketType.PlayerInventory;
    }

    public void Write(PacketWriter writer){
        writer.Write((short)Slots.Count);
        foreach ((short slot, ItemStack? stack) in Slots) {
            writer.Write(slot);
            writer.Write(stack);
        }
    }

    public void Read(PacketReader reader){
        int count = reader.ReadInt16();
        Slots = new List<Tuple<short, ItemStack?>>(count);
        for (int i = 0; i < count; i++) {
            short slot = reader.ReadInt16();
            ItemStack? stack = reader.ReadItemStack();
            Slots.Add(new Tuple<short, ItemStack?>(slot, stack));
        }
    }

    public void Handle(PacketContext ctx){
        ctx.ClientPacketHandler!.HandlePlayerInventory(this);
    }
}