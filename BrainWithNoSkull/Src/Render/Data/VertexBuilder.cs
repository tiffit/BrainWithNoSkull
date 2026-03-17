using BrainWithNoSkull.Render.RenderEngine;
using BrainWithNoSkull.Render.Shader;

namespace BrainWithNoSkull.Render.Data;

public class VertexBuilder {

    private readonly IRenderEngine _renderEngine;
    private readonly ShaderDetails _shaderProgram;
    private readonly LinkedList<Vertex> _vertices = [];
    
    public VertexBuilder(IRenderEngine renderEngine, ShaderDetails shader){
        _renderEngine = renderEngine;
        _shaderProgram = shader;
    }

    public IndVertBuilder Pos(float x, float y, float z){
        return new IndVertBuilder(_vertices, x, y, z);
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