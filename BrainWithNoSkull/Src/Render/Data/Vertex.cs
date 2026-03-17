using Silk.NET.Maths;

namespace BrainWithNoSkull.Render.Data;

public struct Vertex {
    public Vector3D<float> Pos;
    public Vector3D<float> Color;

    public void Pack(float[] buffer, ref int index){
        buffer[index++] = Pos.X;
        buffer[index++] = Pos.Y;
        buffer[index++] = Pos.Z;
        buffer[index++] = Color.X;
        buffer[index++] = Color.Y;
        buffer[index++] = Color.Z;
    }
}