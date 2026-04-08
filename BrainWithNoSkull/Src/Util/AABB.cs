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

    public bool Intersects(AABB other){
        return MinX < other.MaxX && MaxX > other.MinX &&
               MinY < other.MaxY && MaxY > other.MinY &&
               MinZ < other.MaxZ && MaxZ > other.MinZ;
    }

    public static AABB FromExtents(float minX, float minY, float minZ, float width, float height, float depth){
        return new AABB(minX, minY, minZ, minX + width, minY + height, minZ + depth);
    }
    
    public static AABB ContainsBoth(AABB first, AABB second){
        return new AABB(MathF.Min(first.MinX, second.MinX), MathF.Min(first.MinY, second.MinY), MathF.Min(first.MinZ, second.MinZ),
            MathF.Max(first.MaxX, second.MaxX), MathF.Max(first.MaxY, second.MaxY), MathF.Max(first.MaxZ, second.MaxZ));
    }

    public static AABB And(AABB first, AABB second){
        return new AABB(MathF.Max(first.MinX, second.MinX), MathF.Max(first.MinY, second.MinY), MathF.Max(first.MinZ, second.MinZ),
            MathF.Min(first.MaxX, second.MaxX), MathF.Min(first.MaxY, second.MaxY), MathF.Min(first.MaxZ, second.MaxZ));
    }
    
    public static AABB operator +(AABB left, Vector3D<float> right){
        return new AABB(left.MinX + right.X, left.MinY + right.Y, left.MinZ + right.Z,
            left.MaxX + right.X, left.MaxY + right.Y, left.MaxZ + right.Z);
    }
    
    public static AABB operator +(AABB left, Vector3D<int> right){
        return new AABB(left.MinX + right.X, left.MinY + right.Y, left.MinZ + right.Z,
            left.MaxX + right.X, left.MaxY + right.Y, left.MaxZ + right.Z);
    }
}