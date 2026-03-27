using BrainWithNoSkull.Client.Render.Shader;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render.Data;

public struct Vertex {
    public Vector3D<float> Pos;
    public Vector3D<float> Color;
    public Vector2D<float> Uv;
    public Vector3D<float> Normal;
    public uint TextureIndex;

    public void Pack(float[] buffer, ref int index, HashSet<VertexParam> vParams){
        if (vParams.Contains(VertexParam.Position)) {
            buffer[index++] = Pos.X;
            buffer[index++] = Pos.Y;
            buffer[index++] = Pos.Z;
        }

        if (vParams.Contains(VertexParam.Color)) {
            buffer[index++] = Color.X;
            buffer[index++] = Color.Y;
            buffer[index++] = Color.Z;
        }

        if (vParams.Contains(VertexParam.Uv)) {
            buffer[index++] = Uv.X;
            buffer[index++] = Uv.Y;
        }

        if (vParams.Contains(VertexParam.Normal)) {
            buffer[index++] = Normal.X;
            buffer[index++] = Normal.Y;
            buffer[index++] = Normal.Z;
        }
        
        if (vParams.Contains(VertexParam.TextureIndex)) {
            unsafe {
                uint textureIndex = TextureIndex;
                uint* ptr = &textureIndex;
                buffer[index++] = *(float*)ptr;
            }
        }
    }
}