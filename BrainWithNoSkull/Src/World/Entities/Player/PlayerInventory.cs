using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World.Entities.Player;

public class PlayerInventory {

    public int InventorySize = 27;

    public List<ItemStack> Items;

    public PlayerInventory(){
        Items = new List<ItemStack>(InventorySize);
    }

}