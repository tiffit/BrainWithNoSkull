using BrainWithNoSkull.Client.Atlas;

namespace BrainWithNoSkull.Client.Models.Blocks.Unbaked;

public abstract class UnbakedBlockModel {

    public List<string> TextureDependencies = [];
    public abstract void GatherDependencies();

    public abstract BakedBlockModel Bake(TextureResolver resolver);
}

public delegate uint TextureResolver(string location);