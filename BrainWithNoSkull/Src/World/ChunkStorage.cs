using BrainWithNoSkull.World.Gen;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class ChunkStorage {

    private readonly WorldBase _world;
    private readonly bool _remote;
    private readonly WorldGenerator _worldGenerator;
    private readonly Dictionary<Vector2D<int>, Chunk> _loadedChunks = new Dictionary<Vector2D<int>, Chunk>();
    
    public ChunkStorage(WorldBase world){
        _world = world;
        _worldGenerator = world.WorldGenerator;
    }

    public Chunk? GetChunk(Vector2D<int> chunkCoords){
        chunkCoords = new Vector2D<int>(WrapChunkCoord(chunkCoords.X), WrapChunkCoord(chunkCoords.Y));
        if (_loadedChunks.TryGetValue(chunkCoords, out Chunk? chunk)) {
            return chunk;
        }

        if (!_remote) {
            chunk = _worldGenerator.Generate(chunkCoords);
            _loadedChunks[chunkCoords] = chunk;
            return chunk;
        }
        return null;
    }

    public int WrapChunkCoord(int val){
        val %= (int)_world.Settings.WorldSize;
        if (val < 0) {
            val = (int)_world.Settings.WorldSize + val;
        }
        return val;
    }

    public void Tick(double delta){
        foreach ((Vector2D<int> pos, Chunk chunk) in _loadedChunks) {
            chunk.Tick();
        }
    }
    
}