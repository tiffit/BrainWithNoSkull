using System.Runtime.InteropServices;
using BrainWithNoSkull.Util;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render.Shader.PushConstants;

public struct TransformPushConstant {
    public const uint SizeInBytes = 4 * 4 * 1 * 4;
    
    public Matrix4X4<float> Transform;
    
    public float[] ToFloatArray(){
        float[] arr = new float[SizeInBytes / 4];
        Transform.CopyTo(arr, 0);
        return arr;
    }
}

public struct TransformPushConstantShadow{
    public const uint SizeInBytes = TransformPushConstant.SizeInBytes + 4;
    public TransformPushConstant Transform;
    public uint CascadeIndex;
    
    public float[] ToFloatArray(){
        float[] arr = new float[SizeInBytes / 4];
        Transform.Transform.CopyTo(arr, 0);
        arr[^1] = BitConverter.UInt32BitsToSingle(CascadeIndex);
        return arr;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct SkyboxPushConstant {
    public Vector4D<float> LightData;
}