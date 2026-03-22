using Silk.NET.Maths;

namespace BrainWithNoSkull.Net;

public class PacketReader : BinaryReader{
    
    public PacketReader(Stream input) : base(input){
    }

    public Vector2D<int> Read2DIntVec(){
        return new Vector2D<int>(ReadInt32(), ReadInt32());
    }
    
}