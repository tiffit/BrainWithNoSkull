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
    
    public Frustum(Vector3D<float> lightPos, Vector3D<float> lightTarget, float radius){
        Vector3D<float> front = Vector3D.Normalize(lightTarget - lightPos);

        Vector3D<float> worldUp = Vector3D<float>.UnitY;
        if (MathF.Abs(front.Y) > 0.999f) {
            worldUp = Vector3D<float>.UnitZ;
        }
        
        Vector3D<float> right = Vector3D.Normalize(Vector3D.Cross(front, worldUp));
        Vector3D<float> up = Vector3D.Normalize(Vector3D.Cross(right, front));

        _near = CreatePlane(lightPos, front);
        _far = CreatePlane(lightPos + front * (radius * 2), -front);
        _right = CreatePlane(lightPos + right * radius, -right);
        _left = CreatePlane(lightPos - right * radius, right);
        _top = CreatePlane(lightPos + up * radius, -up);
        _bottom = CreatePlane(lightPos - up * radius, up);
    }

    private static Plane<float> CreatePlane(Vector3D<float> point, Vector3D<float> normal){
        normal = Vector3D.Normalize(normal);
        return new Plane<float>(normal, Vector3D.Dot(normal, point));
    }

    public bool IsBoxInside(AABB aabb){
        return
            IsOnOrForwardPlane(aabb, in _left) && IsOnOrForwardPlane(aabb, in _right) &&
            IsOnOrForwardPlane(aabb, in _top) && IsOnOrForwardPlane(aabb, in _bottom) &&
            IsOnOrForwardPlane(aabb, in _near) && IsOnOrForwardPlane(aabb, in _far);
    }

    private static bool IsOnOrForwardPlane(AABB aabb, in Plane<float> plane){
        float rx = aabb.Width / 2f * MathF.Abs(plane.Normal.X);
        float ry = aabb.Height / 2f * MathF.Abs(plane.Normal.Y);
        float rz = aabb.Depth / 2f * MathF.Abs(plane.Normal.Z);
        float r = rx + ry + rz;
        return -r <= GetSignedDistanceToPlane(in plane, aabb.Center);
    }

    private static float GetSignedDistanceToPlane(in Plane<float> plane, Vector3D<float> center){
        return Vector3D.Dot(plane.Normal, center) - plane.Distance;
    }
    
}