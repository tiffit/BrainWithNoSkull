using BrainWithNoSkull.World.Items;

namespace BrainWithNoSkull.World.ItemStacks;

public class ItemStack {

    public Item Item{ get; private set; }
    public int Count;

    public ItemStack(Item item, int count){
        Item = item;
        Count = count;
    }

}