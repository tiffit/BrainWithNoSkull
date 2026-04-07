using System.Collections.Concurrent;
using BrainWithNoSkull.Net;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Server;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.BlockEntities;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Entities;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class Chunk {

    private readonly ConcurrentQueue<Entity> _pendingAdd = new ConcurrentQueue<Entity>();
    private readonly WorldBase _world;
    public readonly Vector2D<int> Coords;
    private readonly Block[,,] _blocks;
    private readonly uint[,,] _blockConfigs;
    private readonly BlockEntity?[,,] _blockEntities;
    private readonly Dictionary<Vector3D<int>, ITickingBlockEntity> _tickingBlocks;
    private readonly Dictionary<uint, Entity> _entities = [];
    
    public Chunk(WorldBase world, Vector2D<int> chunkCoords, Block[,,] blocks, uint[,,] blockConfigs, BlockEntity?[,,] blockEntities, 
        Dictionary<Vector3D<int>, ITickingBlockEntity> tickingBlocks){
        _world = world;
        Coords = chunkCoords;
        _blocks = blocks;
        _blockConfigs = blockConfigs;
        _blockEntities = blockEntities;
        _tickingBlocks = tickingBlocks;
    }

    public void AddEntity(Entity entity){
        _pendingAdd.Enqueue(entity);
        entity.Chunk = this;
    }
    
    public void AddEntityNow(Entity entity){
        _entities[entity.Id] = entity;
    }
    
    public void RemoveEntityNow(uint id){
        _entities.Remove(id);
    }

    public IEnumerable<Entity> GetEntities(){
        return _entities.Values;
    }

    public uint GetBlockConfig(int x, int y, int z){
        if (OutsideChunkBounds(x, y, z)) {
            Console.WriteLine("Accessing block outside of chunk, returning air");
            return 0;
        }

        return _blockConfigs[x, y, z];
    }
    
    public Block GetBlock(int x, int y, int z){
        if (OutsideChunkBounds(x, y, z)) {
            Console.WriteLine("Accessing block outside of chunk, returning air");
            return BlockRegistry.Air;
        }
        return _blocks[x, y, z];
    }

    public void SetBlock(int x, int y, int z, Block block, uint config){
        if (OutsideChunkBounds(x, y, z)) {
            Console.WriteLine("Setting block outside of chunk");
        }

        _blocks[x, y, z] = block;
        _blockConfigs[x, y, z] = config;
        
        BlockEntity? old = _blockEntities[x, y, z];
        if (old is ITickingBlockEntity) {
            _tickingBlocks.Remove(new Vector3D<int>(x, y, z));
        }

        if (block is IBlockEntityProvider provider) {
            BlockEntity entity = provider.GetBlockEntity();
            entity.World = _world;
            entity.BlockPosition = new Vector3D<int>(x, y, z);
            _blockEntities[x, y, z] = entity;
            if (entity is ITickingBlockEntity ticking) {
                _tickingBlocks[entity.BlockPosition] = ticking;
            }
        }
        else {
            _blockEntities[x, y, z] = null;
        }
    }
    
    public BlockEntity? GetBlockEntity(int x, int y, int z){
        if (OutsideChunkBounds(x, y, z)) {
            Console.WriteLine("Accessing block entity outside of chunk, returning air");
            return null;
        }
        return _blockEntities[x, y, z];
    }

    public bool OutsideChunkBounds(int x, int y, int z){
        return x < 0 || y < 0 || z < 0 || x >= Constants.ChunkSize || y >= Constants.MaxWorldHeight || z >= Constants.ChunkSize;
    }

    public CbChunkPacket ToChunkPacket(){
        return new CbChunkPacket(_blocks, _blockConfigs, Coords);
    }

    public void Tick(){
        while (_pendingAdd.TryDequeue(out Entity? pending)) {
            AddEntityNow(pending);
        }
        
        foreach (Entity entity in _entities.Values) {
            entity.Tick();
            if (entity.Remove) {
                _world.EntityRemoveTickets.Enqueue(new EntityRemoveTicket(entity.Id));
                _entities.Remove(entity.Id);
                if (_world.OnServer) {
                    ServerWorld world = (_world as ServerWorld)!;
                    foreach (ConnectedClientBase client in world.GetAllListeners(Coords)) {
                        if (client.Entity != entity) {
                            client.SendPacket(new CbRemoveEntityPacket(entity.Id));
                        }
                    }
                }
            } else if (entity.IsPositionDirty) {
                Vector2D<int> newChunkCoords = ChunkUtils.ToChunkCoords(entity.Position);
                if (newChunkCoords != Coords) {
                    EntityMoveChunkTicket ticket = new EntityMoveChunkTicket(entity, this, newChunkCoords);
                    _world.EntityMoveChunkTickets.Enqueue(ticket);
                }

                if (_world.OnServer) {
                    ServerWorld world = (_world as ServerWorld)!;
                    foreach (ConnectedClientBase client in world.GetAllListeners(Coords)) {
                        if (client.Entity != entity) {
                            client.SendPacket(new CbEntityMovePacket(entity));
                        }
                    }
                }
            }
        }
        
        foreach ((Vector3D<int> pos, ITickingBlockEntity blockEntity) in _tickingBlocks) {
            blockEntity.Tick();
        }
    }
    
}