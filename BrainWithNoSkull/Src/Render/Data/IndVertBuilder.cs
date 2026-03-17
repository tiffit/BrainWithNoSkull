using Silk.NET.Maths;

namespace BrainWithNoSkull.Render.Data;

public class IndVertBuilder {

    private readonly LinkedList<Vertex> _vertices;
    
    private Vector3D<float> _pos;
    private Vector3D<float> _color;

    public IndVertBuilder(LinkedList<Vertex> vertices, float x, float y, float z){
        _pos = new Vector3D<float>(x, y, z);
        _vertices = vertices;
    }

    public IndVertBuilder Color(float r, float g, float b){
        _color = new Vector3D<float>(r, g, b);
        return this;
    }

    public void Build(){
        Vertex v = new Vertex {
            Pos = _pos,
            Color = _color
        };
        _vertices.AddLast(v);
    }

}