using BrainWithNoSkull.Registries;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen.Chunks;

public class FlatChunkGenerator : IChunkGenerator{
    
    public void Generate(Vector2D<int> chunkCoords, int size, GenChunk chunk){
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < Constants.SeaLevel; y++) {
                for (int z = 0; z < size; z++) {
                    chunk.Blocks[x, y, z] = BlockRegistry.Tile;
                }
            }
        }
    }
    
}