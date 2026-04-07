using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;

namespace BrainWithNoSkull.Net;

public record ContainerSyncType(Type UnderlyingType, string Id, ushort Index, ContainerSyncFactory Factory) : IRegistryEntry{
    public string GetId(){
        return Id;
    }

    public ushort GetIndex(){
        return Index;
    }

    public virtual bool Equals(ContainerSyncType? other){
        return other != null && other.Index == Index;
    }

    public override int GetHashCode(){
        return Index.GetHashCode();
    }
}

public delegate ContainerSync ContainerSyncFactory(uint id, IItemContainer container, PlayerEntity player);

public class ContainerSyncTypeBuilder {

    private static ushort _nextIndex;
    
    private readonly Type _type;
    private readonly string _id;
    private readonly ContainerSyncFactory _factory;
    private readonly ushort _index;
    
    public ContainerSyncTypeBuilder(Type type, string id, ContainerSyncFactory factory){
        _type = type;
        _id = id;
        _factory = factory;
        _index = _nextIndex++;
    }

    public ContainerSyncType Build(){
        return new ContainerSyncType(_type, _id, _index, _factory);
    }
}

public class ContainerSync {

    public readonly uint Id;
    protected readonly IItemContainer Container;
    protected readonly PlayerEntity Player;
    
    public ContainerSync(uint id, IItemContainer container, PlayerEntity player){
        Id = id;
        Container = container;
        Player = player;
    }
    
}