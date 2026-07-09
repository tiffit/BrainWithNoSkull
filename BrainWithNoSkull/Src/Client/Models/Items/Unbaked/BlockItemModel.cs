using BrainWithNoSkull.Client.Models.Blocks;
using BrainWithNoSkull.Client.Models.Blocks.Unbaked;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using BrainWithNoSkull.World.Items;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Models.Items.Unbaked;

public class BlockItemModel: UnbakedItemModel {

    private static readonly List<ValueTuple<Direction, uint>> StaticDirections = Direction.All.Select(direction => new ValueTuple<Direction, uint>(direction, 15 << 4)).ToList();

    public Dictionary<ItemModelTransformType, ItemModelTransforms> Transforms = new Dictionary<ItemModelTransformType, ItemModelTransforms>{
        {
            ItemModelTransformType.Gui, new ItemModelTransforms(){
                Rotation = new Vector3D<float>(180+30, -125, 0),
                Scale = new Vector3D<float>(40, 40, 1)
            }
        }
    };
    
    private readonly Block _block;
    
    public BlockItemModel(BlockItem item){
        _block = item.Block;
    }
    
    public override void GatherDependencies(){
    }

    public override IBakedItemModel Bake(TextureResolver resolver){
        BlockModels.BakedModels.TryGetValue(_block, out IBakedBlockModel? model);
        if (model == null)
            return new SimpleBakedItemModel([], []);
        
        List<Tuple<Vector3D<float>, uint>> configs = [];
        uint defaultConfig = _block.ConfigContainer.GetDefault();
        if (_block is IMultiblock multiblock) {
            Vector3D<int> multiblockSize = multiblock.GetMultiblockSize();
            for (uint mbx = 0; mbx < multiblockSize.X; mbx++) {
                for (uint mby = 0; mby < multiblockSize.Y; mby++) {
                    for (uint mbz = 0; mbz < multiblockSize.Z; mbz++) {
                        uint config = _block.ConfigContainer.SetValue(defaultConfig, MultiblockBlockConfigType.Instance, new Vector3D<uint>(mbx, mby, mbz));
                        configs.Add(Tuple.Create(new Vector3D<float>(mbx, mby, mbz), config));
                    }
                }
            }
        }
        else {
            configs.Add(Tuple.Create(Vector3D<float>.Zero, defaultConfig));
        }
        
        List<Vertex> vertices = [];
        
        foreach (var (translation, config) in configs) {
            vertices.AddRange(model.GetVertices(new Vector3D<float>(-0.5f, -0.5f, -0.5f) + translation, _block, config, 15 << 4, StaticDirections));
        }
        
        Dictionary<ItemModelTransformType, ItemModelTransform> transforms = Transforms
            .Select(pair => new KeyValuePair<ItemModelTransformType, ItemModelTransform>(pair.Key, pair.Value.GetTransform()))
            .ToDictionary();
        
        return new SimpleBakedItemModel(vertices, transforms);
    }
    
}