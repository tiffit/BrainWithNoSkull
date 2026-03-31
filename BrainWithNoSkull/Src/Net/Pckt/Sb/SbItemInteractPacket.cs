using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.Net.Pckt.Sb;

public class SbItemInteractPacket : IPacket {

    public WorldRayResult Ray;
    
    public SbItemInteractPacket(){ }
    
    public SbItemInteractPacket(WorldRayResult ray){
        Ray = ray;
    }
    
    public PacketType GetPacketType(){
        return PacketType.ItemInteract;
    }
    
    public void Write(PacketWriter writer){
        writer.Write(Ray);
    }

    public void Read(PacketReader reader){
        Ray = reader.ReadWorldRayResult();
    }

    public void Handle(PacketContext ctx){
        ServerPlayerEntity? player = ctx.Client!.Entity;
        if (player?.World != null) {
            ItemStack? stack = player.Inventory.GetHeldStack();
            if (!ItemStack.IsNull(stack)) {
                stack.Item.Interact(Ray, player.World, player, stack);
            }
        }

    }
}