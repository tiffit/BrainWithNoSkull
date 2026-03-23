using Silk.NET.Maths;

namespace BrainWithNoSkull.Util;

// ReSharper disable once InconsistentNaming
public class AABB {
    public float MinX, MinY, MinZ;
    public float MaxX, MaxY, MaxZ;

    public float Width => MaxX - MinX;
    public float Height => MaxY - MinY;
    public float Depth => MaxZ - MinZ;

    public Vector3D<float> Center => new Vector3D<float>(MinX + Width / 2f, MinY + Height / 2f, MinZ + Depth / 2f);

    public AABB(float minX, float minY, float minZ, float maxX, float maxY, float maxZ){
        MinX = minX;
        MinY = minY;
        MinZ = minZ;
        MaxX = maxX;
        MaxY = maxY;
        MaxZ = maxZ;
    }

    public AABB(Vector3D<float> min, Vector3D<float> max) : this(min.X, min.Y, min.Z, max.X, max.Y, max.Z){}

    public static AABB FromExtents(float minX, float minY, float minZ, float width, float height, float depth){
        return new AABB(minX, minY, minZ, minX + width, minY + height, minZ + depth);
    }
}