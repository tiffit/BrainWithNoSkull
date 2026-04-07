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
        
        SetItemInSlot(new ItemStack(BlockRegistry.CrudeFurnace, 1), 5);
        SetItemInSlot(new ItemStack(BlockRegistry.Magnetite, 5), 6);
        SetItemInSlot(new ItemStack(ItemRegistry.IronBar, 1), 7);
    }

    public void AddItem(ref ItemStack? stack){
        for (int a = 0; a < 2; a++) {
            for (int i = 0; i < InventorySize; i++) {
                if (a == 1 || !ItemStack.IsNull(GetItem(i))) {
                    if ((this as IItemContainer).CombineStacks(i, ref stack, false)) {
                        DirtySlots.Add(i);
                    }
                    if (ItemStack.IsNull(stack)) {
                        stack = null;
                        return;
                    }
                }
            }
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