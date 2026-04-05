using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.World.Items;

public class Item : IRegistryEntry{
    
    public static ushort NextIndex;
    private readonly ushort _index;
    private readonly string _id;
    
    //Properties
    public float BreakSpeed = 1f;
    
    public Item(string id){
        _id = id;
        _index = NextIndex++;
    }
    
    public string GetId(){
        return _id;
    }

    public ushort GetIndex(){
        return _index;
    }

    public override string ToString(){
        return _id;
    }

    public virtual void Interact(WorldRayResult result, WorldBase world, PlayerEntity player, ItemStack stack){
        
    }
}