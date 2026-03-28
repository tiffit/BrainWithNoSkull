using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen.Chunks;

public class FlatChunkGenerator : IChunkGenerator {

    private readonly WorldSettings _settings;
    
    public FlatChunkGenerator(WorldSettings settings){
        _settings = settings;
    }
    
    public void Generate(Vector2D<int> chunkCoords, GenChunk chunk){
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int z = 0; z < Constants.ChunkSize; z++) {
                int height = GetHeight(chunkCoords, x, z);
                
                for (int y = 0; y < height; y++) {
                    Block block = BlockRegistry.Stone;
                    if (y == height - 1) block = BlockRegistry.Grass;
                    else if (y >= height - 3) block = BlockRegistry.Dirt;
                    chunk.Blocks[x, y, z] = block;
                }
            }
        }
    }

    public int GetHeight(Vector2D<int> chunkCoords, int x, int z){
        double worldX = chunkCoords.X * Constants.ChunkSize + x;
        double worldZ = chunkCoords.Y * Constants.ChunkSize + z;
        double angleX = MathF.PI * 2 * worldX / (_settings.WorldSize * Constants.ChunkSize);
        double angleZ = MathF.PI * 2 * worldZ / (_settings.WorldSize * Constants.ChunkSize);
        double scale = _settings.WorldSize / 8;
        double noiseX = Math.Cos(angleX) * scale;
        double noiseY = Math.Sin(angleX) * scale;
        double noiseZ = Math.Cos(angleZ) * scale;
        double noiseW = Math.Sin(angleZ) * scale;
        float noiseHeight = OpenSimplex2.Noise4_ImproveXYZ(_settings.Seed, noiseX, noiseY, noiseZ, noiseW);
        return Constants.SeaLevel + (int)(noiseHeight * 3);
    }
}