using BrainWithNoSkull.Registries;

namespace BrainWithNoSkull.World.Entities;

public record EntityType(Type UnderlyingType, string Id, ushort Index, EntityFactory Factory) : IRegistryEntry{
    public string GetId(){
        return Id;
    }

    public ushort GetIndex(){
        return Index;
    }
}

public delegate Entity EntityFactory(WorldBase? world);

public class EntityTypeBuilder {

    private static ushort _nextIndex;
    
    private readonly Type _type;
    private readonly string _id;
    private readonly EntityFactory _factory;
    private readonly ushort _index;
    
    public EntityTypeBuilder(Type type, string id, EntityFactory factory){
        _type = type;
        _id = id;
        _factory = factory;
        _index = _nextIndex++;
    }

    public EntityType Build(){
        return new EntityType(_type, _id, _index, _factory);
    }
}