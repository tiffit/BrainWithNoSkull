using BrainWithNoSkull.World.Gen;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class ChunkStorage {

    private readonly WorldBase _world;
    private readonly WorldGenerator _worldGenerator;
    private readonly Dictionary<Vector2D<int>, Chunk> _loadedChunks = new Dictionary<Vector2D<int>, Chunk>();
    
    public ChunkStorage(WorldBase world){
        _world = world;
        _worldGenerator = world.WorldGenerator;
    }

    public Chunk GetChunk(Vector2D<int> chunkCoords){
        int worldSize = _world.Settings.WorldSize;
        chunkCoords = new Vector2D<int>(WrapChunkCoord(chunkCoords.X), WrapChunkCoord(chunkCoords.Y));
        if (_loadedChunks.TryGetValue(chunkCoords, out Chunk? chunk)) {
            return chunk;
        }
        chunk = _worldGenerator.Generate(chunkCoords);
        _loadedChunks[chunkCoords] = chunk;
        return chunk;
    }

    public int WrapChunkCoord(int val){
        val %= _world.Settings.WorldSize;
        if (val < 0) {
            val = _world.Settings.WorldSize + val;
        }
        return val;
    }
    
}