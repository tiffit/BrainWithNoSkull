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
        CopyToFloatArray(arr, 0);
        return arr;
    }
    public void CopyToFloatArray(float[] arr, int index){
        Model.CopyTo(arr, index);
        View.CopyTo(arr, index + 16);
        Projection.CopyTo(arr, index + 32);
    }
}

public struct UniformBufferObjectArray {
    public const ulong SizeInBytes = VkRenderEngine.ShadowMapCascadeCount * UniformBufferObject.SizeInBytes;
    
    public UniformBufferObject[] CascadeUbos;
    
    public float[] ToFloatArray(){
        float[] arr = new float[SizeInBytes / 4];
        for (int i = 0; i < VkRenderEngine.ShadowMapCascadeCount; i++) {
            CascadeUbos[i].CopyToFloatArray(arr, i*(int)UniformBufferObject.SizeInBytes/4);
        }
        return arr;
    }
}

public struct ShadowMapData {
    public const ulong SizeInBytes = (4 + 3) * 4;
    public Vector4D<float> CascadeSplits;
    public Vector3D<float> LightDir;
    
    public float[] ToFloatArray(){
        float[] arr = new float[SizeInBytes / 4];
        CascadeSplits.CopyTo(arr, 0);
        LightDir.CopyTo(arr, 4);
        return arr;
    }
}