using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Net;

public class PacketWriter : BinaryWriter{

    public PacketWriter(Stream output): base(output){}

    public void Write(Vector2D<int> vec){
        Write(vec.X);
        Write(vec.Y);
    }
    
    public void Write(Vector3D<float> vec){
        Write(vec.X);
        Write(vec.Y);
        Write(vec.Z);
    }
    
    public void Write(Vector3D<int> vec){
        Write(vec.X);
        Write(vec.Y);
        Write(vec.Z);
    }

    public void Write(ItemStack? stack){
        if (ItemStack.IsNull(stack)) {
            Write(false);
        } else {
            Write(true);
            stack!.Write(this);
        }
    }
    
}