using BrainWithNoSkull.Client.Models.Blocks.Unbaked;

namespace BrainWithNoSkull.Client.Models.Items.Unbaked;

public abstract class UnbakedItemModel {
    public List<string> TextureDependencies = [];
    public abstract void GatherDependencies();
    
    public abstract IBakedItemModel Bake(TextureResolver resolver);
}