using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using BrainWithNoSkull.World.Gen.Chunks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen.Features;

public class PebbleFeatureGenerator: IFeatureGenerator {
    private readonly RandomSource _random;
    private readonly long _seed;
    
    public PebbleFeatureGenerator(WorldSettings settings){
        _seed = settings.Seed - "Pebbles".GetHashCode();
        _random = new RandomSource(_seed);
    }
    
    public void GenerateFeatures(WorldBase world, Vector2D<int> coords, IChunkGenerator chunkGen, GenChunk genChunk){
        for (int x = 0; x < Constants.ChunkSize; x++) {
            for (int z = 0; z < Constants.ChunkSize; z++) {
                if (CanGenerate(x + coords.X*Constants.ChunkSize, z+coords.Y*Constants.ChunkSize)) {
                    int height = chunkGen.GetHeight(coords, x, z);
                    if (genChunk.Blocks[x, height, z].IsAir()) {
                        Direction direction = Direction.Horizontal[_random.NextInt(4)];
                        genChunk.Blocks[x, height, z] = BlockRegistry.Pebbles;
                        genChunk.BlockConfigs[x, height, z] = BlockRegistry.Pebbles.ConfigContainer.SetValue(0, HorizontalFacingBlockConfigType.Instance, direction);
                    }
                }
            }
        }
    }
    
    private bool CanGenerate(int worldX, int worldZ){
        _random.SetSeed(_seed + (long)worldX * 1619 ^ ((long)worldZ * 31337));
        return _random.NextDouble() < 0.015;
    }
}