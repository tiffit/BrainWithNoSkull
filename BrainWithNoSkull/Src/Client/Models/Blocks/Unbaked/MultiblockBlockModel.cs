using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Models.Blocks.Unbaked;

public class MultiblockBlockModel : UnbakedBlockModel {

    private readonly Dictionary<Vector3D<uint>, UnbakedBlockModel> _dict;
    private readonly Block _block;
    
    public MultiblockBlockModel(Block block, Dictionary<Vector3D<uint>, UnbakedBlockModel> dict){
        _block = block;
        _dict = dict;
    }
    
    public override void GatherDependencies(){
        TextureDependencies = [];
        foreach (UnbakedBlockModel model in _dict.Values) {
            model.GatherDependencies();
            TextureDependencies.AddRange(model.TextureDependencies);
        }
    }

    public override IBakedBlockModel Bake(TextureResolver resolver, uint packedConfig){
        Dictionary<Vector3D<uint>, IBakedBlockModel> children = new Dictionary<Vector3D<uint>, IBakedBlockModel>();
        foreach (var (key, value) in _dict) {
            children[key] = value.Bake(resolver, _block.ConfigContainer.SetValue(packedConfig, MultiblockBlockConfigType.Instance, key));
        }
        return new ConfigDepBakedBlockModel<Vector3D<uint>>(MultiblockBlockConfigType.Instance, children);
    }
    
}