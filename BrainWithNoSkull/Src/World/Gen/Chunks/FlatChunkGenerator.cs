using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen.Chunks;

public class FlatChunkGenerator : IChunkGenerator {

    private readonly WorldSettings _settings;
    
    public FlatChunkGenerator(WorldSettings settings){
        _settings = settings;
    }
    
    public void Generate(Vector2D<int> chunkCoords, int size, GenChunk chunk){
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                double worldX = chunkCoords.X * size + x;
                double worldZ = chunkCoords.Y * size + z;
                double angleX = MathF.PI * 2 * worldX / (_settings.WorldSize * Constants.ChunkSize);
                double angleZ = MathF.PI * 2 * worldZ / (_settings.WorldSize * Constants.ChunkSize);
                double noiseX = Math.Cos(angleX);
                double noiseY = Math.Sin(angleX);
                double noiseZ = Math.Cos(angleZ);
                double noiseW = Math.Sin(angleZ);
                float noiseHeight = OpenSimplex2.Noise4_ImproveXYZ(DateTimeOffset.Now.ToUnixTimeMilliseconds(), noiseX, noiseY, noiseZ, noiseW);
                int height = Constants.SeaLevel + (int)(noiseHeight * 5);

                for (int y = 0; y < height; y++) {
                    chunk.Blocks[x, y, z] = BlockRegistry.Tile;
                }
            }
        }
    }
}