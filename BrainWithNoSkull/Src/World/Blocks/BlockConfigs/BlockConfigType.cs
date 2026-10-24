using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Entities.Player;
using Silk.NET.Maths;

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

    public static uint SetPlaceDirection(uint config, BlockConfigContainer container, PlayerEntity player, Vector3D<int> blockPos){
        float xdiff = player.Position.X - (blockPos.X + 0.5f);
        float zdiff = player.Position.Z - (blockPos.Z + 0.5f);
        float angle = MathF.Atan2(xdiff, zdiff);
        return container.SetValue(config, Instance, Direction.FromAngle(angle));
    }
}

public class MultiblockBlockConfigType: BlockConfigType<Vector3D<uint>> {

    public static readonly MultiblockBlockConfigType Instance = new MultiblockBlockConfigType();
    
    protected MultiblockBlockConfigType() : base(9){
    }

    public override Vector3D<uint> GetValue(uint bits){
        uint x = bits & 0b111;
        uint y = (bits >> 3) & 0b111;
        uint z = (bits >> 6) & 0b111;
        return new Vector3D<uint>(x, y, z);
    }

    public override uint FromValue(Vector3D<uint> value){
        uint x = value.X & 0b111;
        uint y = (value.Y & 0b111) << 3;
        uint z = (value.Z & 0b111) << 6;
        return x | y | z;
    }

    public override Vector3D<uint> GetDefault(){
        return Vector3D<uint>.Zero;
    }
}