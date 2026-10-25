using BrainWithNoSkull.Net.Pckt.Cb;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Blocks.BlockConfigs;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.BlockEntities;

public interface IConveyor {
    public ConveyorSyncEntry? GetSyncEntry();
    public void FromSyncEntry(ConveyorSyncEntry entry);

    public bool HasItem();

    public void AddItem(Direction from, ItemStack stack);

    public Direction GetOutputDirection();
}

public class ConveyorBlockEntity: BlockEntity, ITickingBlockEntity, IConveyor {

    public ItemStack? Stack;
    public float Progress;
    private bool _modified;
    
    public ConveyorBlockEntity(BlockEntityType type) : base(type){
    }

    public void Tick(){
        if(!World.OnServer)return;
        Direction dir = GetOutputDirection();
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
            if (Progress < 1) {
                _modified = true;
                Progress += 0.05f;
            }
            if (Progress >= 1) {
                Progress = 1;
                
                Vector3D<int> push = World.GetMultiblockOrigin(BlockPosition + dir.Offset);
                BlockEntity? pushBe = World.GetBlockEntity(push);

                switch (pushBe) {
                    case IConveyor conveyor: {
                        if (conveyor.GetOutputDirection() != dir && !conveyor.HasItem()) {
                            conveyor.AddItem(dir.GetOpposite(), Stack);
                            Stack = null;
                            Progress = 0;
                            _modified = true;
                        }
                        break;
                    }
                    case IContainerBlockEntity cbe: {
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
                        break;
                    }
                }
            }
        }
    }

    public ConveyorSyncEntry? GetSyncEntry(){
        if (_modified) {
            _modified = false;
            int loc = ChunkUtils.PackInChunk(ChunkUtils.ToBlockInChunk(BlockPosition));
            return new ConveyorSyncEntry(loc, Stack?.Clone() ?? null, Progress);
        }
        return null;
    }

    public void FromSyncEntry(ConveyorSyncEntry entry){
        Stack = entry.Stack;
        Progress = entry.Progress;
    }

    public bool HasItem(){
        return Stack != null;
    }

    public void AddItem(Direction from, ItemStack stack){
        Direction output = GetOutputDirection();
        Stack = stack;
        _modified = true;
        if (from == output) {
            Progress = 1f;
        }else if (from == output.GetOpposite()) {
            Progress = 0f;
        }
        else {
            Progress = 0.5f;
        }
    }

    public Direction GetOutputDirection(){
        World.GetBlock(BlockPosition, out uint config);
        return BlockRegistry.Conveyor.ConfigContainer.GetValue(config, HorizontalFacingBlockConfigType.Instance);
    }
}