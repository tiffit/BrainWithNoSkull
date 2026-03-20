using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render.Data;

public class VertexBuilder {

    private readonly IRenderEngine _renderEngine;
    private readonly ShaderType _shaderProgram;
    private readonly LinkedList<Vertex> _vertices = [];

    public MatrixStack MatrixStack{ get; }

    public uint VerticesCount => (uint)_vertices.Count;

    public VertexBuilder(IRenderEngine renderEngine, ShaderType shader, MatrixStack matrixStack){
        _renderEngine = renderEngine;
        _shaderProgram = shader;
        MatrixStack = matrixStack;
    }

    public IndVertBuilder Pos(float x, float y, float z){
        return Pos(new Vector3D<float>(x, y, z));
    }
    
    public IndVertBuilder Pos(Vector3D<float> pos){
        Vector3D<float> transformed = MatrixStack.Transform(pos);
        return new IndVertBuilder(_vertices, transformed.X, transformed.Y, transformed.Z);
    }

    public ulong GetMeshSize(){
        return (ulong)(_vertices.Count * _shaderProgram.VertexSize * 4);
    }
    
    public float[] Pack(){
        float[] buffer = new float[_vertices.Count * _shaderProgram.VertexSize];
        int index = 0;
        foreach (Vertex v in _vertices) {
            v.Pack(buffer, ref index);
        }

        return buffer;
    }



}