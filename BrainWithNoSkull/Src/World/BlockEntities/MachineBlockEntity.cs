using BrainWithNoSkull.Net;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;
using BrainWithNoSkull.World.Recipes;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.BlockEntities;

public enum FuelType {
    None,
    Solid,
    Electricity
}

public enum MachineTier : byte {
    None,
    Crude,
    Steel
}

public class MachineContainerSync: ContainerSync {

    private readonly MachineBlockEntity _blockEntity = null!;

    public readonly SyncVariable<Vector3D<int>> MachinePos;
    public readonly SyncVariable<float> MachineProgress;
    
    public MachineContainerSync(uint id, MachineBlockEntity blockEntity, ServerPlayerEntity player) : base(id, blockEntity.Container, player){
        _blockEntity = blockEntity;
        MachinePos = AddSyncVariable(new Vector3DIntSyncVariable(Vector3D<int>.Zero, () => _blockEntity.BlockPosition));
        MachineProgress = AddSyncVariable(new FloatSyncVariable(0, _blockEntity.GetProgress));
    }
    
    public MachineContainerSync(uint id, IItemContainer container, PlayerEntity player) : base(id, container, player){
        MachinePos = AddSyncVariable(new Vector3DIntSyncVariable(Vector3D<int>.Zero, null));
        MachineProgress = AddSyncVariable(new FloatSyncVariable(0, null));
    }
    
}

public class MachineContainer: IItemContainer {

    public readonly List<ItemStack?> Inputs = [];
    public readonly List<ItemStack?> Outputs = [];
    
    public ItemStack? GetItem(int slot){
        return slot >= Inputs.Count ? Outputs[slot - Inputs.Count] : Inputs[slot];
    }

    public int GetInventorySize(){
        return Inputs.Count + Outputs.Count;
    }

    public void SetItemInSlot(ItemStack? stack, int slot){
        if (slot >= Inputs.Count) {
            Outputs[slot - Inputs.Count] = stack;
        }
        else {
            Inputs[slot] = stack;
        }
    }

    public void MarkSlotDirty(int slot){
        
    }

    public IItemContainer GetInputContainer(){
        return new ListItemContainer(this, 0, Inputs);
    }
    
    public IItemContainer GetOutputContainer(){
        return new ListItemContainer(this, Inputs.Count, Outputs);
    }
    
}

public class MachineBlockEntity: BlockEntity, ITickingBlockEntity {

    public readonly RecipeType RecipeType;
    public readonly FuelType FuelType;
    public readonly MachineTier Tier;
    public readonly bool AutoDetect;
    private readonly RecipeLookup? _lookup;

    public int AutoInputSize = 1;
    public int AutoOutputSize = 1;

    public readonly MachineContainer Container = new MachineContainer();

    public Recipe? CurrentRecipe;
    private float _craftTime = 0;
    private bool _craftStarted = false;
    
    public MachineBlockEntity(BlockEntityType type, RecipeType recipeType, FuelType fuelType, MachineTier tier, bool autoDetect) : base(type){
        RecipeType = recipeType;
        FuelType = fuelType;
        Tier = tier;
        AutoDetect = autoDetect;

        if (autoDetect) {
            Container.Inputs.Clear();
            Container.Outputs.Clear();
            for (int i = 0; i < AutoInputSize; i++) Container.Inputs.Add(null);
            for (int i = 0; i < AutoOutputSize; i++) Container.Outputs.Add(null);
            
            _lookup = RecipeRegistry.Lookups[RecipeType];
        }
    }

    public int GetInputSlotCount(){
        return AutoInputSize;
    }

    public int GetOutputSlotCount(){
        return AutoOutputSize;
    }
    
    public void Tick(){
        if (World.OnServer) {
            if (CurrentRecipe == null) {
                if (AutoDetect && _lookup != null) {
                    IItemContainer inputContainer = Container.GetInputContainer();
                    CurrentRecipe = _lookup.Lookup(inputContainer);
                    if (CurrentRecipe != null) {
                        CurrentRecipe.ConsumeIngredients(inputContainer);
                        _craftTime = 0;
                        _craftStarted = true;
                    }
                }
            }
            else {
                if (_craftStarted) {
                    _craftTime = Math.Min(_craftTime + 1f / Constants.TicksPerSecond, CurrentRecipe.Time);
                    if (_craftTime >= CurrentRecipe.Time) {
                        IItemContainer outputContainer = Container.GetOutputContainer();
                        if (CurrentRecipe.CanOutput(outputContainer)) {
                            CurrentRecipe.ProduceOutput(outputContainer);
                            if (AutoDetect) {
                                CurrentRecipe = null;
                            }
                            _craftStarted = false;
                        }

                    }
                }
                else {
                    
                }
            }
        }
    }

    public float GetProgress(){
        if (CurrentRecipe == null)
            return 0;
        return _craftTime / CurrentRecipe.Time;
    }
}