using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Models.Blocks;

public class ConfigDepBakedBlockModel<T>: IBakedBlockModel where T : notnull {

    private readonly Dictionary<T, IBakedBlockModel> _children;
    private readonly BlockConfigType<T> _configType;
    
    public ConfigDepBakedBlockModel(BlockConfigType<T> configType, Dictionary<T, IBakedBlockModel> children){
        _children = children;
        _configType = configType;
    }
    
    public IEnumerable<Vertex> GetVertices(Vector3D<float> posOffset, Block block, uint packedConfig, List<Direction> visible){
        T val = block.ConfigContainer.GetValue(packedConfig, _configType);
        return _children[val].GetVertices(posOffset, block, packedConfig, visible);
    }

    public IEnumerable<Vertex> GetVerticesForDirection(Direction? direction, Block block, uint packedConfig){
        T val = block.ConfigContainer.GetValue(packedConfig, _configType);
        return _children[val].GetVerticesForDirection(direction, block, packedConfig);
    }
    
}