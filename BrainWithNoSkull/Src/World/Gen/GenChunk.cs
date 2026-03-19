using BrainWithNoSkull.Blocks;
using BrainWithNoSkull.Registries;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen;

public class GenChunk {
    public readonly Block[,,] Blocks;
    private readonly int _size;
    private readonly Vector2D<int> _chunkCoords;
    
    public GenChunk(Vector2D<int> chunkCoords, int size){
        _chunkCoords = chunkCoords;
        _size = size;
        Blocks = new Block[size, Constants.MaxWorldHeight, size];
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < Constants.MaxWorldHeight; y++) {
                for (int z = 0; z < size; z++) {
                    Blocks[x, y, z] = BlockRegistry.Air;
                }
            }
        }
    }

    public Chunk ToChunk(WorldBase world){
        Chunk chunk = new Chunk(world, _chunkCoords, Blocks);
        return chunk;
    }
}