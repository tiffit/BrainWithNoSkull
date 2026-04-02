using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World;

public interface IItemContainer {

    public ItemStack? GetItem(int slot);

    public int GetInventorySize();
}