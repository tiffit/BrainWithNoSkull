using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Render.Data;

public class VertexBuilder {

    private readonly IRenderEngine _renderEngine;
    private readonly ShaderDetails _shaderProgram;
    private readonly LinkedList<Vertex> _vertices = [];
    private readonly MatrixStack _matrixStack;

    public uint VerticesCount => (uint)_vertices.Count;

    public VertexBuilder(IRenderEngine renderEngine, ShaderDetails shader, MatrixStack matrixStack){
        _renderEngine = renderEngine;
        _shaderProgram = shader;
        _matrixStack = matrixStack;
    }

    public IndVertBuilder Pos(float x, float y, float z){
        Vector3D<float> transformed = _matrixStack.Transform(new Vector3D<float>(x, y, z));
        return new IndVertBuilder(_vertices, transformed.X, transformed.Y, transformed.Z);
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