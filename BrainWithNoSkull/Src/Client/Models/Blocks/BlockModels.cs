using BrainWithNoSkull.Client.Models.Blocks.Unbaked;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Blocks;

namespace BrainWithNoSkull.Client.Models.Blocks;

public class BlockModels {

    private static readonly Dictionary<Block, UnbakedBlockModel> UnbakedModels = new Dictionary<Block, UnbakedBlockModel>();
    public static readonly Dictionary<Block, BakedBlockModel> BakedModels = new Dictionary<Block, BakedBlockModel>();
    
    public static ICollection<string> CreateUnbaked(){
        UnbakedModels.Clear();

        UnbakedModels[BlockRegistry.Grass] = new CubeBlockModel("Block/Grass", "Block/Dirt", "Block/Grass_Side", "Block/Grass_Side", "Block/Grass_Side", "Block/Grass_Side");
        UnbakedModels[BlockRegistry.Dirt] = new CubeBlockModel("Block/Dirt");

        HashSet<string> deps = [];
        foreach (UnbakedBlockModel model in UnbakedModels.Values) {
            model.GatherDependencies();
            deps.UnionWith(model.TextureDependencies);
        }

        return deps;
    }

    public static void Bake(TextureResolver resolver){
        BakedModels.Clear();
        foreach ((Block block, UnbakedBlockModel unbaked) in UnbakedModels) {
            BakedModels[block] = unbaked.Bake(resolver);
        }

        UnbakedModels.Clear();
    }
    
}