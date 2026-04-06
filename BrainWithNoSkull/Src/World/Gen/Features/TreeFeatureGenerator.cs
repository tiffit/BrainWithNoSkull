using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks;
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
        for (int x = -4; x < Constants.ChunkSize + 4; x++) {
            for (int z = -4; z < Constants.ChunkSize + 4; z++) {
                if (CanGenerate(x + coords.X*Constants.ChunkSize, z+coords.Y*Constants.ChunkSize)) {
                    GenerateTreeAt(genChunk, in coords, x, chunkGen.GetHeight(coords, x, z), z);
                }
            }
        }
    }

    private void GenerateTreeAt(GenChunk genChunk, in Vector2D<int> chunkCoords, int x, int y, int z){
        double worldX = chunkCoords.X * Constants.ChunkSize + x;
        double worldZ = chunkCoords.Y * Constants.ChunkSize + z;
        _random.SetSeed(_seed + (long)worldX * 1619 ^ ((long)worldZ * 31337) + 3);
        int height = _random.NextInt(3) + 6;

        for (int xOffset = -1; xOffset <= 1; xOffset++) {
            for (int yOffset = 0; yOffset <= 1; yOffset++) {
                for (int zOffset = -1; zOffset <= 1; zOffset++) {
                    genChunk.SetBlock(x + xOffset, y + height + yOffset, z + zOffset, BlockRegistry.OakLeaves);
                }
            }
        }
        
        for (int h = y; h <= y + height; h++) {
            genChunk.SetBlock(x, h, z, BlockRegistry.OakLog);
        }


        List<Rotation> rotations = [Rotation.R0, Rotation.R90, Rotation.R180, Rotation.R270];
        int branchesStartHeight = _random.NextBool() ? y + 2 : y + 3;
        for (int h = branchesStartHeight; h < y + height - 2; h++) {
            if (_random.NextDouble() < 0.75) {
                Rotation rotation = rotations[_random.NextInt(rotations.Count)];
                rotations.Remove(rotation);
                
                SetRotatedBlock(genChunk, x, h, z, 1, 0, rotation, BlockRegistry.OakLeaves);
                SetRotatedBlock(genChunk, x, h, z, 1, 1, rotation, BlockRegistry.OakLeaves);
                SetRotatedBlock(genChunk, x, h, z, 1, -1, rotation, BlockRegistry.OakLeaves);
                SetRotatedBlock(genChunk, x, h, z, 2, 0, rotation, BlockRegistry.OakLeaves);
                
                if (_random.NextDouble() < .50) {
                    SetRotatedBlock(genChunk, x, h, z, 1, 0, rotation, BlockRegistry.OakLeaves);
                }
                else {
                    SetRotatedBlock(genChunk, x, h, z, 1, 0, rotation, BlockRegistry.OakLog);
                    SetRotatedBlock(genChunk, x, h, z, 2, 1, rotation, BlockRegistry.OakLeaves);
                    SetRotatedBlock(genChunk, x, h, z, 2, -1, rotation, BlockRegistry.OakLeaves);
                    SetRotatedBlock(genChunk, x, h, z, 3, 0, rotation, BlockRegistry.OakLeaves);
                }
                h++;
            }
        }
        
        genChunk.SetBlock(x, y + height + 2, z, BlockRegistry.OakLeaves);
    }

    private void SetRotatedBlock(GenChunk genChunk, int x, int y, int z, int offsetX, int offsetZ, Rotation rot, Block block){
        Direction.RotateAround(rot, ref offsetX, ref offsetZ);
        genChunk.SetBlock(x + offsetX, y, z + offsetZ, block);
    }

    private bool CanGenerate(int worldX, int worldZ){
        _random.SetSeed(_seed + HashCode.Combine(worldX, worldZ));
        return _random.NextDouble() < 0.008;
    }
}