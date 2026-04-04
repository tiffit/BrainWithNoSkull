using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World.Entities;

public class ItemStackEntity: Entity {

    public ItemStack? Stack;
    
    public ItemStackEntity(WorldBase? world) : base(EntityTypeRegistry.ItemStack, world){
    }

    public void SetItem(ItemStack? stack){
        Stack = stack;
    }

    public override void Tick(){
        base.Tick();
        if (ItemStack.IsNull(Stack)) {
            Remove = true;
        }
    }
}