using BrainWithNoSkull.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World;

public class Chunk {

    private WorldBase _world;
    private Vector2D<int> _coords;
    private Block[,,] _blocks;
    
    public Chunk(WorldBase world, Vector2D<int> chunkCoords, Block[,,] blocks){
        _world = world;
        _coords = chunkCoords;
        _blocks = blocks;
    }
    
}