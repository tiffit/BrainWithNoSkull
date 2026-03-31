using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World.Entities.Player;

public class PlayerInventory {

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

    public void AddItem(ItemStack stack){
        for (int i = 0; i < InventorySize; i++) {
            AddItemToSlot(stack, i);
            if (stack.Count == 0) {
                return;
            }
        }
    }

    public void AddItemToSlot(ItemStack stack, int slot){
        ItemStack? current = Items[slot];
        if (ItemStack.IsNull(current)) {
            Items[slot] = stack.Clone();
            stack.Count = 0;
            DirtySlots.Add(slot);
            return;
        }

        if (stack.Item == current.Item) {
            current.Count += stack.Count;
            stack.Count = 0;
            DirtySlots.Add(slot);
        }
    }

    public ItemStack? GetHeldStack(){
        return GetItem(HotbarSlot);
    }

    public void SetItemInSlot(ItemStack? stack, int slot){
        Items[slot] = stack;
        DirtySlots.Add(slot);
    }

    public void MarkHeldStackDirty(){
        DirtySlots.Add(HotbarSlot);
    }

    public ItemStack? GetItem(int slot){
        return Items[slot];
    }

}