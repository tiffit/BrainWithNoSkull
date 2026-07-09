using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Server;
using BrainWithNoSkull.World.Gen;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class ChunkStorage {

    private readonly WorldBase _world;
    private readonly bool _onServer;
    private readonly WorldGenerator _worldGenerator;
    private readonly Dictionary<Vector2D<int>, Chunk> _loadedChunks = new Dictionary<Vector2D<int>, Chunk>();
    private readonly LightEngine _lightEngine = new LightEngine();
    
    public ChunkStorage(WorldBase world){
        _world = world;
        _onServer = _world.OnServer;
        _worldGenerator = world.WorldGenerator;
    }

    public Chunk? GetChunk(Vector2D<int> chunkCoords, bool generate = true){
        chunkCoords = new Vector2D<int>(WrapChunkCoord(chunkCoords.X), WrapChunkCoord(chunkCoords.Y));
        if (_loadedChunks.TryGetValue(chunkCoords, out Chunk? chunk)) {
            return chunk;
        }

        if (_onServer && generate) {
            chunk = _worldGenerator.Generate(chunkCoords);
            _loadedChunks[chunkCoords] = chunk;
            return chunk;
        }
        return null;
    }

    public bool IsChunkLoaded(Vector2D<int> chunkCoords){
        chunkCoords = new Vector2D<int>(WrapChunkCoord(chunkCoords.X), WrapChunkCoord(chunkCoords.Y));
        return _loadedChunks.ContainsKey(chunkCoords);
    }
    
    public bool AdjacentGenerated(Vector2D<int> chunkCoords){
        return IsChunkLoaded(chunkCoords + Vector2D<int>.UnitX) &&
               IsChunkLoaded(chunkCoords - Vector2D<int>.UnitX) &&
               IsChunkLoaded(chunkCoords + Vector2D<int>.UnitY) &&
               IsChunkLoaded(chunkCoords - Vector2D<int>.UnitY);
    }

    private void TryPopulateLighting(Vector2D<int> chunkCoords){
        if (IsChunkLoaded(chunkCoords)) {
            Chunk chunk = GetChunk(chunkCoords)!;
            if (!chunk.HasPopulatedLighting && AdjacentGenerated(chunkCoords)) {
                _lightEngine.FullRebuild(this, chunk);
                if (_world is ServerWorld serverWorld) {
                    foreach (ConnectedClientBase client in serverWorld.GetAllListeners(chunkCoords)) {
                        client.SendPacket(new CbFullLightPacket(chunk.GetPackedLight(), chunkCoords));
                    }
                }
            }
        }
    }
    
    public bool GetChunkRelativeBlock(ref Chunk? chunk, ref int x, ref int y, ref int z){
        if (chunk == null) return false;
        if (x < 0) {
            chunk = GetChunk(chunk.Coords - Vector2D<int>.UnitX, false);
            if (chunk == null) return false;
            x += Constants.ChunkSize;
        }
        if (x >= Constants.ChunkSize) {
            chunk = GetChunk(chunk.Coords + Vector2D<int>.UnitX, false);
            if (chunk == null) return false;
            x -= Constants.ChunkSize;
        }
        if (z < 0) {
            chunk = GetChunk(chunk.Coords - Vector2D<int>.UnitY, false);
            if (chunk == null) return false;
            z += Constants.ChunkSize;
        }
        if (z >= Constants.ChunkSize) {
            chunk = GetChunk(chunk.Coords + Vector2D<int>.UnitY, false);
            if (chunk == null) return false;
            z -= Constants.ChunkSize;
        }
        return true;
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
            if (!chunk.HasPopulatedLighting) {
                TryPopulateLighting(pos);
            }
        }
    }

    public void LoadChunk(Chunk chunk){
        if (_loadedChunks.TryGetValue(chunk.Coords, out Chunk? oldChunk)) {
            throw new Exception("Chunk already exists!");
        }
        _loadedChunks[chunk.Coords] = chunk;
    }
    
}