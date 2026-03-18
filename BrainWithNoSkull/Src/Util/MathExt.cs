using Silk.NET.Maths;

namespace BrainWithNoSkull.Util;

public static class MathExt {

    public static Vector4D<T> Clone<T>(this Vector4D<T> vec) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>{
        return new Vector4D<T>(vec.X, vec.Y, vec.Z, vec.W);
    }
    
    public static Matrix4X4<T> Clone<T>(this Matrix4X4<T> matrix) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>{
        return new Matrix4X4<T>(matrix.Row1.Clone(), matrix.Row2.Clone(), matrix.Row3.Clone(), matrix.Row4.Clone());
    }
    
    public static void CopyTo<T>(this Matrix4X4<T> matrix, T[] arr, int offset) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>{
        matrix.Row1.CopyTo(arr, offset);
        matrix.Row2.CopyTo(arr, offset + 4);
        matrix.Row3.CopyTo(arr, offset + 8);
        matrix.Row4.CopyTo(arr, offset + 12);
    }
}