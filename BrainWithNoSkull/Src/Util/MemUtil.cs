using System.Runtime.InteropServices;

namespace BrainWithNoSkull.Util;

public class MemUtil {

    public static byte[] StructToByteArray<T>(T obj) where T : struct{
        byte[] result = new byte[Marshal.SizeOf(obj)];
        IntPtr tempPtr = IntPtr.Zero;
        try {
            tempPtr = Marshal.AllocHGlobal(result.Length);
            Marshal.StructureToPtr(obj, tempPtr, false);
            Marshal.Copy(tempPtr, result, 0, result.Length);
        } finally {
            if (tempPtr != IntPtr.Zero) {
                Marshal.FreeHGlobal(tempPtr);
            }
        }
        return result;
    }
    
}