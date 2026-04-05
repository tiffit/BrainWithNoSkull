using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;

namespace BrainWithNoSkull.World.Entities;

public record EntityType(Type UnderlyingType, string Id, ushort Index, AABB Aabb, EntityFactory Factory) : IRegistryEntry{
    public string GetId(){
        return Id;
    }

    public ushort GetIndex(){
        return Index;
    }

    public virtual bool Equals(EntityType? other){
        return other != null && other.Index == Index;
    }

    public override int GetHashCode(){
        return Index.GetHashCode();
    }
}

public delegate Entity EntityFactory(WorldBase? world);

public class EntityTypeBuilder {

    private static ushort _nextIndex;
    
    private readonly Type _type;
    private readonly string _id;
    private readonly EntityFactory _factory;
    private readonly ushort _index;
    private AABB _aabb = new AABB(-0.5f, 0, -0.5f, 0.5f, 1, 0.5f);
    
    public EntityTypeBuilder(Type type, string id, EntityFactory factory){
        _type = type;
        _id = id;
        _factory = factory;
        _index = _nextIndex++;
    }

    public EntityTypeBuilder BoundingBox(AABB aabb){
        _aabb = aabb;
        return this;
    }

    public EntityType Build(){
        return new EntityType(_type, _id, _index, _aabb, _factory);
    }
}