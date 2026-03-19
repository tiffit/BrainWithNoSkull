using BrainWithNoSkull.World.Gen;
using BrainWithNoSkull.World.Gen.Chunks;

namespace BrainWithNoSkull.World;

public class WorldBase {

    public WorldGenerator WorldGenerator{ get; private set; }
    private ChunkStorage _chunkStorage;

    public WorldBase(){
        WorldGenerator = new WorldGenerator(this, new FlatChunkGenerator());
        _chunkStorage = new ChunkStorage(this);
    }

    public virtual void Tick(){
        
    }

}