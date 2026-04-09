using BrainWithNoSkull.World.Items;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World;

public interface IItemContainer {

    public ItemStack? GetItem(int slot);

    public int GetInventorySize();

    public void SetItemInSlot(ItemStack? stack, int slot);

    public void MarkSlotDirty(int slot);

    public void AddItem(ref ItemStack? stack){
        for (int a = 0; a < 2; a++) {
            for (int i = 0; i < GetInventorySize(); i++) {
                if (a == 1 || !ItemStack.IsNull(GetItem(i))) {
                    if (CombineStacks(i, ref stack, false)) {
                        MarkSlotDirty(i);
                    }
                    if (ItemStack.IsNull(stack)) {
                        stack = null;
                        return;
                    }
                }
            }
        }
    }

    public bool CombineStacks(int slot, ref ItemStack? other, bool swap, bool simulate = false){
        if (ItemStack.IsNull(other))
            return false;
        
        ItemStack? current = GetItem(slot);
        if (simulate && current != null) {
            current = current.Clone();
        }
        if (ItemStack.CanCombine(current, other)) {
            if (!ItemStack.IsNull(current)) {
                current.Count += other.Count;
                if (!simulate) {
                    MarkSlotDirty(slot);
                }
                other = null;
            }
            else {
                if(!simulate) SetItemInSlot(other.Clone(), slot);
                other = null;
            }
            return true;
        }

        if (swap) {
            if(!simulate) SetItemInSlot(other.Clone(), slot);
            other = current;
            return true;
        }
        return false;
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

    public uint GetContainerId(){
        return 0;
    }
}

public class DummyItemContainer : IItemContainer {

    private readonly ItemStack?[] _contents;
    private readonly uint _id;
    
    public DummyItemContainer(uint id, int size){
        _contents = new ItemStack[size];
        _id = id;
    }
    
    public ItemStack? GetItem(int slot){
        return _contents[slot];
    }

    public int GetInventorySize(){
        return _contents.Length;
    }

    public void SetItemInSlot(ItemStack? stack, int slot){
        _contents[slot] = stack;
    }

    public void MarkSlotDirty(int slot){}

    public uint GetContainerId(){
        return _id;
    }
}

public class ListItemContainer : IItemContainer {

    private readonly List<ItemStack?> _list;
    private readonly int _offset;
    private readonly IItemContainer _parent;
    
    public ListItemContainer(IItemContainer parent, int offset, List<ItemStack?> list){
        _parent = parent;
        _offset = offset;
        _list = list;
    }
    
    public ItemStack? GetItem(int slot){
        return _list[slot];
    }

    public int GetInventorySize(){
        return _list.Count;
    }

    public void SetItemInSlot(ItemStack? stack, int slot){
        _list[slot] = stack;
    }

    public void MarkSlotDirty(int slot){
        _parent.MarkSlotDirty(_offset + slot);
    }

    public uint GetContainerId(){
        return _parent.GetContainerId();
    }
}