using BrainWithNoSkull.Client.Models.Blocks.Unbaked;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Models.Blocks;

public class BlockModels {

    private static readonly Dictionary<Block, UnbakedBlockModel> UnbakedModels = new Dictionary<Block, UnbakedBlockModel>();
    public static readonly Dictionary<Block, IBakedBlockModel> BakedModels = new Dictionary<Block, IBakedBlockModel>();
    
    public static HashSet<string> CreateUnbaked(){
        UnbakedModels.Clear();

        UnbakedModels[BlockRegistry.Grass] = new CubeBlockModel("Block/Grass", "Block/Dirt", "Block/Grass_Side", "Block/Grass_Side", "Block/Grass_Side", "Block/Grass_Side");
        UnbakedModels[BlockRegistry.Dirt] = new CubeBlockModel("Block/Dirt");
        UnbakedModels[BlockRegistry.Stone] = new CubeBlockModel("Block/Stone");
        UnbakedModels[BlockRegistry.OakLog] = new CubeBlockModel("Block/Oak_Log");
        UnbakedModels[BlockRegistry.OakLeaves] = new CubeBlockModel("Block/Oak_Leaves");
        UnbakedModels[BlockRegistry.Pebbles] = new RotatingBlockModel(new JsonBlockModel("Rocks", "Block/Rocks"), 
            BlockRegistry.Pebbles, HorizontalFacingBlockConfigType.Instance, Direction.Horizontal);
        UnbakedModels[BlockRegistry.Stick] = new RotatingBlockModel(new JsonBlockModel("Stick", "Block/Stick"),
            BlockRegistry.Stick, HorizontalFacingBlockConfigType.Instance, Direction.Horizontal);
        UnbakedModels[BlockRegistry.Magnetite] = new CubeBlockModel("Block/Magnetite");
        UnbakedModels[BlockRegistry.CrudeFurnace] = new MultiblockBlockModel(BlockRegistry.CrudeFurnace, new Dictionary<Vector3D<uint>, UnbakedBlockModel>(){
            {Vector3D<uint>.Zero, new JsonBlockModel("CrudeFurnaceBottom", "Block/CrudeFurnace")},
            {Vector3D<uint>.UnitY, new JsonBlockModel("CrudeFurnaceTop", "Block/CrudeFurnace")}
        });

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
            BakedModels[block] = unbaked.Bake(resolver, 0);
        }

        UnbakedModels.Clear();
    }
    
}