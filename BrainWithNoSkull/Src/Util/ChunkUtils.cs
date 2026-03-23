using Silk.NET.Maths;

namespace BrainWithNoSkull.Util;

public static class ChunkUtils {

    public static Vector2D<int> ToChunkCoords(Vector3D<float> position){
        return ToChunkCoords(new Vector3D<int>((int)MathF.Floor(position.X), 0, (int)MathF.Floor(position.Z)));
    }
    
    public static Vector2D<int> ToChunkCoords(Vector3D<int> position){
        return new Vector2D<int>(position.X >> 4, position.Z >> 4);
    }
    
}