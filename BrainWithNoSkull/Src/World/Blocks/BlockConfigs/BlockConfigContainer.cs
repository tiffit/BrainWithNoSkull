namespace BrainWithNoSkull.World.Blocks.BlockConfigs;

public class BlockConfigContainer {

    private readonly Dictionary<ushort, byte> _offsets = new Dictionary<ushort, byte>();
    private byte _currentOffset = 0;
    private uint _initialValue = 0;
    
    public BlockConfigContainer(){}

    public BlockConfigContainer With<T>(BlockConfigType<T> type){
        return With(type, type.GetDefault());
    }
    
    public BlockConfigContainer With<T>(BlockConfigType<T> type, T defaultValue){
        _offsets[type.Id] = _currentOffset;
        _currentOffset += type.Length;
        _initialValue = type.FromValue(defaultValue);
        return this;
    }

    public T GetValue<T>(uint packed, BlockConfigType<T> type){
        byte offset = _offsets[type.Id];
        return type.GetValue(type.GetSection(packed, offset));
    }

    public uint SetValue<T>(uint packed, BlockConfigType<T> type, T value){
        byte offset = _offsets[type.Id];
        return type.WriteSection(packed, offset, value);
    }

}