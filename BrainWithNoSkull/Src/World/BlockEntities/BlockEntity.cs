using Silk.NET.Maths;

namespace BrainWithNoSkull.World.BlockEntities;

public class BlockEntity {

    public readonly BlockEntityType Type;

    public WorldBase World = null!;
    public Vector3D<int> BlockPosition;
    
    public BlockEntity(BlockEntityType type){
        Type = type;
    }
    
}

public interface ITickingBlockEntity {
    public void Tick();
}