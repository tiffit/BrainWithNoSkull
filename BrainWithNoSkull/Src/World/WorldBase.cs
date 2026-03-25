using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Entities;
using BrainWithNoSkull.World.Gen;
using BrainWithNoSkull.World.Gen.Chunks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class WorldBase {

    private readonly Queue<EntityMoveChunkTicket> _entityMoveChunkTickets = [];
    
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

    public Block GetBlock(Vector3D<int> blockPos){
        Vector2D<int> chunkCoords = ChunkUtils.ToChunkCoords(blockPos);
        if (!Chunks.IsChunkLoaded(chunkCoords))
            return BlockRegistry.Air;
        Chunk? chunk = Chunks.GetChunk(chunkCoords);
        blockPos = ChunkUtils.ToBlockInChunk(blockPos);
        return chunk == null ? BlockRegistry.Air : chunk.GetBlock(blockPos.X, blockPos.Y, blockPos.Z);
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

    public IEnumerable<Vector3D<int>> GetBlocksInBox(AABB aabb){
        int minX = (int)MathF.Floor(aabb.MinX);
        int minY = (int)MathF.Floor(aabb.MinY);
        int minZ = (int)MathF.Floor(aabb.MinZ);
        int maxX = (int)MathF.Floor(aabb.MaxX);
        int maxY = (int)MathF.Floor(aabb.MaxY);
        int maxZ = (int)MathF.Floor(aabb.MaxZ);

        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                for (int z = minZ; z <= maxZ; z++) {
                    yield return new Vector3D<int>(x, y, z);
                }
            }
        }
    }
    
    public int WrapChunkCoord(int val){
        return Chunks.WrapChunkCoord(val);
    }

}