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
    public readonly Queue<EntityMoveChunkTicket> EntityMoveChunkTickets = [];

    private Dictionary<uint, Entity> _entities = new Dictionary<uint, Entity>();

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

    public virtual void AddEntity(Entity entity, Vector3D<float> position){
        Chunk chunk = GetChunk(ChunkUtils.ToChunkCoords(position))!;
        chunk.AddEntity(entity);
        entity.SetPosition(position);
        _entities[entity.Id] = entity;
    }

    public Entity? GetEntity(uint id){
        _entities.TryGetValue(id, out Entity? value);
        return value;
    }

    public Chunk? GetChunk(Vector2D<int> chunkCoords){
        return Chunks.GetChunk(chunkCoords);
    }

    public Block GetBlock(Vector3D<int> blockPos){
        return GetBlock(blockPos, out uint _);
    }
    
    public Block GetBlock(Vector3D<int> blockPos, out uint config){
        config = 0;
        Vector2D<int> chunkCoords = ChunkUtils.ToChunkCoords(blockPos);
        if (!Chunks.IsChunkLoaded(chunkCoords))
            return BlockRegistry.Air;
        Chunk? chunk = Chunks.GetChunk(chunkCoords);
        blockPos = ChunkUtils.ToBlockInChunk(blockPos);
        if (blockPos.Y is < 0 or > Constants.MaxWorldHeight || chunk == null)
            return BlockRegistry.Air;
        
        config = chunk.GetBlockConfig(blockPos.X, blockPos.Y, blockPos.Z);
        return chunk.GetBlock(blockPos.X, blockPos.Y, blockPos.Z);
    }

    public virtual void SetBlock(Vector3D<int> blockPos, Block block){
        Vector2D<int> chunkCoords = ChunkUtils.ToChunkCoords(blockPos);
        if (!Chunks.IsChunkLoaded(chunkCoords))
            return;
        Chunk? chunk = Chunks.GetChunk(chunkCoords);
        blockPos = ChunkUtils.ToBlockInChunk(blockPos);
        chunk?.SetBlock(blockPos.X, blockPos.Y, blockPos.Z, block);
    }

    public virtual void Tick(double delta){
        Chunks.Tick(delta);
        while (EntityMoveChunkTickets.TryDequeue(out EntityMoveChunkTicket? moveChunkTicket)) {
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

    public IEnumerable<Entity> GetEntitiesInBox(AABB aabb){
        Vector2D<int> minChunk = ChunkUtils.ToChunkCoords(new Vector3D<float>(aabb.MinX, aabb.MinY, aabb.MinZ));
        Vector2D<int> maxChunk = ChunkUtils.ToChunkCoords(new Vector3D<float>(aabb.MaxX, aabb.MaxY, aabb.MaxZ));


        for (int x = minChunk.X; x <= maxChunk.X; x++) {
            for (int z = minChunk.Y; z <= maxChunk.Y; z++) {
                Vector2D<int> chunkCoords = new Vector2D<int>(x, z);
                Chunk? chunk = GetChunk(chunkCoords);
                if (chunk != null) {
                    foreach (Entity entity in chunk.GetEntities()) {
                        if (entity.GetBoundingBox().Intersects(aabb)) {
                            yield return entity;   
                        }
                    }
                }
            }
        }
    }

    public int WrapChunkCoord(int val){
        return Chunks.WrapChunkCoord(val);
    }
}