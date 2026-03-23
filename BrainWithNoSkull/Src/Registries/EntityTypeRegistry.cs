using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.World.Entities;
using BrainWithNoSkull.World.Entities.Player;

#pragma warning disable CS8618

namespace BrainWithNoSkull.Registries;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public class EntityTypeRegistry {
 
    public static readonly Registry<EntityType> Registry = new Registry<EntityType>();
    
    public static EntityType Player;
    
    public static void Init(){
        Player = Registry.Register(new EntityTypeBuilder(typeof(PlayerEntity), "player", world => new PlayerEntity(world)).Build());
    }
    
}