using BrainWithNoSkull.World.Gen.Chunks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen;

public class WorldGenerator {

    private readonly WorldBase _world;
    private readonly IChunkGenerator _chunkGen;

    public WorldGenerator(WorldBase world, IChunkGenerator chunkGen){
        _world = world;
        _chunkGen = chunkGen;
    }

    public Chunk Generate(Vector2D<int> chunkCoords){
        GenChunk genChunk = new GenChunk(chunkCoords, Constants.ChunkSize);
        _chunkGen.Generate(chunkCoords, Constants.ChunkSize, genChunk);
        return genChunk.ToChunk(_world);
    }
    
}