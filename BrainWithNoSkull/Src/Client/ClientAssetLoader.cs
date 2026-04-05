using BrainWithNoSkull.Client.Atlas;
using BrainWithNoSkull.Client.Models.Blocks;
using BrainWithNoSkull.Client.Models.Items;

namespace BrainWithNoSkull.Client;

public class ClientAssetLoader {

    public static readonly BlockTextureAtlas BlockTextureAtlas = new BlockTextureAtlas();
    public static readonly FontTextureAtlas FontTextureAtlas = new FontTextureAtlas();
    
    public static void Load(){
        HashSet<string> deps = BlockModels.CreateUnbaked();
        deps.UnionWith(ItemModels.CreateUnbaked());
        
        BlockTextureAtlas.Create(deps);
        BlockTextureAtlas.CreateGuiTextures();
        BlockModels.Bake(BlockTextureAtlas.LookupBlock);
        ItemModels.Bake(BlockTextureAtlas.LookupBlock);
        Console.WriteLine("Baked " + BlockModels.BakedModels.Count + " block models");
        Console.WriteLine("Baked " + ItemModels.BakedModels.Count + " item models");
        FontTextureAtlas.Create();
    }
    
}