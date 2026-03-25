using System.Numerics;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Util;

public static class ChunkUtils {

    public static Vector2D<int> ToChunkCoords(Vector3D<float> position){
        return ToChunkCoords(new Vector3D<int>((int)MathF.Floor(position.X), 0, (int)MathF.Floor(position.Z)));
    }
    
    public static Vector2D<int> ToChunkCoords(Vector3D<int> position){
        return new Vector2D<int>(position.X >> 4, position.Z >> 4);
    }

    public static Vector3D<int> ToBlockInChunk(Vector3D<int> pos){
        return new Vector3D<int>(ToBlockInChunk(pos.X), pos.Y, ToBlockInChunk(pos.Z));
    }

    public static int ToBlockInChunk(int val){
        val %= 16;
        if (val < 0) {
            val = 16 + val;
        }
        return val;
    }
    
}