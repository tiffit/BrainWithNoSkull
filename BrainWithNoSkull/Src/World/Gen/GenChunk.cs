using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.BlockEntities;
using BrainWithNoSkull.World.Blocks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen;

public class GenChunk {
    public readonly WorldBase World;
    public readonly Block[,,] Blocks;
    public readonly uint[,,] BlockConfigs;
    public readonly byte[,,] Light;
    public readonly Dictionary<Vector3D<int>, BlockEntity> BlockEntities = new Dictionary<Vector3D<int>, BlockEntity>();
    public readonly Dictionary<Vector3D<int>, ITickingBlockEntity> TickingBlocks = new Dictionary<Vector3D<int>, ITickingBlockEntity>();
    private readonly int _size;
    private readonly Vector2D<int> _chunkCoords;
    
    public GenChunk(WorldBase world, Vector2D<int> chunkCoords, int size){
        World = world;
        _chunkCoords = chunkCoords;
        _size = size;
        Blocks = new Block[size, Constants.MaxWorldHeight, size];
        BlockConfigs = new uint[size, Constants.MaxWorldHeight, size];
        Light = new byte[size, Constants.MaxWorldHeight, size];
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < Constants.MaxWorldHeight; y++) {
                for (int z = 0; z < size; z++) {
                    Blocks[x, y, z] = BlockRegistry.Air;
                }
            }
        }
    }

    public void SetBlock(int x, int y, int z, Block block){
        if (OutOfBounds(x, y, z)) {
            return;
        }
        
        Blocks[x, y, z] = block;
        
        //TODO: Copy-pasted from chunk, find some way to combine the two, add multiblock check
        Vector3D<int> pos = new Vector3D<int>(x, y, z);
        BlockEntities.TryGetValue(pos, out BlockEntity? old);
        if (old is ITickingBlockEntity) {
            TickingBlocks.Remove(new Vector3D<int>(x, y, z));
        }

        if (block is IBlockEntityProvider provider) {
            BlockEntity entity = provider.GetBlockEntity();
            entity.World = World;
            entity.BlockPosition = pos;
            BlockEntities[pos] = entity;
            if (entity is ITickingBlockEntity ticking) {
                TickingBlocks[pos] = ticking;
            }
        }
        else {
            BlockEntities.Remove(pos);
        }
    }

    public static bool OutOfBounds(int x, int y, int z){
        return x < 0 || y < 0 || z < 0 || x >= Constants.ChunkSize || y >= Constants.MaxWorldHeight || z >= Constants.ChunkSize;
    }

    public Chunk ToChunk(){
        Chunk chunk = new Chunk(World, _chunkCoords, Blocks, BlockConfigs, Light, BlockEntities, TickingBlocks);
        return chunk;
    }
}