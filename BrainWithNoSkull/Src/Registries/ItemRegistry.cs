using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Items;

#pragma warning disable CS8618

namespace BrainWithNoSkull.Registries;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class ItemRegistry {

    public static readonly Registry<Item> Registry = new Registry<Item>();

    public static readonly Dictionary<Block, BlockItem> BlockItems = new Dictionary<Block, BlockItem>();

    public static Item StonePickaxe;
    
    public static Item IronBar;
    public static Item IronScrew;
    
    public static Item SteelBar;
    
    public static void Init(){
        foreach (Block block in BlockRegistry.Registry.GetEntries()) {
            BlockItems[block] = Registry.Register(new BlockItem(block));
        }

        StonePickaxe = Registry.Register(new Item("StonePickaxe"){ BreakSpeed = 5f });
        
        IronBar = Registry.Register(new Item("IronBar"));
        IronScrew = Registry.Register(new Item("IronScrew"));
        
        SteelBar = Registry.Register(new Item("SteelBar"));
    }

}