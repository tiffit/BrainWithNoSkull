using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Models.Blocks.Unbaked;

public class RotatingBlockModel : UnbakedBlockModel{
    
    private readonly BlockConfigType<Direction> _configType;
    private readonly Block _block;
    private readonly UnbakedBlockModel _child;
    private readonly Direction[] _directions;
    
    public RotatingBlockModel(UnbakedBlockModel child, Block block, BlockConfigType<Direction> config, params Direction[] directions){
        _configType = config;
        _child = child;
        _block = block;
        _directions = directions;
    }
    
    public override void GatherDependencies(){
        _child.GatherDependencies();
        TextureDependencies = _child.TextureDependencies;
    }

    private static List<Vertex> Transform(List<Vertex> vertices, Rotation rotation){
        return vertices.Select(v => {
            float x = v.Pos.X - 0.5f;
            float z = v.Pos.Z - 0.5f;
            Direction.RotateAround(rotation, ref x, ref z);
            return v with{
                Pos = new Vector3D<float>(x + 0.5f, v.Pos.Y, z + 0.5f)
            };
        }).ToList();
    }

    public override IBakedBlockModel Bake(TextureResolver resolver, uint packedConfig){
        IBakedBlockModel baseModel = _child.Bake(resolver, packedConfig);
        Dictionary<Direction, IBakedBlockModel> children = new Dictionary<Direction, IBakedBlockModel>();
        foreach (Direction direction in _directions) {
            List<Vertex> all = [];
            Dictionary<Direction, List<Vertex>> directional = new Dictionary<Direction, List<Vertex>>();
            Dictionary<Direction, List<Vertex>> rotatedDict = new Dictionary<Direction, List<Vertex>>();
            uint value = _block.ConfigContainer.SetValue(packedConfig, _configType, direction);
            baseModel.GetAllVertices(all, directional, _block, value);
            
            foreach ((Direction dir, List<Vertex> vertices) in directional) {
                Direction newDir = dir.RotateHorizontal(direction.Rotation);
                rotatedDict[newDir] = Transform(vertices, direction.Rotation);
            }
            
            children[direction] = new SimpleBakedBlockModel(Transform(all, direction.Rotation), rotatedDict);
        }

        return new ConfigDepBakedBlockModel<Direction>(_configType, children);
    }
    
}