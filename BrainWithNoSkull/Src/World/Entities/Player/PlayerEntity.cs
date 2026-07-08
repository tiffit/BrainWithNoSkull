using BrainWithNoSkull.Commands.Components;
using BrainWithNoSkull.Registries;

namespace BrainWithNoSkull.World.Entities.Player;

public class PlayerEntity : Entity, ICommandExecutor {

    public PlayerInventory Inventory;
    
    public PlayerEntity(WorldBase? world) : base(EntityTypeRegistry.Player, world){
        Inventory = new PlayerInventory();
    }

    public override void TickPhysics(){}
}