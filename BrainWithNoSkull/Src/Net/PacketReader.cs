using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Blocks;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net;

public class PacketReader : BinaryReader{
    
    public PacketReader(Stream input) : base(input){
    }

    public Vector2D<int> Read2DIntVec(){
        return new Vector2D<int>(ReadInt32(), ReadInt32());
    }
    
    public Vector3D<float> Read3DFloatVec(){
        return new Vector3D<float>(ReadSingle(), ReadSingle(), ReadSingle());
    }
    
    public Vector3D<int> Read3DIntVec(){
        return new Vector3D<int>(ReadInt32(), ReadInt32(), ReadInt32());
    }

    public ItemStack? ReadItemStack(){
        return ReadBoolean() ? ItemStack.Read(this) : null;
    }

    public WorldRayResult ReadWorldRayResult(){
        Vector3D<int> pos = Read3DIntVec();
        Direction dir = Direction.All[ReadByte()];
        Block block = BlockRegistry.Registry.GetEntry(ReadUInt16())!;
        return new WorldRayResult{
            Block = block,
            Direction = dir,
            Pos = pos
        };
    }
    
}