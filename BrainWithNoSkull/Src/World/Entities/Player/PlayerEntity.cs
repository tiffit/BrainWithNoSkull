using BrainWithNoSkull.Registries;

namespace BrainWithNoSkull.World.Entities.Player;

public class PlayerEntity : Entity {
    public PlayerEntity(WorldBase? world) : base(EntityTypeRegistry.Player, world){
    }
}