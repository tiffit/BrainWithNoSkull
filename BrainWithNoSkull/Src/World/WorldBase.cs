using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.World.Gen;
using BrainWithNoSkull.World.Gen.Chunks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class WorldBase {

    public WorldGenerator WorldGenerator{ get; private set; }
    protected ChunkStorage Chunks;
    public WorldSettings Settings{ get; private set; }
    public bool Remote{ get; private set; }

    public WorldBase(WorldSettings settings, bool remote){
        Settings = settings;
        Remote = remote;
        WorldGenerator = new WorldGenerator(this, new FlatChunkGenerator(settings));
        Chunks = new ChunkStorage(this);
    }

    public Chunk? GetChunk(Vector2D<int> chunkCoords){
        return Chunks.GetChunk(chunkCoords);
    }

    public virtual void Tick(double delta){
        Chunks.Tick(delta);
    }

}