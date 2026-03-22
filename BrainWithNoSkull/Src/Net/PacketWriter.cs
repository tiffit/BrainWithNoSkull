using Silk.NET.Maths;

namespace BrainWithNoSkull.Net;

public class PacketWriter : BinaryWriter{

    public PacketWriter(Stream output): base(output){}

    public void Write(Vector2D<int> vec){
        Write(vec.X);
        Write(vec.Y);
    }
    
}