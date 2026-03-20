using BrainWithNoSkull.Util;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render.Shader;

public struct TransformPushConstant {
    public const uint SizeInBytes = 4 * 4 * 1 * 4;
    
    public Matrix4X4<float> Transform;
    
    public float[] ToFloatArray(){
        float[] arr = new float[SizeInBytes / 4];
        Transform.CopyTo(arr, 0);
        return arr;
    }
}