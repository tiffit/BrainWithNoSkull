using BrainWithNoSkull.Registries;

namespace BrainWithNoSkull.World.Entities.Player;

public class PlayerEntity : Entity {

    public PlayerInventory Inventory;
    
    public PlayerEntity(WorldBase? world) : base(EntityTypeRegistry.Player, world){
        Inventory = new PlayerInventory();
    }

    public override void TickPhysics(){}
}