using BrainWithNoSkull.World.Items;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World;

public interface IItemContainer {

    public ItemStack? GetItem(int slot);

    public int GetInventorySize();

    public void SetItemInSlot(ItemStack? stack, int slot);

    public void MarkSlotDirty(int slot);

    public bool CombineStacks(int slot, ref ItemStack? other, bool swap){
        if (ItemStack.IsNull(other))
            return false;
        
        ItemStack? current = GetItem(slot);
        if (ItemStack.CanCombine(current, other)) {
            if (!ItemStack.IsNull(current)) {
                current.Count += other.Count;
                other = null;
            }
            else {
                SetItemInSlot(other.Clone(), slot);
                other = null;
            }
            return true;
        }

        if (swap) {
            SetItemInSlot(other.Clone(), slot);
            other = current;
        }
        return true;
    }
    
    public Dictionary<Item, int> GetCounts(){
        Dictionary<Item, int> counts = new Dictionary<Item, int>();
        for (int i = 0; i < GetInventorySize(); i++) {
            ItemStack? stack = GetItem(i);
            if (!ItemStack.IsNull(stack)) {
                counts.TryGetValue(stack.Item, out int current);
                counts[stack.Item] = current + stack.Count;
            }
        }
        return counts;
    }

    public void Remove(Item item, int amount){
        for (int i = 0; i < GetInventorySize(); i++) {
            ItemStack? stack = GetItem(i);
            if (!ItemStack.IsNull(stack)) {
                if (stack.Item == item) {
                    int toRemove = Math.Min(amount, stack.Count);
                    amount -= toRemove;
                    stack.Count -= toRemove;
                    MarkSlotDirty(i);
                    if(amount == 0) return;
                }
            }
        }
    }
}