using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Gen.Chunks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen.Features;

public class TreeFeatureGenerator : IFeatureGenerator {
    private readonly RandomSource _random;
    private readonly long _seed;

    public TreeFeatureGenerator(WorldSettings settings){
        _seed = settings.Seed - "Trees".GetHashCode();
        _random = new RandomSource(_seed);
    }

    public void GenerateFeatures(WorldBase world, Vector2D<int> coords, IChunkGenerator chunkGen, GenChunk genChunk){
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int z = 0; z < Constants.ChunkSize; z++) {
                if (CanGenerate(x + coords.X*Constants.ChunkSize, z+coords.Y*Constants.ChunkSize)) {
                    GenerateTreeAt(genChunk, x, chunkGen.GetHeight(coords, x, z), z);
                }
            }
        }
    }

    private void GenerateTreeAt(GenChunk genChunk, int x, int y, int z){
        _random.SetSeed(_seed + (long)x * 1619 ^ ((long)z * 31337) + 3);
        int height = _random.NextInt(3) + 6;
        for (int h = y; h <= y + height; h++) {
            genChunk.SetBlock(x, h, z, BlockRegistry.OakLog);
        }
    }

    private bool CanGenerate(int worldX, int worldZ){
        _random.SetSeed(_seed + (long)worldX * 1619 ^ ((long)worldZ * 31337));
        return _random.NextDouble() < 0.01;
    }
}