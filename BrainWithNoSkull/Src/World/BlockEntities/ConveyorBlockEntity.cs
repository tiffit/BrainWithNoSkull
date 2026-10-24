using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.BlockEntities;

public interface IConveyor {
    public bool ConveyorModified(out ItemStack? stack, out float progress);
}

public class ConveyorBlockEntity: BlockEntity, ITickingBlockEntity, IConveyor {

    public ItemStack? Stack;
    public float Progress;
    private bool _modified = false;
    
    public ConveyorBlockEntity(BlockEntityType type) : base(type){
    }

    public void Tick(){
        World.GetBlock(BlockPosition, out uint config);
        Direction dir = BlockRegistry.Conveyor.ConfigContainer.GetValue(config, HorizontalFacingBlockConfigType.Instance);
        Vector3D<int> pull = World.GetMultiblockOrigin(BlockPosition + dir.GetOpposite().Offset);
        BlockEntity? pullBe = World.GetBlockEntity(pull);
        if (Stack == null) {
            if (pullBe is IContainerBlockEntity cbe) {
                IItemContainer container = cbe.GetOutputContainer();
                for (int i = 0; i < container.GetInventorySize(); i++) {
                    ItemStack? stack = container.GetItem(i);
                    if (stack != null) {
                        Stack = stack.Clone();
                        container.SetItemInSlot(null, i);
                        container.MarkSlotDirty(i);
                        _modified = true;
                        break;
                    }
                }
            }
        }

        if (Stack != null) {
            _modified = true;
            Progress += 0.05f;
            if (Progress > 1) {
                Progress = 1;
                
                Vector3D<int> push = World.GetMultiblockOrigin(BlockPosition + dir.Offset);
                BlockEntity? pushBe = World.GetBlockEntity(push);
                
                if (pushBe is IContainerBlockEntity cbe) {
                    IItemContainer container = cbe.GetInputContainer();
                    for (int i = 0; i < container.GetInventorySize(); i++) {
                        if (container.CombineStacks(i, ref Stack, false)) {
                            container.MarkSlotDirty(i);
                            _modified = true;
                            if (Stack == null) {
                                Progress = 0;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public bool ConveyorModified(out ItemStack? stack, out float progress){
        stack = null;
        progress = 0;
        return false;
    }
}