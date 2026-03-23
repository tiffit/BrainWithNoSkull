using System.Collections.Concurrent;
using BrainWithNoSkull.Net.Pckt;
using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Registries;
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

    public Block GetBlock(int x, int y, int z){
        if (x < 0 || y < 0 || z < 0 || x >= Constants.ChunkSize || y >= Constants.MaxWorldHeight || z >= Constants.ChunkSize) {
            Console.WriteLine("Accessing block outside of chunk, returning air");
            return BlockRegistry.Air;
        }
        return _blocks[x, y, z];
    }

    public CbChunkPacket ToChunkPacket(){
        return new CbChunkPacket(_blocks, Coords);
    }

    public void Tick(){
        while (_pendingAdd.TryDequeue(out Entity? pending)) {
            _entities[pending.Id] = pending;
        }
        
        foreach (Entity entity in _entities.Values) {
            entity.Tick();
        }
    }
    
}