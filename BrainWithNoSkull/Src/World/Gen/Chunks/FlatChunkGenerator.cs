using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen.Chunks;

public class FlatChunkGenerator : IChunkGenerator {
    public void Generate(Vector2D<int> chunkCoords, int size, GenChunk chunk){
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                float noiseHeight = OpenSimplex2.Noise4_ImproveXYZ(5, (chunkCoords.X*size + x)/20.0, 0, (chunkCoords.Y*size + z)/20.0, 0);
                int height = Constants.SeaLevel + (int)(noiseHeight * 5);
                for (int y = 0; y < height; y++) {
                    chunk.Blocks[x, y, z] = BlockRegistry.Tile;
                }
            }
        }
    }
}