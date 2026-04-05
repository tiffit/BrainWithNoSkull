using BrainWithNoSkull.Net;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World.Entities;

public class ItemStackEntity: Entity {

    public ItemStack? Stack;
    
    public ItemStackEntity(WorldBase? world) : base(EntityTypeRegistry.ItemStack, world){
    }

    public override void Tick(){
        base.Tick();
        if (ItemStack.IsNull(Stack)) {
            Remove = true;
        }

        if (World != null) {
            if (!Remove && World.OnServer) {
                foreach (Entity entity in World.GetEntitiesInBox(GetBoundingBox())) {
                    if (entity is PlayerEntity player) {
                        player.Inventory.AddItem(ref Stack);
                        if (ItemStack.IsNull(Stack)) {
                            Remove = true;
                            return;
                        }
                    }
                }
            }
        }
    }

    public override void WriteToPacket(PacketWriter writer){
        base.WriteToPacket(writer);
        writer.Write(Stack);
    }

    public override void ReadFromPacket(PacketReader reader){
        base.ReadFromPacket(reader);
        Stack = reader.ReadItemStack();
    }
}