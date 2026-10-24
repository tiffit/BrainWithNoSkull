using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World.BlockEntities;

public interface IConveyor {
    public bool ConveyorModified(out ItemStack? stack, out float progress);
}

public class ConveyorBlockEntity: BlockEntity, ITickingBlockEntity, IConveyor {

    public ItemStack? Stack;
    public float Progress;
    
    public ConveyorBlockEntity(BlockEntityType type) : base(type){
    }

    public void Tick(){
    }

    public bool ConveyorModified(out ItemStack? stack, out float progress){
        stack = null;
        progress = 0;
        return false;
    }
}