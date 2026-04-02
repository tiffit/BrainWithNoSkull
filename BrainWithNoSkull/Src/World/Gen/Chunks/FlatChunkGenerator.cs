using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Gen.Noise;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen.Chunks;

public class FlatChunkGenerator : IChunkGenerator {

    private readonly WorldSettings _settings;
    private readonly Octaves _octaves;
    
    public FlatChunkGenerator(WorldSettings settings){
        _settings = settings;
        _octaves = [
            new Octave(_settings.Seed, 1, 3),
            new Octave(_settings.Seed, 3, 5),
            new Octave(_settings.Seed, 10, 10),
        ];
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
        float worldX = chunkCoords.X * Constants.ChunkSize + x;
        float worldZ = chunkCoords.Y * Constants.ChunkSize + z;
        return _octaves.GetHeight(worldX, worldZ, _settings.WorldSize, Constants.SeaLevel);
    }
}