using BrainWithNoSkull.Client.Atlas;
using BrainWithNoSkull.Client.Models.Blocks;

namespace BrainWithNoSkull.Client;

public class ClientAssetLoader {

    public static readonly BlockTextureAtlas BlockTextureAtlas = new BlockTextureAtlas();
    public static readonly FontTextureAtlas FontTextureAtlas = new FontTextureAtlas();
    
    public static void Load(){
        ICollection<string> deps = BlockModels.CreateUnbaked();
        BlockTextureAtlas.Create(deps);
        BlockTextureAtlas.CreateGuiTextures();
        BlockModels.Bake(BlockTextureAtlas.LookupBlock);
        Console.WriteLine("Baked " + BlockModels.BakedModels.Count + " block models");
        FontTextureAtlas.Create();
    }
    
}