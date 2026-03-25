using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Entities;
using BrainWithNoSkull.World.Gen;
using BrainWithNoSkull.World.Gen.Chunks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class WorldBase {

    private Queue<EntityMoveChunkTicket> _entityMoveChunkTickets = [];
    
    public WorldGenerator WorldGenerator{ get; private set; }
    protected ChunkStorage Chunks;
    public WorldSettings Settings{ get; private set; }
    public bool OnServer{ get; private set; }

    public WorldBase(WorldSettings settings, bool onServer){
        Settings = settings;
        OnServer = onServer;
        WorldGenerator = new WorldGenerator(this, new FlatChunkGenerator(settings));
        Chunks = new ChunkStorage(this);
    }
    
    public void AddEntity(Entity entity, Vector3D<float> position){
        Chunk chunk = GetChunk(ChunkUtils.ToChunkCoords(position))!;
        chunk.AddEntity(entity);
        entity.SetPosition(position);
    }

    public Chunk? GetChunk(Vector2D<int> chunkCoords){
        return Chunks.GetChunk(chunkCoords);
    }

    public virtual void Tick(double delta){
        Chunks.Tick(delta);
        while (_entityMoveChunkTickets.TryDequeue(out EntityMoveChunkTicket? moveChunkTicket)) {
            Chunk oldChunk = moveChunkTicket.OldChunk;
            Chunk newChunk = GetChunk(moveChunkTicket.NewChunk)!;
            oldChunk.RemoveEntityNow(moveChunkTicket.Entity.Id);
            newChunk.AddEntity(moveChunkTicket.Entity);
        }
    }
    
    public int WrapChunkCoord(int val){
        return Chunks.WrapChunkCoord(val);
    }

}