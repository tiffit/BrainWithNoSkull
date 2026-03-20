using BrainWithNoSkull.Blocks;
using BrainWithNoSkull.Registries;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class Chunk {

    private readonly WorldBase _world;
    private readonly Vector2D<int> _coords;
    private readonly Block[,,] _blocks;
    
    public Chunk(WorldBase world, Vector2D<int> chunkCoords, Block[,,] blocks){
        _world = world;
        _coords = chunkCoords;
        _blocks = blocks;
    }

    public Block GetBlock(int x, int y, int z){
        if (x < 0 || y < 0 || z < 0 || x >= Constants.ChunkSize || y >= Constants.MaxWorldHeight || z >= Constants.ChunkSize) {
            return BlockRegistry.Air;
        }
        return _blocks[x, y, z];
    }
    
}