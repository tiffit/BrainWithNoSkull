using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Items;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World.Entities.Player;

public class PlayerInventory: IItemContainer {

    public int InventorySize = 27;
    public int HotbarSlot = 0;
    public List<ItemStack?> Items;
    public HashSet<int> DirtySlots = [];

    public PlayerInventory(){
        Items = new List<ItemStack?>(InventorySize);
        for (int i = 0; i < InventorySize; i++) {
            Items.Add(null);
        }
    }

    public ItemStack? GetHeldStack(){
        return GetItem(HotbarSlot);
    }

    public void SetItemInSlot(ItemStack? stack, int slot){
        Items[slot] = stack;
        DirtySlots.Add(slot);
    }

    public void MarkSlotDirty(int slot){
        DirtySlots.Add(slot);
    }

    public void MarkHeldStackDirty(){
        DirtySlots.Add(HotbarSlot);
    }

    public ItemStack? GetItem(int slot){
        return Items[slot];
    }

    public int GetInventorySize(){
        return InventorySize;
    }
}