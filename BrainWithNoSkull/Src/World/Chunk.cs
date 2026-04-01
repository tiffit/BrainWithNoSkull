using System.Collections.Concurrent;
using BrainWithNoSkull.Net.Pckt;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.Entities;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class Chunk {

    private readonly ConcurrentQueue<Entity> _pendingAdd = new ConcurrentQueue<Entity>();
    private readonly WorldBase _world;
    public readonly Vector2D<int> Coords;
    private readonly Block[,,] _blocks;
    private readonly Dictionary<uint, Entity> _entities = [];
    
    public Chunk(WorldBase world, Vector2D<int> chunkCoords, Block[,,] blocks){
        _world = world;
        Coords = chunkCoords;
        _blocks = blocks;
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

    public Block GetBlock(int x, int y, int z){
        if (OutsideChunkBounds(x, y, z)) {
            Console.WriteLine("Accessing block outside of chunk, returning air");
            return BlockRegistry.Air;
        }
        return _blocks[x, y, z];
    }

    public void SetBlock(int x, int y, int z, Block block){
        if (OutsideChunkBounds(x, y, z)) {
            Console.WriteLine("Setting block outside of chunk");
        }

        _blocks[x, y, z] = block;
    }

    public bool OutsideChunkBounds(int x, int y, int z){
        return x < 0 || y < 0 || z < 0 || x >= Constants.ChunkSize || y >= Constants.MaxWorldHeight || z >= Constants.ChunkSize;
    }

    public CbChunkPacket ToChunkPacket(){
        return new CbChunkPacket(_blocks, Coords);
    }

    public void Tick(){
        while (_pendingAdd.TryDequeue(out Entity? pending)) {
            AddEntityNow(pending);
        }
        
        foreach (Entity entity in _entities.Values) {
            entity.Tick();
            if (entity.IsPositionDirty) {
                Vector2D<int> newChunkCoords = ChunkUtils.ToChunkCoords(entity.Position);
                if (newChunkCoords != Coords) {
                    EntityMoveChunkTicket ticket = new EntityMoveChunkTicket(entity, this, newChunkCoords);
                    _world.EntityMoveChunkTickets.Enqueue(ticket);
                }
            }
        }
    }
    
}