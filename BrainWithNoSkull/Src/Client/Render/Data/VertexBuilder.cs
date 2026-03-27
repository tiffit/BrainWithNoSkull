using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Client.Render.Shader;
using Silk.NET.Maths;
using Silk.NET.Vulkan;

namespace BrainWithNoSkull.Client.Render.Data;

public class VertexBuilder {

    private readonly IRenderEngine _renderEngine;
    private readonly ShaderType _shaderProgram;
    private readonly LinkedList<Vertex> _vertices = [];

    public byte[]? PushConstants;
    public ShaderStageFlags PushConstantStages = ShaderStageFlags.None;
    
    public uint VerticesCount => (uint)_vertices.Count;

    public VertexBuilder(IRenderEngine renderEngine, ShaderType shader){
        _renderEngine = renderEngine;
        _shaderProgram = shader;
    }

    public IndVertBuilder Pos(float x, float y, float z){
        return Pos(new Vector3D<float>(x, y, z));
    }
    
    public IndVertBuilder Pos(Vector3D<float> pos){
        return new IndVertBuilder(_vertices, pos.X, pos.Y, pos.Z);
    }

    public IndVertBuilder Pos(Vector3D<float> pos, MatrixStack stack){
        return Pos(stack.Transform(pos));
    }

    public void AddAll(IEnumerable<Vertex> vertices){
        foreach (Vertex v in vertices) {
            _vertices.AddLast(v);
        }
    }
    
    public ulong GetMeshSize(){
        return (ulong)(_vertices.Count * _shaderProgram.VertexSize * 4);
    }
    
    public float[] Pack(){
        float[] buffer = new float[_vertices.Count * _shaderProgram.VertexSize];
        int index = 0;
        HashSet<VertexParam> vParams = new HashSet<VertexParam>(_shaderProgram.Params);
        foreach (Vertex v in _vertices) {
            v.Pack(buffer, ref index, vParams);
        }

        return buffer;
    }



}