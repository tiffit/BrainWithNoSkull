using Silk.NET.Maths;

namespace BrainWithNoSkull.Util;

public record Direction(Vector3D<int> Offset) {

    public static readonly Direction Up = new Direction(Vector3D<int>.UnitY);
    public static readonly Direction Down = new Direction(-Vector3D<int>.UnitY);
    
    public static readonly Direction North = new Direction(Vector3D<int>.UnitX);
    public static readonly Direction South = new Direction(-Vector3D<int>.UnitX);
    
    public static readonly Direction East = new Direction(Vector3D<int>.UnitZ);
    public static readonly Direction West = new Direction(-Vector3D<int>.UnitZ);

    public static readonly Direction[] All = [Up, Down, North, South, East, West];

    private static int _nextIndex;
    public readonly int Index = _nextIndex++;
    
    public static void RotateAroundY(Direction dir, ref int x, ref int z){
        if (dir == South) {
            x = -x;
        }
        if (dir == East) {
            (x, z) = (z, x);
        }
        if (dir == West) {
            (x, z) = (z, -x);
        }
    }
}