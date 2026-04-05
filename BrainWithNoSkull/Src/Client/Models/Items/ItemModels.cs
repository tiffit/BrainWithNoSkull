using BrainWithNoSkull.Client.Models.Blocks.Unbaked;
using BrainWithNoSkull.Client.Models.Items.Unbaked;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Items;

namespace BrainWithNoSkull.Client.Models.Items;

public class ItemModels {
    private static readonly Dictionary<Item, UnbakedItemModel> UnbakedModels = new Dictionary<Item, UnbakedItemModel>();
    public static readonly Dictionary<Item, IBakedItemModel> BakedModels = new Dictionary<Item, IBakedItemModel>();
    
    public static HashSet<string> CreateUnbaked(){
        UnbakedModels.Clear();

        UnbakedModels[ItemRegistry.StonePickaxe] = new FlatItemModel("Item/Stone_Pickaxe");

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