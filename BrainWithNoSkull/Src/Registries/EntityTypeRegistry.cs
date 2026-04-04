using System.Diagnostics.CodeAnalysis;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Entities;
using BrainWithNoSkull.World.Entities.Player;

#pragma warning disable CS8618

namespace BrainWithNoSkull.Registries;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public class EntityTypeRegistry {
 
    public static readonly Registry<EntityType> Registry = new Registry<EntityType>();
    
    public static EntityType Player;
    public static EntityType ItemStack;
    
    public static void Init(){
        Player = Registry.Register(new EntityTypeBuilder(typeof(PlayerEntity), "player", world => new PlayerEntity(world))
            .BoundingBox(new AABB(-0.4f, 0, -0.4f, 0.4f, 1.5f, 0.4f)).Build());
        ItemStack = Registry.Register(new EntityTypeBuilder(typeof(ItemStackEntity), "item", world => new ItemStackEntity(world))
            .BoundingBox(new AABB(-0.3f, -0.3f, -0.3f, 0.3f, 0.3f, 0.3f)).Build());
    }
    
}