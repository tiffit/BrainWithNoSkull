using BrainWithNoSkull.Client.Models.Blocks.Unbaked;
using BrainWithNoSkull.Client.Models.Items.Unbaked;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Items;

namespace BrainWithNoSkull.Client.Models.Items;

public class ItemModels {
    private static readonly Dictionary<Item, UnbakedItemModel> UnbakedModels = new Dictionary<Item, UnbakedItemModel>();
    public static readonly Dictionary<Item, IBakedItemModel> BakedModels = new Dictionary<Item, IBakedItemModel>();
    
    public static HashSet<string> CreateUnbaked(){
        UnbakedModels.Clear();

        foreach ((Block block, BlockItem item) in ItemRegistry.BlockItems) {
            UnbakedModels[item] = new BlockItemModel(item);
        }
        
        UnbakedModels[ItemRegistry.StonePickaxe] = new FlatItemModel("Item/Stone_Pickaxe");
        UnbakedModels[ItemRegistry.IronBar] = new JsonItemModel("Bar", "Item/Iron_Bar");
        UnbakedModels[ItemRegistry.IronScrew] = new JsonItemModel("Screw", "Item/Iron_Screw");
        
        UnbakedModels[ItemRegistry.SteelBar] = new JsonItemModel("Bar", "Item/Steel_Bar");

        HashSet<string> deps = [];
        foreach (UnbakedItemModel model in UnbakedModels.Values) {
            model.GatherDependencies();
            deps.UnionWith(model.TextureDependencies);
        }

        return deps;
    }

    public static void Bake(TextureResolver resolver){
        BakedModels.Clear();
        foreach ((Item item, UnbakedItemModel unbaked) in UnbakedModels) {
            BakedModels[item] = unbaked.Bake(resolver);
        }

        UnbakedModels.Clear();
    }
}