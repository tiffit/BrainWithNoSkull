using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Gen.Chunks;
using BrainWithNoSkull.World.Gen.Features;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen;

public class WorldGenerator {

    private readonly WorldBase _world;
    private readonly IChunkGenerator _chunkGen;
    private readonly LinkedList<IFeatureGenerator> _featureGenerators = [];

    public WorldGenerator(WorldBase world, IChunkGenerator chunkGen){
        _world = world;
        _chunkGen = chunkGen;
        _featureGenerators.AddLast(new TreeFeatureGenerator(world.Settings));
        _featureGenerators.AddLast(new PebbleFeatureGenerator(world.Settings));
        _featureGenerators.AddLast(new StickFeatureGenerator(world.Settings));
        _featureGenerators.AddLast(new OreFeatureGenerator(world.Settings, BlockRegistry.Magnetite, 32, 140, 5, 12));
    }

    public Chunk Generate(Vector2D<int> chunkCoords){
        GenChunk genChunk = new GenChunk(chunkCoords, Constants.ChunkSize);
        _chunkGen.Generate(chunkCoords, genChunk);
        
        foreach (IFeatureGenerator gen in _featureGenerators) {
            gen.GenerateFeatures(_world, chunkCoords, _chunkGen, genChunk);
        }
        
        return genChunk.ToChunk(_world);
    }
    
}