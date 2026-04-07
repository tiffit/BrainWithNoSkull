using BrainWithNoSkull.Registries;

namespace BrainWithNoSkull.World.BlockEntities;

public record BlockEntityType(Type UnderlyingType, string Id, ushort Index, BlockEntityFactory Factory) : IRegistryEntry{
    public string GetId(){
        return Id;
    }

    public ushort GetIndex(){
        return Index;
    }

    public virtual bool Equals(BlockEntityType? other){
        return other != null && other.Index == Index;
    }

    public override int GetHashCode(){
        return Index.GetHashCode();
    }

    public BlockEntity Create(){
        return Factory(this);
    }
}

public delegate BlockEntity BlockEntityFactory(BlockEntityType type);

public class BlockEntityTypeBuilder {

    private static ushort _nextIndex;
    
    private readonly Type _type;
    private readonly string _id;
    private readonly BlockEntityFactory _factory;
    private readonly ushort _index;
    
    public BlockEntityTypeBuilder(Type type, string id, BlockEntityFactory factory){
        _type = type;
        _id = id;
        _factory = factory;
        _index = _nextIndex++;
    }

    public BlockEntityType Build(){
        return new BlockEntityType(_type, _id, _index, _factory);
    }
}