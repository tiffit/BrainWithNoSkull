using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Gen.Chunks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen.Features;

public class OreFeatureGenerator: IFeatureGenerator {
    
    private readonly RandomSource _random;
    private readonly long _seed;
    private readonly int _minY, _maxY, _minSize, _maxSize;
    private readonly Block _oreBlock;
    private readonly int _checkSize;
    
    public OreFeatureGenerator(WorldSettings settings, Block oreBlock, int minY, int maxY, int minSize, int maxSize){
        _seed = settings.Seed - ("Ore" + oreBlock.GetId()).GetHashCode();
        _random = new RandomSource(_seed);
        _minY = minY;
        _maxY = maxY;
        _minSize = minSize;
        _maxSize = maxSize;
        _oreBlock = oreBlock;
        _checkSize = maxSize / 8 + 1;
    }
    
    public void GenerateFeatures(WorldBase world, Vector2D<int> coords, IChunkGenerator chunkGen, GenChunk genChunk){
        for (int x = -_checkSize; x < Constants.ChunkSize + _checkSize; x++) {
            for (int z = -_checkSize; z < Constants.ChunkSize + _checkSize; z++) {
                for (int y = _minY; y <= _maxY; y++) {
                    if (CanGenerate(x + coords.X*Constants.ChunkSize, y, z+coords.Y*Constants.ChunkSize)) {
                        GenerateVein(genChunk, x, y, z);
                    }
                }
            }
        }
    }

    private bool CanGenerate(int worldX, int worldY, int worldZ){
        _random.SetSeed(_seed + HashCode.Combine(worldX, worldY, worldZ));
        return _random.NextDouble() < 0.005;
    }
    
    private void GenerateVein(GenChunk genChunk, int x, int y, int z){
        int numToGenerate = _random.NextInt(_maxSize - _minSize) + _minSize;
        float rotation = _random.NextFloat() * MathF.PI;
        float scale = numToGenerate / 8.0f;

        float sin = MathF.Sin(rotation) * scale;
        float cos = MathF.Cos(rotation) * scale;
        
        float minX = x - sin;
        float maxX = x + sin;
        float minY = -1;
        float maxY = 1;
        float minZ = z - sin;
        float maxZ = z + sin;

        HashSet<Vector3D<int>> toPlace = [];
        HashSet<Vector3D<int>> potentialPlace = [];
        
        for (int i = 0; i < numToGenerate; i++) {
            float delta = (float)i / numToGenerate;
            float posX = float.Lerp(minX, maxX, delta);
            float posY = float.Lerp(minY, maxY, delta);
            float posZ = float.Lerp(minZ, maxZ, delta);

            toPlace.Add(new Vector3D<int>((int)MathF.Floor(posX), (int)MathF.Floor(posY), (int)MathF.Floor(posZ)));

            for (float dx = -1; dx <= 1; dx++) {
                for (float dy = -1; dy <= 1; dy++) {
                    for (float dz = -1; dz <= 1; dz++) {
                        Vector3D<int> pos = new Vector3D<int>((int)MathF.Floor(posX + dx), (int)MathF.Floor(posY + dy), (int)MathF.Floor(posZ + dz));
                        if (!toPlace.Contains(pos)) {
                            potentialPlace.Add(pos);
                        }
                    }
                }
            }
        }

        List<Vector3D<int>> potential = new List<Vector3D<int>>(potentialPlace);
        
        while (toPlace.Count < numToGenerate && potential.Count > 0) {
            int index = _random.NextInt(potential.Count);
            toPlace.Add(potential[index]);
            potential.RemoveAt(index);
        }
        
        foreach (Vector3D<int> vec in toPlace) {
            int placeX = x + vec.X;
            int placeY = y + vec.Y;
            int placeZ = z + vec.Z;
            if(GenChunk.OutOfBounds(placeX, placeY, placeZ))
                continue;
            if (genChunk.Blocks[placeX, placeY, placeZ] == BlockRegistry.Stone) {
                genChunk.SetBlock(placeX, placeY, placeZ, _oreBlock);
            }
        }
        
    }
    
}