using BrainWithNoSkull.Net;
using BrainWithNoSkull.Registries;
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