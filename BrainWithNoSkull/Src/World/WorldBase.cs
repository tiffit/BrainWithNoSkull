using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.BlockEntities;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using BrainWithNoSkull.World.Entities;
using BrainWithNoSkull.World.Gen;
using BrainWithNoSkull.World.Gen.Chunks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class WorldBase {
    public readonly Queue<EntityMoveChunkTicket> EntityMoveChunkTickets = [];
    public readonly Queue<EntityRemoveTicket> EntityRemoveTickets = [];

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
        entity.SetPositionInstant(position);
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
        SetBlock(blockPos, block, block.ConfigContainer.GetDefault());
    }
    
    public virtual void SetBlock(Vector3D<int> blockPos, Block block, uint config){
        Vector2D<int> chunkCoords = ChunkUtils.ToChunkCoords(blockPos);
        if (!Chunks.IsChunkLoaded(chunkCoords))
            return;
        Chunk? chunk = Chunks.GetChunk(chunkCoords);
        blockPos = ChunkUtils.ToBlockInChunk(blockPos);
        chunk?.SetBlock(blockPos.X, blockPos.Y, blockPos.Z, block, config);
    }
    
    public virtual void RemoveBlock(Vector3D<int> blockPos){
        Block block = GetBlock(blockPos, out uint config);
        if (block is IMultiblock multiblock) {
            Vector3D<int> size = multiblock.GetMultiblockSize();
            Vector3D<int> origin = GetMultiblockOrigin(blockPos);
            for (int x = 0; x < size.X; x++) {
                for (int y = 0; y < size.Y; y++) {
                    for (int z = 0; z < size.Z; z++) {
                        SetBlock(new Vector3D<int>(origin.X + x, origin.Y + y, origin.Z + z), BlockRegistry.Air);
                    }
                }
            }
        }
        else {
            SetBlock(blockPos, BlockRegistry.Air);
        }
    }
    
    public BlockEntity? GetBlockEntity(Vector3D<int> blockPos){
        Vector2D<int> chunkCoords = ChunkUtils.ToChunkCoords(blockPos);
        if (!Chunks.IsChunkLoaded(chunkCoords))
            return null;
        Chunk? chunk = Chunks.GetChunk(chunkCoords);
        blockPos = ChunkUtils.ToBlockInChunk(blockPos);
        if (blockPos.Y is < 0 or > Constants.MaxWorldHeight || chunk == null)
            return null;

        return chunk.GetBlockEntity(blockPos.X, blockPos.Y, blockPos.Z);
    }

    public Vector3D<int> GetMultiblockOrigin(Vector3D<int> blockPos){
        Block block = GetBlock(blockPos, out uint config);
        if (block is IMultiblock multiblock) {
            Vector3D<uint> offset = block.ConfigContainer.GetValue(config, MultiblockBlockConfigType.Instance);
            return new Vector3D<int>(blockPos.X - (int)offset.X, blockPos.Y - (int)offset.Y, blockPos.Z - (int)offset.Z);
        }
        return blockPos;
    }

    public virtual void PlaceBlock(Vector3D<int> blockPos, Block block){
        if (block is IMultiblock multiblock) {
            Vector3D<int> size = multiblock.GetMultiblockSize();
            for (uint x = 0; x < size.X; x++) {
                for (uint y = 0; y < size.Y; y++) {
                    for (uint z = 0; z < size.Z; z++) {
                        uint config = block.ConfigContainer.SetValue(0, MultiblockBlockConfigType.Instance, new Vector3D<uint>(x, y, z));
                        SetBlock(blockPos + new Vector3D<int>((int)x, (int)y, (int)z), block, config);
                    }
                }
            }
        }
        else {
            SetBlock(blockPos, block);
        }
    }

    public virtual void Tick(double delta){
        Chunks.Tick(delta);
        while (EntityMoveChunkTickets.TryDequeue(out EntityMoveChunkTicket? moveChunkTicket)) {
            Chunk oldChunk = moveChunkTicket.OldChunk;
            Chunk newChunk = GetChunk(moveChunkTicket.NewChunk)!;
            oldChunk.RemoveEntityNow(moveChunkTicket.Entity.Id);
            newChunk.AddEntity(moveChunkTicket.Entity);
        }

        while (EntityRemoveTickets.TryDequeue(out EntityRemoveTicket? removeTicket)) {
            _entities.Remove(removeTicket.Id);
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