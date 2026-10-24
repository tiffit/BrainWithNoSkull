using System.Numerics;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Util;

public enum Rotation {
    R0,
    R90,
    R180,
    R270
}

public record Direction(Vector3D<int> Offset, Rotation Rotation) {

    public static readonly Direction Up = new Direction(Vector3D<int>.UnitY, Rotation.R0);
    public static readonly Direction Down = new Direction(-Vector3D<int>.UnitY, Rotation.R0);
    
    public static readonly Direction North = new Direction(Vector3D<int>.UnitX, Rotation.R0);
    public static readonly Direction South = new Direction(-Vector3D<int>.UnitX, Rotation.R180);
    
    public static readonly Direction East = new Direction(Vector3D<int>.UnitZ, Rotation.R90);
    public static readonly Direction West = new Direction(-Vector3D<int>.UnitZ, Rotation.R270);

    public static readonly Direction[] All = [Up, Down, North, South, East, West];
    public static readonly Direction[] Horizontal = [North, South, East, West];
    
    
    public static readonly int[] Opposite = [1, 0, 3, 2, 5, 4];

    private static int _nextIndex;
    public readonly int Index = _nextIndex++;

    public Direction RotateHorizontal(Rotation rotation){
        if (this == Up || this == Down || rotation == Rotation.R0) {
            return this;
        }

        if (this == North) {
            switch (rotation) {
                case Rotation.R90:
                    return East;
                case Rotation.R180:
                    return South;
                case Rotation.R270:
                    return West;
            }
        }
        if (this == South) {
            switch (rotation) {
                case Rotation.R90:
                    return West;
                case Rotation.R180:
                    return North;
                case Rotation.R270:
                    return East;
            }
        }
        if (this == East) {
            switch (rotation) {
                case Rotation.R90:
                    return South;
                case Rotation.R180:
                    return West;
                case Rotation.R270:
                    return North;
            }
        }
        if (this == West) {
            switch (rotation) {
                case Rotation.R90:
                    return North;
                case Rotation.R180:
                    return East;
                case Rotation.R270:
                    return South;
            }
        }
        return this;
    }

    public Direction GetOpposite(){
        return All[Opposite[Index]];
    }
    
    public static void RotateAround<T>(Rotation rotation, ref T x, ref T z) where T: INumber<T>{
        switch (rotation) {
            case Rotation.R0:
                break;
            case Rotation.R90:
                (x, z) = (z, -x);
                break;
            case Rotation.R180:
                (x, z) = (-x, -z);
                break;
            case Rotation.R270:
                (x, z) = (-z, x);
                break;
        }
    }

    public static Direction FromAngle(float angle){
        angle = (angle / (MathF.PI * 2)) * 360f;
        int i = (int)MathF.Floor((angle * 4.0F / 360.0F) + 0.5F) & 3;
        return i switch{
            0 => West,
            1 => North,
            2 => East,
            _ => South,
        };
    }
}