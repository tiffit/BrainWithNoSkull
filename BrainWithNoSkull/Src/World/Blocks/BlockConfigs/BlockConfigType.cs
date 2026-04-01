using BrainWithNoSkull.Util;

namespace BrainWithNoSkull.World.Blocks.BlockConfigs;

public abstract class BaseBlockConfigType {
    private static ushort _nextId;
    public readonly ushort Id = _nextId++;
    
    public readonly byte Length;

    protected BaseBlockConfigType(byte length){
        Length = length;
    }

    public uint GetSection(uint packed, byte offset){
        return (uint)((packed >> offset) & ((1 << Length) - 1));
    }
}

public abstract class BlockConfigType<T> : BaseBlockConfigType{
    protected BlockConfigType(byte length): base(length){}

    public abstract T GetValue(uint bits);

    public abstract uint FromValue(T value);

    public abstract T GetDefault();
    
    public uint WriteSection(uint packed, byte offset, T value){
        uint clearFlag = (uint)((~((1 << Length) - 1)) << offset);
        packed &= clearFlag;

        uint val = FromValue(value);
        
        return packed | (val << offset);
    }
}

public class HorizontalFacingBlockConfigType: BlockConfigType<Direction> {

    public static readonly HorizontalFacingBlockConfigType Instance = new HorizontalFacingBlockConfigType();
    
    protected HorizontalFacingBlockConfigType() : base(2){
    }

    public override Direction GetValue(uint bits){
        return Direction.All[bits + 2];
    }

    public override uint FromValue(Direction value){
        return (uint)(value.Index - 2);
    }

    public override Direction GetDefault(){
        return Direction.North;
    }
}