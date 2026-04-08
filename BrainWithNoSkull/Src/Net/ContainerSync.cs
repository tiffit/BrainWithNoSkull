using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

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
    public readonly IItemContainer Container;
    protected readonly PlayerEntity Player;

    private ushort _nextSyncVariableId;
    private readonly Dictionary<ushort, ISyncVariable> _syncVariables = new Dictionary<ushort, ISyncVariable>();
    private readonly ItemStack?[] _itemCache;
    
    public ContainerSync(uint id, IItemContainer container, PlayerEntity player){
        Id = id;
        Container = container;
        Player = player;
        _itemCache = new ItemStack[container.GetInventorySize()];
    }

    protected SyncVariable<T> AddSyncVariable<T>(SyncVariable<T> variable){
        variable.Id = _nextSyncVariableId++;
        _syncVariables[variable.Id] = variable;
        return variable;
    }

    public byte[] GetDirtySyncVariables(){
        using MemoryStream stream = new MemoryStream();
        using PacketWriter writer = new PacketWriter(stream);
        List<ISyncVariable> variables = _syncVariables.Values.Where(variable => variable.ShouldUpdate()).ToList();
        writer.Write((byte)variables.Count);
        foreach (ISyncVariable var in variables) {
            writer.Write(var.GetId());
            var.WritePacket(writer);
        }
        
        if (stream.TryGetBuffer(out ArraySegment<byte> segment)) {
            byte[] result = new byte[segment.Count];
            segment.CopyTo(result);
            return result;
        }
        return [0];
    }
    
    public void UpdateSyncVariables(byte[] arr){
        using MemoryStream stream = new MemoryStream(arr);
        using PacketReader reader = new PacketReader(stream);

        int count = reader.ReadByte();
        for (int i = 0; i < count; i++) {
            ushort id = reader.ReadUInt16();
            if (_syncVariables.TryGetValue(id, out ISyncVariable? variable)) {
                variable.ReadPacket(reader);
            }
        }
    }

    public List<Tuple<int, ItemStack?>> GetDirtyItemStacks(){
        List<Tuple<int, ItemStack?>> list = [];
        for (int i = 0; i < _itemCache.Length; i++) {
            ItemStack? realItem = Container.GetItem(i);
            if (!ItemStack.AreEqual(_itemCache[i], realItem)) {
                _itemCache[i] = realItem?.Clone();
                list.Add(new Tuple<int, ItemStack?>(i, realItem));
            }
        }
        return list;
    }
}

internal interface ISyncVariable {
    bool ShouldUpdate();
    void WritePacket(PacketWriter writer);
    void ReadPacket(PacketReader reader);

    ushort GetId();
}

public abstract class SyncVariable<T>: ISyncVariable {

    private T _value;
    private readonly Func<T>? _getter;
    public ushort Id;
    public event Action<T>? OnSync;

    protected SyncVariable(T value, Func<T>? getter){
        _value = value;
        _getter = getter;
    }

    public ushort GetId() => Id;

    protected abstract bool AreValuesEqual(T val1, T val2);

    protected abstract void WriteValueToPacket(PacketWriter writer, T value);
    protected abstract T GetValueFromPacket(PacketReader reader);

    public void WritePacket(PacketWriter writer){
        WriteValueToPacket(writer, _value);
    }
    
    public void ReadPacket(PacketReader reader){
        _value = GetValueFromPacket(reader);
        OnSync?.Invoke(_value);
    }

    public bool ShouldUpdate(){
        if (_getter == null)
            return false;
        T other = _getter();
        if (!AreValuesEqual(_value, other)) {
            _value = other;
            return true;
        }
        return false;
    }
}

public class Vector3DIntSyncVariable : SyncVariable<Vector3D<int>> {
    public Vector3DIntSyncVariable(Vector3D<int> value, Func<Vector3D<int>>? getter) : base(value, getter){}
    protected override bool AreValuesEqual(Vector3D<int> val1, Vector3D<int> val2) => val1 == val2;
    protected override void WriteValueToPacket(PacketWriter writer, Vector3D<int> value) => writer.Write(value);
    protected override Vector3D<int> GetValueFromPacket(PacketReader reader) => reader.Read3DIntVec();
}

public class FloatSyncVariable : SyncVariable<float> {
    public FloatSyncVariable(float value, Func<float>? getter) : base(value, getter){}
    protected override bool AreValuesEqual(float val1, float val2) => val1 == val2;
    protected override void WriteValueToPacket(PacketWriter writer, float value) => writer.Write(value);
    protected override float GetValueFromPacket(PacketReader reader) => reader.ReadSingle();
}