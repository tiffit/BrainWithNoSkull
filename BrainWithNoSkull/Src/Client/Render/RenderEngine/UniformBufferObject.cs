using BrainWithNoSkull.Util;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render.RenderEngine;

public struct UniformBufferObject {
    public const ulong SizeInBytes = 4 * 4 * 3 * 4;
    
    public Matrix4X4<float> Model;
    public Matrix4X4<float> View;
    public Matrix4X4<float> Projection;

    public float[] ToFloatArray(){
        float[] arr = new float[SizeInBytes / 4];
        Model.CopyTo(arr, 0);
        View.CopyTo(arr, 16);
        Projection.CopyTo(arr, 32);
        return arr;
    }
}