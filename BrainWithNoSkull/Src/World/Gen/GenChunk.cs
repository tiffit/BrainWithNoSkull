using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen;

public class GenChunk {
    public readonly Block[,,] Blocks;
    public readonly uint[,,] BlockConfigs;
    private readonly int _size;
    private readonly Vector2D<int> _chunkCoords;
    
    public GenChunk(Vector2D<int> chunkCoords, int size){
        _chunkCoords = chunkCoords;
        _size = size;
        Blocks = new Block[size, Constants.MaxWorldHeight, size];
        BlockConfigs = new uint[size, Constants.MaxWorldHeight, size];
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < Constants.MaxWorldHeight; y++) {
                for (int z = 0; z < size; z++) {
                    Blocks[x, y, z] = BlockRegistry.Air;
                }
            }
        }
    }

    public void SetBlock(int x, int y, int z, Block block){
        if (x < 0 || y < 0 || z < 0 || x >= Constants.ChunkSize || y >= Constants.MaxWorldHeight || z >= Constants.ChunkSize) {
            return;
        }

        Blocks[x, y, z] = block;
    }

    public Chunk ToChunk(WorldBase world){
        Chunk chunk = new Chunk(world, _chunkCoords, Blocks, BlockConfigs);
        return chunk;
    }
}