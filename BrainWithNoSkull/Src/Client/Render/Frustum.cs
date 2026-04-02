using BrainWithNoSkull.Util;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render;

public class Frustum {
    private Plane<float> _top;
    private Plane<float> _bottom;
    private Plane<float> _right;
    private Plane<float> _left;
    private Plane<float> _far;
    private Plane<float> _near;

    public Frustum(Camera camera){
        float halfVSide = camera.Far * MathF.Tan(Scalar.DegreesToRadians(camera.FoV) / 2f);
        float halfHSide = halfVSide * camera.AspectRatio;
        Vector3D<float> front = Vector3D.Normalize(camera.Target - camera.Position);
        Vector3D<float> right = Vector3D.Normalize(Vector3D.Cross(front, Vector3D<float>.UnitY));
        Vector3D<float> up = Vector3D.Normalize(Vector3D.Cross(right, front));
        Vector3D<float> frontMultFar = front * camera.Far;
        Vector3D<float> frontMultNear = front * camera.Near;

        _near = CreatePlane(camera.Position + frontMultNear, front);
        _far = CreatePlane(camera.Position + frontMultFar, -front);
        _right = CreatePlane(camera.Position, Vector3D.Cross(frontMultFar - right * halfHSide, up));
        _left = CreatePlane(camera.Position, Vector3D.Cross(up, frontMultFar + right * halfHSide));
        _top = CreatePlane(camera.Position, Vector3D.Cross(right, frontMultFar - up * halfVSide));
        _bottom = CreatePlane(camera.Position, Vector3D.Cross(frontMultFar + up * halfVSide, right));
    }

    private static Plane<float> CreatePlane(Vector3D<float> point, Vector3D<float> normal){
        normal = Vector3D.Normalize(normal);
        return new Plane<float>(normal, Vector3D.Dot(normal, point));
    }

    public bool IsBoxInside(AABB aabb){
        return
            IsOnOrForwardPlane(aabb, ref _left) && IsOnOrForwardPlane(aabb, ref _right) &&
            IsOnOrForwardPlane(aabb, ref _top) && IsOnOrForwardPlane(aabb, ref _bottom) &&
            IsOnOrForwardPlane(aabb, ref _near) && IsOnOrForwardPlane(aabb, ref _far);
    }

    private static bool IsOnOrForwardPlane(AABB aabb, ref Plane<float> plane){
        float rx = aabb.Width / 2f * MathF.Abs(plane.Normal.X);
        float ry = aabb.Height / 2f * MathF.Abs(plane.Normal.Y);
        float rz = aabb.Depth / 2f * MathF.Abs(plane.Normal.Z);
        float r = rx + ry + rz;
        return -r <= GetSignedDistanceToPlane(ref plane, aabb.Center);
    }

    private static float GetSignedDistanceToPlane(ref Plane<float> plane, Vector3D<float> center){
        return Vector3D.Dot(plane.Normal, center) - plane.Distance;
    }
    
}