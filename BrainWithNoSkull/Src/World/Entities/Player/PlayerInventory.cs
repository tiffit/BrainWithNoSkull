using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World.Entities.Player;

public class PlayerInventory {

    public int InventorySize = 27;
    public int HotbarSlot = 0;
    public List<ItemStack?> Items;

    public PlayerInventory(){
        Items = new List<ItemStack?>(InventorySize);
        for (int i = 0; i < InventorySize; i++) {
            Items.Add(null);
        }

        Items[0] = new ItemStack(ItemRegistry.BlockItems[BlockRegistry.Grass], 1);
    }

}