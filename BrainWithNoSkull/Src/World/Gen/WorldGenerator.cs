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